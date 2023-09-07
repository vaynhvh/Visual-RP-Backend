using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    class ContactsApp : RXModule
    {
        public ContactsApp() : base("ContactsApp", new RXWindow("Phone")) { }

        public static List<DbPhoneContact> PhoneContacts = new List<DbPhoneContact>();

        public static ContactsApp Instance = new ContactsApp();

        //[HandleExceptions]
        public override async Task OnTenSecond()
        {
            using var db = new RXContext();

            List<DbPhoneContact> copyPhoneContacts = new List<DbPhoneContact>();

            TransferDBContextValues(await db.PhoneContacts.ToListAsync(), phoneContact => copyPhoneContacts.Add(phoneContact));

            PhoneContacts = copyPhoneContacts;
        }

        //[HandleExceptions]
        public static string GetContactName(RXPlayer player, uint number)
        {
            if (number == player.Phone) return "Ich";

            var contact = PhoneContacts.FirstOrDefault(contact => contact.PlayerId == player.Id && contact.Number == number);
            if (contact == null) return number.ToString();

            return contact.Name.Replace("000FAV", "");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task RqContacts(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            await player.TriggerEventAsync("RsContacts", JsonConvert.SerializeObject(PhoneContacts.Where(x => x.PlayerId == player.Id).ToList()), player.Phone);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task AddContact(RXPlayer player, uint number, string name, string text)
        {
            if (!player.CanInteract()) return;

            if (number <= 0 || number > 99999999) return;

            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_#\s-]+$"))
            {
                await player.SendNotify("Kontakt konnte nicht gespeichert werden!");
                return;
            }

            using var db = new RXContext();

            await db.PhoneContacts.AddAsync(new DbPhoneContact
            {
                PlayerId = player.Id,
                Name = name,
                Number = number,
                Note = text
            });

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task RemoveContact(RXPlayer player, uint number)
        {
            if (!player.CanInteract()) return;

            if (number <= 0 || number > 99999999) return;

            using var db = new RXContext();

            var contact = await db.PhoneContacts.FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Number == number);
            if (contact == null) return;

            db.PhoneContacts.Remove(contact);

            await db.SaveChangesAsync();

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task EditContact(RXPlayer player, uint oldNumber, uint newNumber, string name, string text)
        {
            if (!player.CanInteract()) return;


            if (oldNumber <= 0 || oldNumber > 99999999) return;
            if (newNumber <= 0 || newNumber > 99999999) return;

            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_#\s-]+$"))
            {
                await player.SendNotify("Kontakt konnte nicht aktualisiert werden!");
                return;
            }

            using var db = new RXContext();

            var contact = await db.PhoneContacts.FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Number == oldNumber);
            if (contact == null) return;

            contact.Name = name;
            contact.Number = newNumber;
            contact.Note = text;

            await db.SaveChangesAsync();

        }
    }
}
