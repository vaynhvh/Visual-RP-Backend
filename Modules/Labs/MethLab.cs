using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Casino;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Jail;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.Modules.Labs
{
    public class TestTemp
    {

        [JsonProperty(PropertyName = "temperature")]
        public Parametert Temperature { get; set; }

        [JsonProperty(PropertyName = "pressure")]
        public Parametert Pressure { get; set; }

        [JsonProperty(PropertyName = "stirring")]
        public Parametert Stirring { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public Parametert Amount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; }

    }

    public class Parametert
    {
        public float min { get; set; }
        public float max { get; set; }
        public float current { get; set; }
        public float step { get; set; }
    }


    class MethLab : RXModule
    {
        public MethLab() : base("MethLab", new RXWindow("MethLabor")) { }

        public static List<DbMethLab> MethLabs = new List<DbMethLab>();
        public static List<uint> RessourceItemIds = new List<uint> { 27, 73, 26 }; //Toilettenreiniger, Batterien, Ephedrinkonzentrat (965) 
        public static List<uint> EndProductItemIds = new List<uint> { 29 }; //Pures Meth
        public static uint FuelItemId = 65; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)

        public static int temp = 0;
        public static uint RankNeededForParameter = 9;
        public string PlayerIds = "";
        public int AmountPerProcess = 0;//
        public List<RXTeam> HasAlreadyHacked = new List<RXTeam>();



        public override async void LoadAsync()
        {
            try
            {
                RequireModule("Team");

                await Task.Delay(8000);
                using var db = new RXContext();
                foreach (var lab in await db.MethLabs.ToListAsync())
                {
                    MethLabs.Add(lab);
                }

                foreach (var lab in MethLabs)
                {
                    var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);

                    if (team == null)
                    {
                        RXLogger.Print("Dieses Team existiert nicht!");
                        return;
                    }
                    var labcomputer = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryLaptopPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    labcomputer.ColShape.Action = async player => await EnterLabComputer(player, lab);
                  
                    var labstart = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryStartPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    labstart.ColShape.Action = async player => await EnterStartLab(player, lab);

                    var anallab = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryAnalyzePosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
             
                    var batteriepoint = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryBatterieSwitch, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);

                    batteriepoint.ColShape.Action = async player => await BatteryExchange(player, lab);

                    var ephepulver = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryEphePulver, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);

                    ephepulver.ColShape.Action = async player => await EphiConvert(player, lab);

                    var boilerquality = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryCheckBoilerQuality, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);

                    var labweaponinputinv = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryInvInputPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labweaponoutputinv = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryInvOutputPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labmethfuelinv = await NAPI.Entity.CreateMCB(LabCoords.MethlaboratoryInvFuelPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labenter = await NAPI.Entity.CreateMCB(lab.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);
                    var labexit = await NAPI.Entity.CreateMCB(LabCoords.MethLaboratoryEntranceExitPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);

                    var labenterlock = await NAPI.Entity.CreateMCB(lab.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);
                    var labexitlock = await NAPI.Entity.CreateMCB(LabCoords.MethLaboratoryEntranceExitPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);

                    labenterlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                    labenterlock.ColShape.Action = async player => await ToggleLabLock(player, lab);

                    labexitlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                    labexitlock.ColShape.Action = async player => await ToggleLabLock(player, lab);

                    labexit.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um das Labor zu verlassen!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = "Methlabor " + lab.Name,
                        RestrictedToTeam = lab.TeamId,
                    };

                    labexit.ColShape.Action = async player => await LeaveLab(player, lab);

                    labenter.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um das Labor zu betreten!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = "Methlabor " + lab.Name,
                        RestrictedToTeam = lab.TeamId,
                    };

                    labenter.ColShape.Action = async player => await EnterLab(player, lab);

                    var fuelcontainer = await db.Containers.FirstOrDefaultAsync(x => x.Id == lab.FuelContainerId);
                    if (fuelcontainer == null)
                    {
                        var dblab = await db.MethLabs.FirstOrDefaultAsync(x => x.Id == lab.Id);

                        fuelcontainer = new DbContainer
                        {
                            Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                            Name = "Labor Fuel",
                            MaxSlots = 60,
                            MaxWeight = 300000,
                        };

                        dblab.FuelContainerId = fuelcontainer.Id;
                        lab.FuelContainerId = fuelcontainer.Id;

                        await db.Containers.AddAsync(fuelcontainer);
                        await db.SaveChangesAsync();

                    }

                    lab.LabProduction = new Production(
              new List<uint>
              {
                    RessourceItemIds[0], //Ephi
                    RessourceItemIds[1], //Batterien
                    RessourceItemIds[2]  //Toilettenreiniger
              },
              EndProductItemIds,      //Meth
              5,      //Minimale Anzahl von Meth
              6      //Maximale Anzahl von Meth
          );

                    lab.Parameters = new List<Parameter>
            {
                new Parameter("Temperatur", "°C", 100.0f, 1500.0f, lab.Temperature),
                new Parameter("Druck", "Bar", 1.0f, 10.0f, lab.Druck),
                new Parameter("Ruehrgeschwindigkeit", "U/min", 1.0f, 300.0f, lab.Ruehrgeschwindigkeit),
                new Parameter("Menge", "Stück", 5.0f, 15.0f, lab.Menge),
            };



                    await NAPI.Task.RunAsync(() =>
                    {
                        labmethfuelinv.ColShape.IsContainerColShape = true;
                        labmethfuelinv.ColShape.ContainerId = lab.FuelContainerId;
                        labmethfuelinv.ColShape.ContainerOpen = true;
                        labmethfuelinv.ColShape.ContainerRestrictedTeam = team.Id;

                    });

                }
            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public async Task EphiConvert(RXPlayer player, DbMethLab lab)
        {
            int EphikonzenAmount = player.Container.GetItemAmount(74);
            int addableAmount = EphikonzenAmount / 2;
            // 725 -> 965
            if (EphikonzenAmount >= 2)
            {
                if (addableAmount > player.Container.GetMaxItemAddedAmount(26))
                {
                    addableAmount = player.Container.GetMaxItemAddedAmount(26);
                }

                if (addableAmount > 0)
                {
                        await player.SendProgressbar(500 * addableAmount);

                    player.Container.RemoveItem(74, addableAmount * 2);

                    player.Freezed = true;
                    await player.disableAllPlayerActions(true);

                    await Task.Delay(500 * addableAmount);

                    player.Freezed = false;
                    await player.disableAllPlayerActions(false);

                    await player.StopAnimationAsync();
                    player.Container.AddItem(26, addableAmount);

                    await player.SendNotify($"{addableAmount * 2} {ItemModelModule.ItemModels.Find(x => x.Id == 74).Name} wurde in {addableAmount} {ItemModelModule.ItemModels.Find(x => x.Id == 26).Name} zerlegt.");

                }
            }
        }

        public async Task BatteryExchange(RXPlayer player, DbMethLab lab)
        {
            int BatterieAmount = player.Container.GetItemAmount(25);
            int addableAmount = BatterieAmount * 5;
            if (BatterieAmount >= 1)
            {
                if (addableAmount > player.Container.GetMaxItemAddedAmount(73))
                {
                    addableAmount = player.Container.GetMaxItemAddedAmount(73);
                }

                if (addableAmount > 0)
                {
                        await player.SendProgressbar(100 * addableAmount);

                        player.Container.RemoveItem(25, addableAmount / 5);

                        player.Freezed = true;
                        await player.disableAllPlayerActions(true);

                        await Task.Delay(100 * addableAmount);

                        player.Freezed = false;
                        await player.disableAllPlayerActions(false);

                        await player.StopAnimationAsync();
                        player.Container.AddItem(73, addableAmount);

                        await player.SendNotify($"{addableAmount / 5} {ItemModelModule.ItemModels.Find(x => x.Id == 25).Name} wurde in {addableAmount} {ItemModelModule.ItemModels.Find(x => x.Id == 73).Name} zerlegt.");
                }
            }
        }

        public async Task StartProcess(RXPlayer dbPlayer, DbMethLab lab)
        {
            int menge = 10;

            foreach (uint itemId in lab.LabProduction.NeededItems)
            {
                if (dbPlayer.LabInputContainer.GetItemAmount(itemId) < menge)
                {
                    await StopProcess(dbPlayer, lab);
                    await dbPlayer.SendNotify("Es fehlen Materialien... ( " + menge + " " + ItemModelModule.ItemModels.Find(x => x.Id == itemId).Name + " )");
                    return;
                }
                uint fuelAmount = (uint)lab.LabFuelContainer.GetItemAmount(FuelItemId);
                if (fuelAmount < FuelAmountPerProcessing)
                {
                    await dbPlayer.SendNotify("Es fehlt Kraftstoff...");
                    await StopProcess(dbPlayer, lab);
                    return;
                }
            }
            if (lab.ActivePlayers.ToList().Contains(dbPlayer))
            {
                await dbPlayer.SendNotify("Der Prozess ist bereits im Gange...");
                return;
            }
            lab.ActivePlayers.Add(dbPlayer);
            await dbPlayer.SendNotify("Prozess gestartet!");
        }
        public async Task StopProcess(RXPlayer dbPlayer, DbMethLab lab)
        {
            if (lab.ActivePlayers.ToList().Contains(dbPlayer))
            {
                lab.ActivePlayers.Remove(dbPlayer);
                await dbPlayer.SendNotify("Prozess gestoppt!");
            }
        }

        [RemoteEvent]
        public void saveMethLabor(RXPlayer dbPlayer, float temperature, float pressure, float stirring, float amount)
        {
            if (dbPlayer == null)
            {
                return;
            }

            var methlaboratory = MethLabs.Find(x => x.TeamId == dbPlayer.TeamId);
            if (methlaboratory == null) return;


            methlaboratory.Parameters[0].ActValue = temperature;
            methlaboratory.Parameters[1].ActValue = pressure;
            methlaboratory.Parameters[2].ActValue = stirring;
            methlaboratory.Parameters[3].ActValue = amount;


            CalculateNewQuality(methlaboratory);

        }

        [RemoteEvent]
        public async Task toggleMethLabor(RXPlayer dbPlayer, bool result)
        {
            if (dbPlayer == null)
            {
                return;
            }
            var methlaboratory = MethLabs.Find(x => x.TeamId == dbPlayer.Id);
            if (methlaboratory == null)
            {
                return;
            }

            if (result)
            {
                await StartProcess(dbPlayer, methlaboratory);
            }
            else
            {
                await StopProcess(dbPlayer, methlaboratory);
            }
        }

        public void CalculateNewQuality(DbMethLab lab)
        {
            if (lab.CalculatedValue == 0) GenerateNewCalculatedValue(lab);

            int SumMin = Convert.ToInt32(lab.Parameters[0].MinValue + lab.Parameters[1].MinValue + lab.Parameters[2].MinValue + lab.Parameters[3].MinValue);
            int SumMax = Convert.ToInt32(lab.Parameters[0].MaxValue + lab.Parameters[1].MaxValue + lab.Parameters[2].MaxValue + lab.Parameters[3].MaxValue);

            int SumActual = Convert.ToInt32(lab.Parameters[0].ActValue + lab.Parameters[1].ActValue + lab.Parameters[2].ActValue + lab.Parameters[3].ActValue);

            int QualityRange1 = Convert.ToInt32((SumMax - SumMin) * 0.04); // 8 % 
            int QualityRange2 = Convert.ToInt32((SumMax - SumMin) * 0.10); // 20 % 
            int QualityRange3 = Convert.ToInt32((SumMax - SumMin) * 0.20); // 40 % 

            if (Math.Abs(SumActual - lab.CalculatedValue) <= QualityRange1) lab.Quality = 0.99; // Best
            else if (Math.Abs(SumActual - lab.CalculatedValue) > QualityRange1 && Math.Abs(SumActual - lab.CalculatedValue) <= QualityRange2) lab.Quality = 0.8; // Good
            else if (Math.Abs(SumActual - lab.CalculatedValue) > QualityRange2 && Math.Abs(SumActual - lab.CalculatedValue) <= QualityRange3) lab.Quality = 0.7; // Normal
            else if (Math.Abs(SumActual - lab.CalculatedValue) > QualityRange3) lab.Quality = 0.4; // Bad

            return;
        }

        public void GenerateNewCalculatedValue(DbMethLab lab)
        {
            int SumMin = Convert.ToInt32(lab.Parameters[0].MinValue + lab.Parameters[1].MinValue + lab.Parameters[2].MinValue + lab.Parameters[3].MinValue);
            int SumMax = Convert.ToInt32(lab.Parameters[0].MaxValue + lab.Parameters[1].MaxValue + lab.Parameters[2].MaxValue + lab.Parameters[3].MaxValue);

            Random random = new Random();

            lab.CalculatedValue = random.Next(SumMin, SumMax);

            return;
        }


        public async Task ChangeMethlaboratoryParameter(RXPlayer dbPlayer, string returnString, string parameterName)
        {
            if (dbPlayer == null)
            {
                return;
            }

            if (!Regex.IsMatch(returnString, @"^[0-9]*$"))
            {
                await dbPlayer.SendNotify("Nur Zahlen sind erlaubt");
                return;
            }
            if (!float.TryParse(returnString, out float value)) return;

            var methlaboratory = MethLabs.Find(x => x.TeamId == dbPlayer.TeamId);
            if (methlaboratory == null) return;
            Parameter parameter = methlaboratory.Parameters.Find(delegate (Parameter para) { return para.Name == parameterName; });
            if (parameter.MinValue <= value && value <= parameter.MaxValue)
            {
                parameter.ActValue = value;
                await dbPlayer.SendNotify($"{parameter.Name} wurde auf {value} {parameter.Einheit} eingestellt.");
            }
            else
            {
                await dbPlayer.SendNotify("Das ist kein gültiger Einstellparameter. Minimal- und Maximalwert beachten!");
            }
        }


        public async Task EnterStartLab(RXPlayer player, DbMethLab lab)
        {
            var par = lab.Parameters[0];
            TestTemp testTemp = new TestTemp();


            testTemp.Temperature = new Parametert
            {
                current = par.ActValue,
                min = par.MinValue,
                max = par.MaxValue,
                step = 1.0f
            };

            par = lab.Parameters[1];
            testTemp.Pressure = new Parametert
            {
                current = par.ActValue,
                min = par.MinValue,
                max = par.MaxValue,
                step = 1.0f
            };

            par = lab.Parameters[2];
            testTemp.Stirring = new Parametert
            {
                current = par.ActValue,
                min = par.MinValue,
                max = par.MaxValue,
                step = 1.0f
            };

            par = lab.Parameters[3];
            testTemp.Amount = new Parametert
            {
                current = par.ActValue,
                min = par.MinValue,
                max = par.MaxValue,
                step = 1.0f
            };

            testTemp.Status = lab.ActivePlayers.Contains(player);

            await this.Window.OpenWindow(player, testTemp); 
        }

        public async Task EnterLabComputer(RXPlayer player, DbMethLab lab)
        {

          
        }

        public async Task<bool> FriskLaboratory(RXPlayer dbPlayer, DbMethLab lab)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!lab.HackInProgress)
            {
                await dbPlayer.SendNotify("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (lab.FriskInProgress)
            {
                await dbPlayer.SendNotify("Das Labor wird schon durchsucht...");
                return false;
            }
            lab.FriskInProgress = true;

            Dictionary<int, int> itemsFound = new Dictionary<int, int>();
            foreach (int itemId in lab.LabProduction.NeededItems)
            {
                itemsFound.Add(itemId, 0);
            }
            foreach (int itemId in lab.LabProduction.EndProducts)
            {
                itemsFound.Add(itemId, 0);
            }

            bool found = false;
            int timeToFrisk = LabManager.TimeToFrisk;

            dbPlayer.Team.SendNotification("Das Labor  wird durchsucht...", 60000);
            timeToFrisk = timeToFrisk * 3;

            await dbPlayer.SendProgressbar(timeToFrisk);
            await dbPlayer.PlayAnimationAsync(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
            await dbPlayer.TriggerEventAsync("freezePlayer", true);
            await Task.Delay(timeToFrisk);
            if (dbPlayer == null || !dbPlayer.CanInteract())
            {
                lab.FriskInProgress = false;
                return false;
            }
            await dbPlayer.TriggerEventAsync("freezePlayer", false);
            await dbPlayer.StopAnimationAsync();
            foreach (var member in PlayerController.GetValidPlayers().Where(x => x.TeamId == dbPlayer.TeamId))
            { 
                if (member == null) continue;
                {
                    var inputcontainer = ContainerModule.Containers.Find(x => x.Id == member.LabInputContainerId);
                    foreach (var kvpSlots in inputcontainer.Slots.ToList())
                    {
                        if (kvpSlots != null & kvpSlots.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.ItemModelId] += kvpSlots.Amount;
                            found = true;
                        }
                    }
                }
                var outputcontainer = ContainerModule.Containers.Find(x => x.Id == member.LabOutputContainerId);

                if (outputcontainer != null)
                {
                    foreach (var kvpSlots in outputcontainer.Slots.ToList())
                    {
                        if (kvpSlots != null & kvpSlots.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.ItemModelId] += kvpSlots.Amount;
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                string info = "Funde: ";
                foreach (KeyValuePair<int, int> kvp in itemsFound)
                    info += kvp.Value + " " + ItemModelModule.ItemModels.Find(x => x.Id == kvp.Key).Name + ", ";
                info = info.Substring(0, info.Length - 1);
                await dbPlayer.SendNotify(info);
            }
            else
            {
                await dbPlayer.SendNotify("Nichts gefunden");
            }
            lab.FriskInProgress = false;
            return false;
        }

        public async Task EnterLab(RXPlayer player, DbMethLab lab)
        {

            if (lab.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);

            int boilerState = 2;
            int tableState = 1;
            int securityState = 1;

            await player.TriggerEventAsync("loadMethInterior", tableState, boilerState, securityState);

            await player.SetDimensionAsync(team.Dimension);

            await player.SetPositionAsync(LabCoords.MethLaboratoryEntranceExitPosition);
        }

        public async Task ToggleLabLock(RXPlayer player, DbMethLab lab)
        {
            if (player.TeamId != lab.TeamId) return;
            if (lab.Locked)
            {
                await player.SendNotify("Du hast die Tür aufgeschlossen!", 3500, "green");
                lab.Locked = false;
            } else
            {
                await player.SendNotify("Du hast die Tür abgeschlossen!", 3500, "red");
                lab.Locked = true;
            }
        }
        

        public async Task LeaveLab(RXPlayer player, DbMethLab lab)
        {

            if (lab.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);


            await player.SetDimensionAsync(0);

            await player.SetPositionAsync(lab.Position.ToPos());


        }
      
    }
}
