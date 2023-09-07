using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Bank;
using Backend.Modules.Player;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Barber
{
    public class BarberObject
    {
        [JsonProperty(PropertyName = "barber")]
        public ListJsonBarberObject Barber { get; set; }

        [JsonProperty(PropertyName = "player")]
        public BarberPlayerObject Player { get; set; }
    }

    public class BarberPlayerObject
    {
        [JsonProperty(PropertyName = "hair")]
        public int Hair { get; set; }

        [JsonProperty(PropertyName = "hairColor")]
        public int HairColor { get; set; }

        [JsonProperty(PropertyName = "hairColor2")]
        public int HairColor2 { get; set; }

        [JsonProperty(PropertyName = "beard")]
        public int Beard { get; set; }

        [JsonProperty(PropertyName = "beardColor")]
        public int BeardColor { get; set; }

        [JsonProperty(PropertyName = "beardOpacity")]
        public float BeardOpacity { get; set; }

        [JsonProperty(PropertyName = "chestHair")]
        public int Chest { get; set; }

        [JsonProperty(PropertyName = "chestHairColor")]
        public int ChestHairColor { get; set; }

        [JsonProperty(PropertyName = "chestHairOpacity")]
        public float ChestHairOpacity { get; set; }
    }

    public class JsonBarberObject
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "customid")]
        public int CustomizationId { get; set; }
    }

    public class ListJsonBarberObject
    {
        [JsonProperty(PropertyName = "hairs")]
        public List<JsonBarberObject> Hairs { get; set; }

        [JsonProperty(PropertyName = "beards")]
        public List<JsonBarberObject> Beards { get; set; }

        [JsonProperty(PropertyName = "chests")]
        public List<JsonBarberObject> Chests { get; set; }

        [JsonProperty(PropertyName = "colors")]
        public List<JsonBarberObject> Colors { get; set; }
    }

    public class RXBarberShop
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }

        public RXBarberShop(uint id, Vector3 position)
        {
            Id = id;
            Position = position;
        }

        public RXBarberShop(Vector3 position)
        {
            Id = BarberModule.Barbers.Count > 0 ? BarberModule.Barbers.Max(x => x.Id) + 1 : 1;
            Position = position;
        }

        public RXBarberShop(double x, double y, double z)
        {
            Id = BarberModule.Barbers.Count > 0 ? BarberModule.Barbers.Max(x => x.Id) + 1 : 1;
            Position = new Vector3(x, y, z);
        }
    }

    class BarberModule : RXModule
    {
        public BarberModule() : base("Barber", new RXWindow("Barber")) { }

        public static List<RXBarberShop> Barbers = new List<RXBarberShop>();

        public static ListJsonBarberObject BarberObj = new ListJsonBarberObject
        {
            Beards = new List<JsonBarberObject>
                        {
                            new JsonBarberObject
                            {
                                Id = 1,
                                CustomizationId = 0,
                                Name = "3-Tage Bart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 2,
                                CustomizationId = 1,
                                Name = "Runder Bart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 3,
                                CustomizationId = 2,
                                Name = "Dichter runder Bart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 4,
                                CustomizationId = 3,
                                Name = "Anchor",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 5,
                                CustomizationId = 4,
                                Name = "Kinnbart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 6,
                                CustomizationId = 7,
                                Name = "Leichter Bart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 7,
                                CustomizationId = 8,
                                Name = "Imperial",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 8,
                                CustomizationId = 9,
                                Name = "Schnäutzer",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 9,
                                CustomizationId = 10,
                                Name = "Vollbart",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 10,
                                CustomizationId = 17,
                                Name = "seitliche Schnurrhaare",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 11,
                                CustomizationId = 19,
                                Name = "Lenker",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 12,
                                CustomizationId = 21,
                                Name = "Zapper",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 13,
                                CustomizationId = 27,
                                Name = "Hammelkoteletts",
                                Price = 10
                            }

                        },
            Hairs = new List<JsonBarberObject>
                        {
                            new JsonBarberObject
                            {
                                Id = 1,
                                CustomizationId = 0,
                                Name = "Glatze",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 2,
                                CustomizationId = 1,
                                Name = "Boxerschnitt",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 3,
                                CustomizationId = 3,
                                Name = "Rasierter Hipster",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 4,
                                CustomizationId = 2,
                                Name = "Iro, modisch",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 5,
                                CustomizationId = 10,
                                Name = "Kurz gekämmt",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 6,
                                CustomizationId = 12,
                                Name = "Cäsarenfrisur",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 7,
                                CustomizationId = 13,
                                Name = "Zerzaust",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 8,
                                CustomizationId = 14,
                                Name = "Dreadlocks",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 9,
                                CustomizationId = 15,
                                Name = "Langhaarfrisur",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 10,
                                CustomizationId = 16,
                                Name = "Zottellocken",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 11,
                                CustomizationId = 18,
                                Name = "Seitenscheitel",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 12,
                                CustomizationId = 19,
                                Name = "Hochgekämmt",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 13,
                                CustomizationId = 20,
                                Name = "Gelfrisur",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 14,
                                CustomizationId = 21,
                                Name = "Junger Hipster",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 15,
                                CustomizationId = 22,
                                Name = "Vokuhila",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 16,
                                CustomizationId = 24,
                                Name = "Klassische Flechtreihen",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 17,
                                CustomizationId = 30,
                                Name = "Hightop",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 18,
                                CustomizationId = 33,
                                Name = "Zur Seite gekämmter Undercut",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 19,
                                CustomizationId = 34,
                                Name = "Stacheliger Iro",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 20,
                                CustomizationId = 41,
                                Name = "Kurzhaarschnitt",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 21,
                                CustomizationId = 43,
                                Name = "Pferdeschwanz",
                                Price = 10
                            }
                        },
            Colors = new List<JsonBarberObject>
                        {
                            new JsonBarberObject
                            {
                                Id = 1,
                                CustomizationId = 0,
                                Name = "Schwarz",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 2,
                                CustomizationId = 6,
                                Name = "Braun",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 3,
                                CustomizationId = 14,
                                Name = "Blond",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 4,
                                CustomizationId = 48,
                                Name = "Orange",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 5,
                                CustomizationId = 19,
                                Name = "Dunkelrot",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 6,
                                CustomizationId = 26,
                                Name = "Grau",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 7,
                                CustomizationId = 29,
                                Name = "Weiß",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 8,
                                CustomizationId = 32,
                                Name = "Lila",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 9,
                                CustomizationId = 33,
                                Name = "Pink",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 10,
                                CustomizationId = 35,
                                Name = "Rosa",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 11,
                                CustomizationId = 38,
                                Name = "Blau",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 12,
                                CustomizationId = 39,
                                Name = "Grün",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 13,
                                CustomizationId = 46,
                                Name = "Gelb",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 14,
                                CustomizationId = 53,
                                Name = "Rot",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 15,
                                CustomizationId = 28,
                                Name = "Hellgrau",
                                Price = 10
                            },
                            new JsonBarberObject
                            {
                                Id = 16,
                                CustomizationId = 27,
                                Name = "Hellgrau",
                                Price = 10
                            }
                        },
            Chests = new List<JsonBarberObject>(),
        };

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            Barbers = new List<RXBarberShop>
            {
                new RXBarberShop(-814.3, -183.8, 36.6),
                new RXBarberShop(136.8, -1708.4, 28.3),
                new RXBarberShop(-1282.6, -1116.8, 6.0),
                new RXBarberShop(1931.5, 3729.7, 31.8),
                new RXBarberShop(1212.8, -472.9, 65.2),
                new RXBarberShop(-32.9, -152.3, 56.1),
                new RXBarberShop(-278.1, 6228.5, 30.7),
            };

            await Barbers.forEach(async barber =>
            {
                var mcb = await NAPI.Entity.CreateMCB(barber.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, true, 71, 0, "Friseurladen");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um deinen Style zu verändern!",
                    Color = "dgray",
                    Duration = 3500,
                    Title = "Barbershop"
                };

                mcb.ColShape.Action = async player => await OpenBarber(player, barber.Id);
            });
        }

        //[HandleExceptions]
        public async Task OpenBarber(RXPlayer player, uint barberId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var barberShop = Barbers.FirstOrDefault(x => x.Id == barberId);
            if (barberShop == null) return;

            using var db = new RXContext();

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbCharacter == null) return;

            Customization customization = JsonConvert.DeserializeObject<Customization>(dbCharacter.Customization);
            if (customization == null) return;



            await this.Window.OpenWindow(player, customization);

            await NAPI.Task.RunAsync(() => player.SetData("barberId", barberId));
        }

  
            //[HandleExceptions]
        [RemoteEvent]
        public async Task CancelBarber(RXPlayer player) => await player.LoadCharacter();

        //[HandleExceptions]
        [RemoteEvent]
        public async Task FinishBarber(RXPlayer player, string json)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || await NAPI.Task.RunReturnAsync(() => !player.HasData("barberId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("barberId") == 0)) return;

            using var db = new RXContext();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbPlayer == null) return;

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbCharacter == null) return;

                dbCharacter.Customization = json;

                await player.SendNotify("Deine Änderungen wurden erfolgreich gespeichert.", 3500, "green", "Barber");

           

            await db.SaveChangesAsync();



            await player.LoadCharacter(dbCharacter);
        }
    }
}
