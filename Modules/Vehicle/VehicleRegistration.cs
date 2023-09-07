using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Vehicle
{
    public class VehicleRegistration
    {
        public static int REGISTRATION_COST_NORMAL = 5000;
        public static int REGISTRATION_COST_WISH = 30000;

        public static int RegistrationRadius = 20;

        public static async Task<bool> IsPlateRegistered(String plate, bool privateCar)
        {
            using var db = new RXContext();

            var veh = await db.Vehicles.ToListAsync();

            var findveh = veh.Find(x => x.Plate == plate);

            if (findveh != null)
            {
                return true;
            }


            return false;
        }

        public static async Task<bool> IsVehicleRegistered(uint id)
        {
            using var db = new RXContext();

            var veh = await db.Vehicles.ToListAsync();

            var findveh = veh.Find(x => x.Id == id);

            if (findveh != null)
            {
                if (findveh.Registered)
                {
                    return true;
                }
            }


            return false;
        }

        public static async Task<bool> registerVehicle(RXVehicle sxVehicle, RXPlayer owner, RXPlayer worker, String plate, bool wish)
        {
            //check if owner and person who is from dpos isnt offline or shit
            if (owner == null || worker == null) return false;

            //calculate costs for plate 
            int costs = wish == true ? REGISTRATION_COST_WISH : REGISTRATION_COST_NORMAL;
            if (owner.BankAccount.Balance < costs)
            {
                await worker.SendNotify($"Das Konto des Kunden ist nicht gedeckt. {costs}$");
                await owner.SendNotify("Ihr Konto ist nicht gedeckt... Sie benötigen $" + costs);
                return false;
            }

           
                //vehicle is not registered
                if (await IsPlateRegistered(plate, false))
                {
                    //plate has active entry...
                    await worker.SendNotify("Dieses Kennzeichen ist bereits registriert!");
                    return false;
                }

            //Plate has no entries -> is not registered

                using var db = new RXContext();

                var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == sxVehicle.Id);

                //take money
                await owner.BankAccount.TakeBankMoney(costs, "Fahrzeug angemeldet " + sxVehicle.Id);
                //update in database and log
                sxVehicle.Registered = true;
                sxVehicle.Plate = plate;
            dbVehicle.Registered = true;
            dbVehicle.Plate = plate;
           await sxVehicle.SetNumberPlateAsync(plate);

                await worker.GiveMoney(500);
                await worker.SendNotify("Sie haben das Fahrzeug erfolgreich angemeldet.");
                await owner.SendNotify("Ihr Fahrzeug wurde erfolgreich angemeldet.");
            await db.SaveChangesAsync();
                return true;
          

        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static async Task<String> GetRandomPlate(bool privateCar)
        {
            String plate = "";
            do
            {
                plate = RandomString(8);
                if (!await IsPlateRegistered(plate, privateCar)) break;
            } while (true);
            return plate;
        }


    }
}
