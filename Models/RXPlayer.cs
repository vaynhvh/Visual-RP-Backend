using Backend.Controllers;
using Backend.Models.Appearance;
using Backend.Modules.Animations;
using Backend.Modules.Attachment;
using Backend.Modules.Bank;
using Backend.Modules.Commands;
using Backend.Modules.Crime;
using Backend.Modules.Faction;
using Backend.Modules.Gangwar;
using Backend.Modules.Inventory;
using Backend.Modules.MemberData;
using Backend.Modules.Native;
using Backend.Modules.Phone.Apps;
using Backend.Modules.XMAS;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Models
{
    public enum Icon
    {
        Admin,
        LSPD,
        Marriage,
        Dev,
        Events,
        WN,
        Drop,
        LSMC,
        Army
    }

    public enum ReactionGameLevel
    {
        Easy,
        Normal,
        Hard
    }

    public class WeaponLoadoutItem
    {
        public string WeaponHash { get; set; }
        public int Ammo { get; set; }

        public bool BWeapon { get; set; }
    }

    public class AnimationSyncItem
    {
        public bool Active { get; set; }
        public string AnimationDict { get; set; }
        public string AnimationName { get; set; }
        public int AnimationFlags { get; set; }
        public float AnimationSpeed { get; set; }
        public float Heading { get; set; }

        public AnimationSyncItem(bool active, string animationDict, string animationName, int animationFlags,
            float animationSpeed, float heading)
        {
            Active = active;
            AnimationDict = animationDict;
            AnimationName = animationName;
            AnimationFlags = animationFlags;
            AnimationSpeed = animationSpeed;
            Heading = heading;
        }

        public AnimationSyncItem(RXPlayer iPlayer)
        {
            Active = iPlayer.PlayingAnimation;
            AnimationDict = iPlayer.AnimationDict;
            AnimationFlags = iPlayer.CurrentAnimFlags;
            AnimationName = iPlayer.AnimationName;
            AnimationSpeed = iPlayer.AnimationSpeed;
            Heading = iPlayer.Heading;
        }
    }

    public static class PlayerDatas
    {
        public const string CuffedMedic = "CuffedMedic";
        public const string AdminDutyEvent = "updateAduty";
        public const string CuffedEvent = "updateCuffed";
        public const string TiedEvent = "updateTied";
        public const string DutyEvent = "updateDuty";
    }

    public enum FunkStatus
    {
        Deactive = 0,
        Hearing = 1,
        Active = 2
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class RXPlayer : Player
    {
        //[HandleExceptions]
        public async Task<bool> GetExistsAsync() => await NAPI.Task.RunReturnAsync(() => this.Exists);

        public Dictionary<uint, uint> AnimationShortcuts { get; set; } = new Dictionary<uint, uint>();

        public bool PlayingAnimation { get; set; } = false;
        public int CurrentAnimFlags { get; set; } = 0;
        public int ForumId { get; set; }
        public uint Phone { get; set; }
        public string DiscordLoginHash { get; set; } = "";
        public string AnimationDict { get; set; } = "";
        public string AnimationName { get; set; } = "";
        public float AnimationSpeed { get; set; } = 8f;

        public string VoiceHash { get; set; } = "";
        public string LastKiller { get; set; } = "";
        public string LastKillerWeapon { get; set; } = "";

        public int VDMCounter { get; set; } = 0;

        public int Joints { get; set; } = 0;
        public DateTime LastJoint { get; set; } = DateTime.MinValue;

        public bool IsCrouched { get; set; } = false;

        public GangwarWeaponPack gangwarWeaponPack { get; set; }

        private int _cash { get; set; }
        private int _stress { get; set; }
        private int _sport { get; set; }
        private int _hunger { get; set; }
        private int _thirst { get; set; }
        private int _playerteam { get; set; }
        private int _blackcash { get; set; }
        private bool _freezed { get; set; } = false;
        private bool _invincible { get; set; } = false;
        private bool _invisible { get; set; } = false;
        private bool _collision { get; set; } = true;
        private double _frequency { get; set; } = 0;
        public uint AntiFlags { get; set; } = 0;
        public bool IsLoggedIn { get; set; } = false;
        public bool IsCuffed { get; set; } = false;
        public bool IsTied { get; set; } = false;
        public bool IsInvDisabled { get; set; } = false;
        public bool IsTaskAllowed { get; set; } = true;
        public bool DraggingItem { get; set; } = false;
        public Dictionary<int, DbAttachment> Attachments { get; set; }
        public int DraggingTime { get; set; } = 0;
        public bool InAduty { get; set; } = false;
        public bool InGduty { get; set; } = false;

        public string WalletAdress { get; set; } = "";
        public double WalletValue { get; set; } = 0;
        public bool inPaintball { get; set; } = false;
        public uint FFAKillStreak { get; set; } = 0;
        public uint Paintballkills { get; set; } = 0;
        public uint Paintballdeaths { get; set; } = 0;
        public uint GangwarKills { get; set; } = 0;
        public uint GangwarDeaths { get; set; } = 0;

        public bool OnWayToKH { get; set; } = false;

        public NativeMenu Menu { get; set; }

        //[HandleExceptions]
        public async Task<WeaponHash> GetCurrentWeaponAsync() => await NAPI.Task.RunReturnAsync(() => this.CurrentWeapon);

        //[HandleExceptions]
        public async Task<bool> GetIsInVehicleAsync() => await NAPI.Task.RunReturnAsync(() => this.IsInVehicle);

        //[HandleExceptions]
        public async Task<int> GetArmorAsync() => await NAPI.Task.RunReturnAsync(() => this.Armor);

        //[HandleExceptions]
        public async Task SetArmorAsync(int armor) => await NAPI.Task.RunAsync(() => { this.Armor = armor;});

        //[HandleExceptions]
        public async Task<int> GetHealthAsync() => await NAPI.Task.RunReturnAsync(() => this.Health);

        //[HandleExceptions]
        public async Task SetHealthAsync(int health) => await NAPI.Task.RunAsync(() => { this.Health = health; });

        //[HandleExceptions]
        public async Task<uint> GetDimensionAsync() => await NAPI.Task.RunReturnAsync(() => this.Dimension);

        //[HandleExceptions]
        public async Task SetDimensionAsync(uint dimension) => await NAPI.Task.RunAsync(() => { this.Dimension = dimension; });

        //[HandleExceptions]
        public async Task<Vector3> GetPositionAsync() => await NAPI.Task.RunReturnAsync(() => this.Position);
        public async Task<Vector3> GetVelocityAsync() => await NAPI.Task.RunReturnAsync(() => this.Velocity);

        //[HandleExceptions]
        public async Task SetPositionAsync(Vector3 position) => await NAPI.Task.RunAsync(() => { this.Position = position; });

        //[HandleExceptions]
        public async Task<float> GetHeadingAsync() => await NAPI.Task.RunReturnAsync(() => this.Heading);

        //[HandleExceptions]
        public async Task SetHeadingAsync(float heading) => await NAPI.Task.RunAsync(() => this.Heading = heading);

        //[HandleExceptions]
        public async Task<string> GetNameAsync() => await NAPI.Task.RunReturnAsync(() => this.Name);
        public async Task<int> GetVehicleSeatAsync() => await NAPI.Task.RunReturnAsync(() => this.VehicleSeat);

        //[HandleExceptions]
        public async Task SetNameAsync(string name) => await NAPI.Task.RunAsync(() => this.Name = name);

        //[HandleExceptions]
        public async Task<string> GetSocialNameAsync() => await NAPI.Task.RunReturnAsync(() => this.SocialClubName);
        public async Task<ulong> GetSocialIdAsync() => await NAPI.Task.RunReturnAsync(() => this.SocialClubId);


        //[HandleExceptions]
        public async Task<string> GetAddressAsync() => await NAPI.Task.RunReturnAsync(() => this.Address);

        //[HandleExceptions]
        public async Task<RXVehicle> GetVehicleAsync() => await NAPI.Task.RunReturnAsync(() => (RXVehicle)this.Vehicle);
        public async Task<string> GetSerialAsync() => await NAPI.Task.RunReturnAsync(() => this.Serial);
        public async Task<string> GetPayTimeAsync() => await NAPI.Task.RunReturnAsync(() => TimeSpan.FromMinutes(this.Paytime).ToString(@"hh\:mm"));

        public RXPayment AwaitingPayment { get; set; } = null;

        public uint Id { get; set; }
        public NetHandle PlayerHandle { get; set; }
        public List<string> PlayerKills { get; set; } = new List<string>();
        public bool IsSpectate { get; set; } = false;
        public string DiscordID { get; set; }
        public string ClientHash { get; set; } = "";
        public DateTime LastSeen { get; set; } = DateTime.Now;

        public int Paytime { get; set; } = 180;


        //[HandleExceptions]
        public int Cash
        {
            get
            {
                return _cash;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("updateMoney", value));
                _cash = value;
            }
        }


        public int Stress
        {
            get
            {
                return _stress;
            }
            set
            {



                NAPI.Task.Run(() => this.TriggerEvent("updateStress", value));
                _stress = value;
            }
        }
        public int Sport
        {
            get
            {
                return _sport;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("updateSport", value));
                _sport = value;

                if (_sport > 100)
                {
                    _sport = 100;
                } else if (_sport < 1)
                {
                    _sport = 0;
                }

            }
        }

        public int Hunger
        {
            get
            {
                return _hunger;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("updateHunger", value));
                _hunger = value;

                if (_hunger > 100)
                {
                    _hunger = 100;
                }
                else if (_hunger < 1)
                {
                    _hunger = 0;
                }
            }
        }

        public int Thirst
        {
            get
            {
                return _thirst;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("updateThirst", value));
                _thirst = value;

                if (_thirst > 100)
                {
                    _thirst = 100;
                }
                else if (_thirst < 1)
                {
                    _thirst = 0;
                }
            }
        }

        //[HandleExceptions]
        public int Blackmoney
        {
            get
            {
                return _blackcash;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("updateBlackMoney", value));
                _blackcash = value;
            }
        }

        //[HandleExceptions]
        public bool Freezed
        {
            get
            {
                return _freezed;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("freezePlayer", value));
                _freezed = value;
            }
        }

        private bool _trainingsduty = false;

        public bool TrainingsDuty
        {
            get
            {
                return _trainingsduty;
            }
            set
            {
                NAPI.Task.Run(() => this.TriggerEvent("toggleTrainingDuty", value));
                _trainingsduty = value;
            }
        }



        //[HandleExceptions]
        public bool Seatbelt
        {
            get
            {
                if (!NAPI.Task.RunReturn(() => this.HasSharedData("Seatbelt"))) return false;

                return NAPI.Task.RunReturn(() => this.GetSharedData<bool>("Seatbelt"));
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("Seatbelt", value));
            }
        }

        //[HandleExceptions]
        public double Frequency
        {
            get
            {

                return _frequency;
            }
            set
            {
                _frequency = value;
            }
        }

        public int PaintballTeam
        {
            get
            {
                if (NAPI.Task.RunReturn(() => !this.HasSharedData("PaintballTeam"))) return 0;

                return NAPI.Task.RunReturn(() => this.GetSharedData<int>("PaintballTeam"));
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("PaintballTeam", (int)value));
            }
        }



        //[HandleExceptions]
        public bool Invincible
        {
            get
            {
                return _invincible;
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("Invincible", value));
                _invincible = value;
            }
        }

        //[HandleExceptions]
        public bool Invisible
        {
            get
            {
                return _invisible;
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("Invisible", value));
                _invisible = value;
            }
        }

        public bool Collision
        {
            get
            {
                return _collision;
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("Collision", value));
                _collision = value;
            }
        }


        public RXRank Rank { get; set; } = new RXRank
        {
            Id = 0,
            ClothesId = 0,
            Name = "Spieler",
            Permission = 0
        };
        public uint TeamId { get; set; } = 0;
        public uint Teamrank { get; set; } = 0;
        public uint InventoryId { get; set; } = 0;
        public bool InDuty { get; set; } = false;
        public uint Jailtime { get; set; } = 0;
        public uint GangwarContainerId { get; set; } = 0;
        public uint WorkstationId { get; set; } = 0;

        public bool WorkstationRunning { get; set; } = false;
        public uint WorkstationInputContainerId { get; set; } = 0;
        public uint WorkstationOutputContainerId { get; set; } = 0;
        public uint LabInputContainerId { get; set; } = 0;
        public uint LabOutputContainerId { get; set; } = 0;
        public uint BankAccountId { get; set; } = 0;
        public uint HouseId { get; set; } = 0;
        public string DateOfEntry { get; set; } = "00.00.0000";
        public DateTime OnlineSince { get; set; } = DateTime.Now;
        public DateTime LuckyWheel { get; set; } = DateTime.Now.AddDays(-1);
        public int Warns { get; set; } = 0;
        public List<WeaponLoadoutItem> Weapons { get; set; } = new List<WeaponLoadoutItem>();
        public List<uint> Storages { get; set; } = new List<uint>();
        public RXDeathData DeathData { get; set; }
        public GTANetworkAPI.Object DeathProp { get; set; } = null;
        public bool Injured { get; set; } = false;
        public bool IsCarried { get; set; } = false;
        public bool IsCarry { get; set; } = false;
        public bool Coma { get; set; } = false;
        public int Level { get; set; } = 0;
        public bool IsMale { get; set; } = true;
        public bool IsInRob { get; set; } = false;

        public List<uint> FunkFav { get; set; } = new List<uint>();
        public DateTime LastInteracted { get; set; } = DateTime.Now;

        public int Flags = 0;

        //[HandleExceptions]
        public RXContainerObj Container
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.InventoryId);
        }
        public RXContainerObj GangwarContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.GangwarContainerId);
        }
        public RXContainerObj WorkstationInputContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.WorkstationInputContainerId);
        }
        public RXContainerObj WorkstationOutputContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.WorkstationOutputContainerId);
        }

        public RXContainerObj LabInputContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.LabInputContainerId);
        }
        public RXContainerObj LabOutputContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.LabOutputContainerId);
        }

        //[HandleExceptions]
        public RXTeam Team
        {
            get => TeamModule.Teams.FirstOrDefault(x => x.Id == TeamId);
        }

        //[HandleExceptions]
        public DbBankAccount BankAccount
        {
            get => BankModule.BankAccounts.FirstOrDefault(x => x.Id == this.BankAccountId);
        }

        //[HandleExceptions]
        public DbBusinessMemberData BusinessMemberData
        {
            get => MemberDataModule.BusinessMemberDatas.FirstOrDefault(x => x.PlayerId == this.Id);
        }

        //[HandleExceptions]
        public DbTeamMemberData TeamMemberData
        {
            get => MemberDataModule.TeamMemberDatas.FirstOrDefault(x => x.PlayerId == this.Id);
        }

        //[HandleExceptions]
        public DbPhoneSettings PhoneSettings
        {
            get => SettingsApp.PhoneSettings.FirstOrDefault(x => x.PlayerId == this.Id);
        }

        public DbPlayerCrimeData PlayerCrimeData
        {
            get => CrimeModule.PlayerCrimeData.FirstOrDefault(x => x.PlayerId == this.Id);
        }
        public List<DbPlayerCrimes> PlayerCrimes
        {
            get => CrimeModule.PlayerCrimes.Where(x => x.PlayerId == this.Id).ToList();
        }
        public List<DbPlayerXMAS> PlayerGifts
        {
            get => XMASModule.OpenGifts.Where(x => x.ForumId == this.ForumId).ToList();
        }

        public RXPlayer(NetHandle handle) : base(handle)
        {
            PlayerHandle = handle;
        }

        //[HandleExceptions]
        public async Task SendNotify(string text, int duration = 3500, string color = "", string title = "")
        {
            if (await NAPI.Task.RunReturnAsync(() => this.HasData("DisableNextNotify")))
            {
                await NAPI.Task.RunAsync(() => this.ResetData("DisableNextNotify"));
                return;
            }

            await this.TriggerEventAsync("sendPlayerNotification", text, duration, color, title, "");
        }

        //[HandleExceptions]
        public async Task SendGlobalNotify(string text, int duration = 8000, string color = "red", Icon icon = Icon.Admin)
        {

            int iconStr = 0;

            if (icon == Icon.Admin)
            {
                iconStr = 0;
            }
            if (icon == Icon.LSPD)
            {
                iconStr = 1;
            }
            if (icon == Icon.Marriage)
            {
                iconStr = 4;
            }
            if (icon == Icon.Dev)
            {
                iconStr = 2;
            }
            if (icon == Icon.Events)
            {
                iconStr = 5;
            }
            if (icon == Icon.WN)
            {
                iconStr = 6;
            }
            if (icon == Icon.Drop)
            {
                iconStr = 7;
            }
            if (icon == Icon.LSMC)
            {
                iconStr = 8;
            }
            if (icon == Icon.Army)
            {
                iconStr = 9;
            }

            //if (color == ) color = "#242424";

            await this.TriggerEventAsync("sendGlobalNotification", text, iconStr);
        }

        public async Task GivePTAPoints(string playername, string discordid, string reason, uint points)
        {
            using var db = new RXContext();

            var target = await db.Players.FirstOrDefaultAsync(x => x.Id == this.Id);

            if (target == null) return;

            target.PTAPoints += (int)points;
            await db.PTA.AddAsync(new DbPTA() { Date = DateTime.Now, Points = points, Reason = reason, Teamname = await this.GetNameAsync(), TeamDiscord = target.DiscordID, Username = playername, UserDiscord = discordid });

            await db.SaveChangesAsync();

            if (InAduty)
            {
                await SendNotify("Du hast " + points + " PTA Punkte erhalten! Begründung: " + reason);
            }
        }

        //[HandleExceptions]
        public async Task disableAllPlayerActions(bool val) => await this.TriggerEventAsync("disableAllPlayerActions", val);

        //[HandleExceptions]
        public async Task SetDefaultClothes()
        {
            await this.SetClothesAsync(1, 0, 0);
            await this.SetClothesAsync(11, 1, 0);
            await this.SetClothesAsync(8, 15, 0);
            await this.SetClothesAsync(3, 0, 0);
            await this.SetClothesAsync(4, 5, 0);
            await this.SetClothesAsync(6, 1, 0);
        }

        public async Task SyncAttachmentOnlyItems(RXPlayer dbPlayer)
        {
            if (dbPlayer.HasAttachmentOnlyItem(dbPlayer) && await dbPlayer.GetIsInVehicleAsync() && !dbPlayer.IsCuffed && !dbPlayer.IsTied && dbPlayer.CanInteract())
            {
                RXItem xItem = dbPlayer.Container.GetAttachmentOnlyItem();
                if (xItem != null)
                {
                    await AttachmentModule.RemoveAllAttachments(dbPlayer);
                    await AttachmentModule.AddAttachment(dbPlayer, xItem.Model.AttachmentOnlyId, true);
                }
            }
        }

        public bool HasAttachmentOnlyItem(RXPlayer dbPlayer)
        {
            if (dbPlayer != null && dbPlayer.Container != null)
            {
                RXItem xItem = dbPlayer.Container.GetAttachmentOnlyItem();
                if (xItem != null)
                {
                    return true;
                }
            }

            return false;
        }
        private static int GetHeadOverlayColor(Customization customization, int overlayId)
        {
            switch (overlayId)
            {
                case 1:
                    return customization.BeardColor;
                case 2:
                    return customization.EyebrowColor;
                case 5:
                    return customization.BlushColor;
                case 8:
                    return customization.LipstickColor;
                case 10:
                    return customization.ChestHairColor;
                default:
                    return 0;
            }
        }

        //[HandleExceptions]
        public async Task LoadCharacter(DbCharacter dbCharacter = null)
        {
            if (dbCharacter == null)
            {
                using var db = new RXContext();

                dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == this.Id);
                if (dbCharacter == null) return;
            }

            Customization customization = JsonConvert.DeserializeObject<Customization>(dbCharacter.Customization);

            var headBlend = new GTANetworkAPI.HeadBlend
            {
                ShapeFirst = (byte)customization.Parents.MotherShape,
                ShapeSecond = (byte)customization.Parents.FatherShape,
                ShapeThird = 0,
                SkinFirst = (byte)customization.Parents.MotherSkin,
                SkinSecond = (byte)customization.Parents.FatherSkin,
                SkinThird = 0,
                ShapeMix = (byte)customization.Parents.Similarity,
                SkinMix = (byte)customization.Parents.SkinSimilarity,
                ThirdMix = 0
            };


            var headOverlays = new Dictionary<int, GTANetworkAPI.HeadOverlay>(customization.Appearance.Count);

            for (int i = 0, length = customization.Appearance.Count; i < length; i++)
            {
                headOverlays[i] = new GTANetworkAPI.HeadOverlay
                {
                    Index = (byte)customization.Appearance[i].Value,
                    Opacity = customization.Appearance[i].Opacity,
                    Color = (byte)GetHeadOverlayColor(customization, i)
                };
            }

            List<Decoration> decorations = new List<Decoration>();

            // this.HeadBlend = headBlend;
            await this.SetCustomizationAsync(
            customization.Gender == 0, headBlend, (byte)customization.EyeColor,
            (byte)customization.Hair.Color, (byte)customization.Hair.HighlightColor,
                customization.Features, headOverlays, decorations.ToArray());
            await this.SetClothesAsync(2, customization.Hair.Hair, 0);

            int feature_id = 0;

            await customization.Features.forEach(async f =>
            {
                await this.SetFaceFeatureAsync(feature_id, f);
                feature_id++;
            });

            await this.SetDefaultClothes();

            Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes);
            Dictionary<int, RXClothesProp> propParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories);

            await clothesParts.forEach(async cp
                =>
            {

                if (cp.Value.active == null)
                {
                    cp.Value.active = true;
                }
                if (cp.Value.active)
                {
                    await this.SetClothesAsync(cp.Key, cp.Value.drawable, cp.Value.texture);
                }
                else
                {
                    int cc = cp.Key;
                    if (cc == 6)
                    {
                        if (!dbCharacter.Gender)
                        {
                            await this.SetClothesAsync(cc, 35, 0);
                        }
                        else
                        {
                            await this.SetClothesAsync(cc, 34, 0);
                        }
                    }
                    else if (cc == 11)
                    {
                        if (!dbCharacter.Gender)
                        {
                            await this.SetClothesAsync(cc, 5, 0);
                        }
                        else
                        {
                            await this.SetClothesAsync(cc, 15, 0);
                        }
                    }
                    else if (cc == 7)
                    {
                        if (!dbCharacter.Gender)
                        {
                            await this.SetClothesAsync(cc, 0, 0);
                        }
                        else
                        {
                            await this.SetClothesAsync(cc, 0, 0);
                        }
                    }
                    else if (cc == 4)
                    {
                        if (!dbCharacter.Gender)
                        {
                            await this.SetClothesAsync(cc, 15, 0);
                        }
                        else
                        {
                            await this.SetClothesAsync(cc, 21, 0);
                        }
                    }
                    else if (cc == 3)
                    {
                        if (!dbCharacter.Gender)
                        {
                            await this.SetClothesAsync(cc, 15, 0);
                        }
                        else
                        {
                            await this.SetClothesAsync(cc, 15, 0);
                        }
                    }
                    else
                    {
                        await this.SetClothesAsync(cc, 0, 0);

                    }
                }
            });


            await propParts.forEach(async pp
                =>
            {
                if (pp.Value.active == null)
                {
                    pp.Value.active = true;
                }
                if (pp.Value.active)
                {

                    await this.SetAccessoriesAsync(pp.Key, pp.Value.drawable, pp.Value.texture);

                }
                else
                {
                    int cc = pp.Key;
                    if (cc == 0 || cc == 2 || cc == 6 || cc == 7)
                    {
                        await this.SetAccessoriesAsync(cc, -1, 0);
                    }
                    else
                    {
                        await this.SetAccessoriesAsync(cc, 0, 0);
                    }

                }
            });

            if (!clothesParts.ContainsKey(5)) await this.SetClothesAsync(5, 0, 0);
            if (!clothesParts.ContainsKey(7)) await this.SetClothesAsync(7, 0, 0);
            if (!clothesParts.ContainsKey(9)) await this.SetClothesAsync(9, 0, 0);
            if (!clothesParts.ContainsKey(10)) await this.SetClothesAsync(10, 0, 0);

            if (!propParts.ContainsKey(0)) await this.SetAccessoriesAsync(0, -1, 0);
            if (!propParts.ContainsKey(1)) await this.SetAccessoriesAsync(1, -1, 0);
            if (!propParts.ContainsKey(2)) await this.SetAccessoriesAsync(2, -1, 0);
            if (!propParts.ContainsKey(6)) await this.SetAccessoriesAsync(6, -1, 0);
            if (!propParts.ContainsKey(7)) await this.SetAccessoriesAsync(7, -1, 0);

            await this.RemoveAllWeaponsAsync();

            await this.Weapons.forEach(async x => await this.GiveWeaponAsync((WeaponHash)NAPI.Util.GetHashKey("weapon_" + x.WeaponHash.ToLower()), x.Ammo));

            await NAPI.Task.RunAsync(() => NAPI.Player.SetPlayerCurrentWeapon(this, WeaponHash.Unarmed));
        }

        //[HandleExceptions]
        public async Task PlayInventoryInteractAnimation(int time = 1500)
        {
            if (await this.GetIsInVehicleAsync()) return;

            await PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_common", "givetake2_a");
            Freezed = true;
            await Task.Delay(time);
            if (!await this.GetExistsAsync()) return;
            Freezed = false;
            await StopAnimationAsync();
        }

        //[HandleExceptions]
        public async Task StopAnimationAsync()
        {
            PlayingAnimation = false;

            await this.TriggerEventAsync("SetOwnAnimData", JsonConvert.SerializeObject(await NAPI.Task.RunReturnAsync(() => new AnimationSyncItem(this))));

            var nearPlayers = await PlayerController.GetPlayersInRange(await this.GetPositionAsync(), 100f);
            foreach (var player in nearPlayers)
            {
                if (player != null && player.IsLoggedIn)
                {
                    await player.TriggerEventAsync("SetAnimDataNear", this, JsonConvert.SerializeObject(await NAPI.Task.RunReturnAsync(() => new AnimationSyncItem(this))));
                }
            }

            if (!await this.GetIsInVehicleAsync())
                await NAPI.Task.RunAsync(() => this.StopAnimation());
        }

        //[HandleExceptions]
        public async Task PlayAnimationAsync(int animationFlags, string animationDict, string animationName, float speed = 8f)
        {
            PlayingAnimation = true;
            AnimationDict = animationDict;
            AnimationName = animationName;
            CurrentAnimFlags = animationFlags;
            AnimationSpeed = speed;

            await this.TriggerEventAsync("SetOwnAnimData", JsonConvert.SerializeObject(await NAPI.Task.RunReturnAsync(() => new AnimationSyncItem(this))));

            var nearPlayers = await PlayerController.GetPlayersInRange(await this.GetPositionAsync(), 100f);
            foreach (var player in nearPlayers)
            {
                if (player != null && player.IsLoggedIn)
                {
                    await player.TriggerEventAsync("SetAnimDataNear", this, JsonConvert.SerializeObject(await NAPI.Task.RunReturnAsync(() => new AnimationSyncItem(this))));
                }
            }

            NAPI.Task.Run(() =>
            {
                PlayingAnimation = false;
            }, 2000);


            // await NAPI.Task.RunAsync(() => NAPI.Player.PlayPlayerAnimation(this, animationFlags, animationDict, animationName, speed));
        }

        //[HandleExceptions]
        public async Task SendProgressbar(int duration = 5000) => await this.TriggerEventAsync("sendProgressbar", duration);



        //[HandleExceptions]
        public async Task StopProgressbar() => await this.TriggerEventAsync("sendProgressbar", 1);

        //[HandleExceptions]
        public async Task RemoveWeaponAsync(WeaponHash weaponHash) => await NAPI.Task.RunAsync(() => this.RemoveWeapon(weaponHash));
        public async Task SetIntoVehicleAsync(RXVehicle vehicle, int seat) => await NAPI.Task.RunAsync(() => this.SetIntoVehicle(vehicle, seat));

        //[HandleExceptions]
        public async Task GiveWeaponAsync(WeaponHash weaponHash, int ammo) => await NAPI.Task.RunAsync(() => this.GiveWeapon(weaponHash, ammo));

        //[HandleExceptions]
        public async Task RemoveAllWeaponsAsync() => await NAPI.Task.RunAsync(() => this.RemoveAllWeapons());

        //[HandleExceptions]
        public async Task SetClothesAsync(int slot, int drawable, int texture) => await NAPI.Task.RunAsync(() => this.SetClothes(slot, drawable, texture));

        //[HandleExceptions]
        public async Task SetAccessoriesAsync(int slot, int drawable, int texture) => await NAPI.Task.RunAsync(() => this.SetAccessories(slot, drawable, texture));

        //[HandleExceptions]
        public async Task SetFaceFeatureAsync(int slot, float scale) => await NAPI.Task.RunAsync(() => this.SetFaceFeature(slot, scale));

        //[HandleExceptions]
        public async Task SetCustomizationAsync(bool gender, GTANetworkAPI.HeadBlend headBlend, byte eyeColor, byte hairColor, byte highlightColor, float[] faceFeatures, Dictionary<int, GTANetworkAPI.HeadOverlay> headOverlays, Decoration[] decorations)
            => await NAPI.Task.RunAsync(() => this.SetCustomization(gender, headBlend, eyeColor, hairColor, highlightColor, faceFeatures, headOverlays, decorations));

        public async Task SetHeadOverlayColorAsync(int slot, Color color)
    => await NAPI.Task.RunAsync(() => this.SetHeadBlendPaletteColor(slot, color));


        //[HandleExceptions]
        public async Task TriggerEventAsync(string eventName, params object[] args) => await NAPI.Task.RunAsync(() => this.TriggerEvent(eventName, args));

        //[HandleExceptions]
        public Task SpawnAsync(Vector3 pos, float heading = 0)
        {
            NAPI.Task.Run(() => NAPI.Player.SpawnPlayer(this, pos, Heading));

            return Task.CompletedTask;
        }

        //[HandleExceptions]
        public async Task UpdateHeadBlendAsync(float shapeMix, float skinMix, float thirdMix) => await NAPI.Task.RunAsync(() => this.UpdateHeadBlend(shapeMix, skinMix, thirdMix));

        //[HandleExceptions]
        public async Task EvalAsync(string code) => await NAPI.Task.RunAsync(() => this.Eval(code));

        //[HandleExceptions]
        public async Task KickAsync(string reason = "") => await NAPI.Task.RunAsync(() => this.Kick(reason));

        //[HandleExceptions]
        public async Task SendNotificationAsync(string notification, bool flashing = true) => await NAPI.Task.RunAsync(() => this.SendNotification(notification, flashing));

        //[HandleExceptions]
        public async Task AddWeaponToLoadout(WeaponHash weaponHash, bool save = true, int ammo = 0, bool bweapon = false)
        {
            
            if (this.Weapons.Find(x => x.WeaponHash == weaponHash.ToString()) == null)
            {
                this.Weapons.Add(new WeaponLoadoutItem() { WeaponHash = weaponHash.ToString(), Ammo = ammo, BWeapon = bweapon });
            }


            if (!save) return;

            await this.GiveWeaponAsync(weaponHash, ammo);
            this.SetWeaponAmmo(weaponHash, ammo);

            using var db = new RXContext();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(s => s.Id == this.Id);
            if (dbPlayer == null) return;

            if (dbPlayer.Weapons.Length > 0 && dbPlayer.Weapons.IsValidJson<List<WeaponLoadoutItem>>())
            {
                List<WeaponLoadoutItem> dbList = JsonConvert.DeserializeObject<List<WeaponLoadoutItem>>(dbPlayer.Weapons);

                if (dbList.Find(x => x.WeaponHash == weaponHash.ToString()) == null) dbList.Add(new WeaponLoadoutItem() { WeaponHash = weaponHash.ToString(), Ammo = ammo, BWeapon = bweapon });

                dbPlayer.Weapons = JsonConvert.SerializeObject(dbList);
            }
            else if (string.IsNullOrEmpty(dbPlayer.Weapons))
            {
                dbPlayer.Weapons = JsonConvert.SerializeObject(new List<WeaponLoadoutItem>() { new WeaponLoadoutItem() { WeaponHash = weaponHash.ToString(), Ammo = ammo, BWeapon = bweapon } });
            }

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        public async Task RemoveWeaponFromLoadout(WeaponHash weaponHash, bool save = true)
        {
            if (this.Weapons.Find(x => x.WeaponHash == weaponHash.ToString()) != null)
            {
                this.Weapons.Remove(this.Weapons.Find(x => x.WeaponHash == weaponHash.ToString()));
            }

            await this.RemoveWeaponAsync(weaponHash);

            if (!save) return;

            using var db = new RXContext();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(s => s.Id == this.Id);
            if (dbPlayer == null) return;

            if (dbPlayer.Weapons.Length > 0 && dbPlayer.Weapons.IsValidJson<List<WeaponLoadoutItem>>())
            {
                List<WeaponLoadoutItem> dbList = JsonConvert.DeserializeObject<List<WeaponLoadoutItem>>(dbPlayer.Weapons);

                if (dbList.Find(x => x.WeaponHash == weaponHash.ToString()) != null) dbList.Remove(dbList.Find(x => x.WeaponHash == weaponHash.ToString()));

                dbPlayer.Weapons = JsonConvert.SerializeObject(dbList);
            }
            else if (string.IsNullOrEmpty(dbPlayer.Weapons))
            {
                dbPlayer.Weapons = "[]";
            }

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        public async Task ClearLoadout(bool save = true)
        {
            if (!save) return;

            await RemoveAllWeaponsAsync();

            using var context = new RXContext();

            DbPlayer dbPlayer = await context.Players.FirstOrDefaultAsync(s => s.Id == this.Id);
            if (dbPlayer == null) return;

            dbPlayer.Weapons = "[]";

            await context.SaveChangesAsync();
        }

        //[HandleExceptions]
        public async Task<int> TakeAnyMoney(int amount, string message = null, bool ignoreMinuse = false)
        {
            if (await this.TakeMoney(amount)) return 0;
            if (this.BankAccount != null && await this.BankAccount.TakeBankMoney(amount, message, ignoreMinuse)) return 1;

            return -1;
        }

        //[HandleExceptions]
        public async Task<bool> TakeMoney(int money)
        {
            if (money < 0) return false;
            if (this.Cash < money) return false;
            if (this.Cash - money > this.Cash) return false;

            this.Cash -= money;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Id == this.Id);
            if (dbPlayer == null) return false;

            dbPlayer.Cash = this.Cash;

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TakeBlackMoney(int money)
        {
            if (money < 0) return false;
            if (this.Blackmoney < money) return false;
            if (this.Blackmoney - money > this.Blackmoney) return false;

            this.Blackmoney -= money;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Id == this.Id);
            if (dbPlayer == null) return false;

            dbPlayer.Blackmoney = this.Blackmoney;

            await db.SaveChangesAsync();

            return true;
        }

        //[HandleExceptions]
        public async Task<bool> GiveMoney(int money)
        {
            if (money < 0) return false;
            if (this.Cash + money < this.Cash) return false;

            this.Cash += money;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Id == this.Id);
            if (dbPlayer == null) return false;

            dbPlayer.Cash = this.Cash;

            await db.SaveChangesAsync();

            return true;
        }

        public async Task BanPlayer(string reason)
        {
            string socialClubName = "";
            string playerAddress = "";
            string socialClubId = "";
            string hardwareId = "";
            string clientHash = "";
            string targetName = "";
            int targetWarns = 0;
            int targetForumId = 0;
            string targetDiscordID = "";

            using var db = new RXContext();



                socialClubName = await NAPI.Task.RunReturnAsync(() => this.SocialClubName);
                playerAddress = await NAPI.Task.RunReturnAsync(() => this.Address);
                socialClubId = await NAPI.Task.RunReturnAsync(() => this.SocialClubId.ToString());
                hardwareId = await NAPI.Task.RunReturnAsync(() => this.Serial);
                clientHash = this.ClientHash;
                targetName = await NAPI.Task.RunReturnAsync(() => this.Name);
            targetWarns = this.Warns;
                targetForumId = this.ForumId;
                targetDiscordID = this.DiscordID;
            


                await this.SendNotify("Du wirst in wenigen Sekunden vom Gameserver gebannt: Grund: " + reason, 10000, "red", "Bann");
                await Task.Delay(2000);
                await this.KickAsync();
            

            await AdminCommands.SendForumKonversation(targetForumId, targetName, targetWarns, reason);
            RX.SendGlobalNotifyToAll(targetName + " einen permanenten Communityausschluss vom Anticheat erhalten!" + (reason == "" ? "" : " (Grund: " + reason + ")"), 8000, "red", Icon.Admin);

            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubId });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = playerAddress });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubName });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = hardwareId });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = clientHash });

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        public async Task<bool> CanInteractAntiFlood(int seconds = 3)
        {
            if (this.LastInteracted.AddSeconds(seconds) > DateTime.Now)
            {
                await this.SendNotify("Bitte warte kurz..");
                return await Task.FromResult(false);
            }
            this.LastInteracted = DateTime.Now;
            return await Task.FromResult(true);
        }

        //[HandleExceptions]
        public async Task<bool> CanInteractAntiFloodNoMSG(double seconds = 3)
        {
            if (this.LastInteracted.AddSeconds(seconds) > DateTime.Now)
            {
                return await Task.FromResult(false);
            }
            this.LastInteracted = DateTime.Now;
            return await Task.FromResult(true);
        }
        //events.ShowIF("ReactionGame", JSON.stringify({s: 1.2, p: 100, e: "Test"}))

        public async Task ShowReactionGame(string ev, object args, ReactionGameLevel reactionGameLevel)
        {
            double cursorspeed = 0.8;
            uint cursorposition = 100;



            Random rand = new Random();

            cursorposition = (uint)rand.Next(360);
            if (reactionGameLevel == ReactionGameLevel.Easy)
            {
                cursorspeed = GetRandomNumber(0.8, 1.4);
            }
            else if (reactionGameLevel == ReactionGameLevel.Normal)
            {
                cursorspeed = GetRandomNumber(1.4, 1.9);
            }
            else
            {
                cursorspeed = GetRandomNumber(1.9, 2.1);
            }


            object reactionGame = new
            {
                s = cursorspeed,
                p = cursorposition,
                e = ev,
                a = args,
            };

            RXWindow window = new RXWindow("ReactionGame");

            await window.OpenWindow(this, reactionGame);

        }
        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        //[HandleExceptions]
        public void ShowNativeMenu(NativeMenu nativeMenu, bool search = false)
        {
            NAPI.Task.Run(() =>
            {
                this.Menu = nativeMenu;

                if (search)
                {
                    this.TriggerEvent("openWindow", "SearchMenu", JsonConvert.SerializeObject(nativeMenu));
                }
                else
                {
                    this.TriggerEvent("componentServerEvent", "NativeMenu", "showNativeMenu", JsonConvert.SerializeObject(nativeMenu), 0);
                }
            });
        }

        //[HandleExceptions]
        public void CloseNativeMenu(bool search = false)
        {
            NAPI.Task.Run(() =>
            {
                this.Menu = null;

                if (search)
                {
                    this.TriggerEvent("closeWindow", "SearchMenu");
                }
                else
                {
                    this.TriggerEvent("componentServerEvent", "NativeMenu", "hide");
                }
            });
        }

        public FunkStatus FunkStatus { get; set; } = FunkStatus.Deactive;

        public class InfoCardData
        {
            public string key { get; set; }
            public string value { get; set; }

        }

        //[HandleExceptions]
        public async void SendInfocard(string title, string color, string imgSrc, int duration, int type, List<InfoCardData> data)
            => await this.TriggerEventAsync("sendInfocard", title, color, imgSrc, duration, type, JsonConvert.SerializeObject(data));

        //[HandleExceptions]
        public async Task ShowIdCard(RXPlayer target)
        {
            if (target == null || !this.IsLoggedIn || !target.IsLoggedIn) return;

            var name = (await this.GetNameAsync()).Split('_');

            await target.TriggerEventAsync("showPerso", name[0], name[1], this.DateOfEntry, this.HouseId == 0 ? "Obdachlos" : "Haus " + this.HouseId, this.Level, this.Id, false, "");
        }

        //[HandleExceptions]
        public async void SetTied(bool tied, bool inVehicle = false)
        {
            if (tied)
            {
                await this.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                await this.TriggerEventAsync("freezePlayer", true);
            }
            else
            {
                await this.StopAnimationAsync();
                await this.TriggerEventAsync("freezePlayer", false);
            }

            this.IsTied = tied;

            await this.TriggerEventAsync(PlayerDatas.TiedEvent, tied);
        }

        public async void SetCuffed(bool cuffed, bool inVehicle = false)
        {
            if (cuffed)
            {
                await this.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                await this.TriggerEventAsync("freezePlayer", true);
            }
            else
            {
                await this.StopAnimationAsync();
                await this.TriggerEventAsync("freezePlayer", false);
            }

            this.IsCuffed = cuffed;

            await this.TriggerEventAsync(PlayerDatas.CuffedEvent, cuffed);
        }


        public async void resetFriskInventoryFlags()
        {
            await NAPI.Task.RunAsync(() =>
            {
                this.ResetData("disableFriskInv");
                this.ResetData("friskInvUserName");
                this.ResetData("friskInvUserID");
                this.ResetData("friskInvVeh");
                this.ResetData("friskInvHouse");
            });
        }

        public bool CanInteract()
        {
            if (!this.IsLoggedIn || this.IsCuffed || this.IsTied || this.DeathData.IsDead || this.IsCarry || this.IsCarried) return false;

            return true;
        }

        public bool IsHigh()
        {
            if (this.Joints >= 1 && (DateTime.Now - this.LastJoint).TotalMinutes < 15)
            {
                return true;
            }
            else return false;
        }

        public async Task<double> GetFrequencyAsync()
        {
            return await NAPI.Task.RunReturnAsync(() => this.Frequency);
        }

        public async Task<FunkStatus> GetFunkStatusAsync()
        {
            return this.FunkStatus;
        }

        public void resetDisabledInventoryFlag() => this.IsInvDisabled = false;

        public async Task KickPlayer(string reason, bool publication = false)
        {
            await this.SendNotify("Du wirst in wenigen Sekunden vom Gameserver gekickt: Grund: " + reason, 10000, "red", "Kick");

            await Task.Delay(5000);

            if (publication)
                RX.SendGlobalNotifyToAll(await this.GetNameAsync() + " wurde vom Server gekickt!" + (reason == "" ? "" : " (Grund: " + reason + ")"), 8000, "red", Icon.Admin);

            await this.KickAsync();
        }

        public async Task RevivePlayer()
        {
            await this.StopAnimationAsync();
            await this.SpawnAsync(await this.GetPositionAsync());

            await this.TriggerEventAsync("transitionFromBlurred", 2000);

            this.Injured = false;
            this.Invincible = false;
            this.Freezed = false;
            this.DeathData = new RXDeathData
            {
                DeathTime = DateTime.Now,
                IsDead = false
            };

            await this.disableAllPlayerActions(false);

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == this.Id);
            if (dbPlayer == null) return;

            dbPlayer.Coma = false;
            dbPlayer.DeathStatus = false;
            dbPlayer.DeathTime = DateTime.Now;
            dbPlayer.Stress = 0;

            await db.SaveChangesAsync();
        }

        public async Task SaveAnimationShortcuts()
        {
            if (this.AnimationShortcuts == null) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == this.Id);
            if (dbPlayer == null) return;

            dbPlayer.AnimationShortcuts = JsonConvert.SerializeObject(this.AnimationShortcuts);

            await db.SaveChangesAsync();
        }

        public async Task UpdateAnimationShortcuts()
            => await this.TriggerEventAsync("setNMenuItems", this.GetJsonAnimationsShortcuts());

        public string GetJsonAnimationsShortcuts()
        {
            List<AnimationShortcutJson> animationShortCutJsons = new List<AnimationShortcutJson>();

            foreach (KeyValuePair<uint, uint> kvp in this.AnimationShortcuts)
            {
                AnimationShortcutJson item = new AnimationShortcutJson() { Slot = (int)kvp.Key, Event = "", Name = "Nicht belegt", Args = "", animId = 0, isServer = true };

                // Specials
                if (kvp.Key == 0) // general stop anim..
                {
                    item = new AnimationShortcutJson() { Slot = (int)kvp.Key, Name = "Animation beenden", Event = "StopAnimationSlow", Args = "", animId = 0, isServer = true };

                    animationShortCutJsons.Add(item);
                    continue;
                }

                if (kvp.Value == 0) // nicht belegt
                {
                    item = new AnimationShortcutJson() { Slot = (int)kvp.Key, Name = "Nicht belegt", Event = "", Args = "", animId = 0, isServer = true };
                    animationShortCutJsons.Add(item);
                    continue;
                }
                else
                {
                    if (AnimationModule.AnimationItems.FirstOrDefault(x => x.Id == kvp.Value) != null)
                    {
                        DbAnimationItem animationItem = AnimationModule.AnimationItems.FirstOrDefault(x => x.Id == kvp.Value);
                        item = new AnimationShortcutJson() { Slot = (int)kvp.Key, Name = animationItem.Text, Event = "PlayAnimFromNMenu", Args = kvp.Key.ToString(), animId = 0, isServer = true }; //animationItem.Icon };
                        animationShortCutJsons.Add(item);
                    }
                    else
                    {
                        item = new AnimationShortcutJson() { Slot = (int)kvp.Key, Name = "Nicht belegt", Event = "", Args = "", animId = 0, isServer = true };
                        animationShortCutJsons.Add(item); // add defaults...
                    }
                }
            }

            return JsonConvert.SerializeObject(animationShortCutJsons);
        }

        public async Task ShowLoader(string msg = "", int duration = 0)
            => await this.TriggerEventAsync("componentServerEvent", "Loader", "ShowLoader", msg, duration);

        public async Task ChangeLoaderMessage(string msg = "")
            => await this.TriggerEventAsync("componentServerEvent", "Loader", "ChangeMessage", msg);

        public async Task HideLoader()
            => await this.TriggerEventAsync("componentServerEvent", "Loader", "HideLoader");

        public static explicit operator RXPlayer(List<Entity> v)
        {
            throw new NotImplementedException();
        }
    }
}
