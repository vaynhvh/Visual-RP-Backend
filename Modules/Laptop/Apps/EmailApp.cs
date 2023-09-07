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
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Laptop.Apps
{
    public class Email
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "readed")]
        public bool Readed { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }


        public Email(uint id, string subject, string body, bool readed, DateTime date)
        {
            Id = id;
            Subject = subject;
            Body = body;
            Readed = readed;
            Date = date;
        }

        public Email(DbEmail dbEmail)
        {
            Id = dbEmail.Id;
            Subject = dbEmail.Subject;
            Body = dbEmail.Body;
            Readed = dbEmail.Readed;
            Date = dbEmail.Date;
        }

    }

    class EmailApp : RXModule
    {
        public EmailApp() : base("EmailApp", new RXWindow("EmailApp")) { }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task requestEmails(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            List<Email> Emails = new List<Email>();

            using var db = new RXContext();

            var list = await db.Emails.Where(x => x.PlayerId == player.Id).ToListAsync();
            if (list == null || list.Count == 0) return;

            foreach (var email in list)
            {
                Emails.Add(new Email(email));
            }

            Emails = Emails.OrderByDescending(x => x.Date).ToList();

            await this.Window.TriggerEvent(player, "responseEmails", JsonConvert.SerializeObject(Emails));
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task markEmailAsRead(RXPlayer player, uint emailId)
        {
            if (!player.IsLoggedIn) return;

            using var db = new RXContext();

            var email = await db.Emails.FirstOrDefaultAsync(x => x.Id == emailId && x.PlayerId == player.Id);
            if (email == null) return;

            email.Readed = true;

            await db.SaveChangesAsync();
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task deleteMail(RXPlayer player, uint emailId)
        {
            if (!player.IsLoggedIn) return;

            using var db = new RXContext();

            var email = await db.Emails.FirstOrDefaultAsync(x => x.Id == emailId && x.PlayerId == player.Id);
            if (email == null) return;

            if (!email.Readed)
            {
                await Task.Delay(500);

                await player.SendNotify("Die Email wurde nicht gelesen!");
                await requestEmails(player);

                return;
            }

            db.Emails.Remove(email);

            await db.SaveChangesAsync();

            await player.SendNotify("Die Email wurde erfolgreich gelöscht!");
        }

        //[HandleExceptions]
        public async Task SendPlayerEmail(RXPlayer player, string subject, string template)
        {
            using var db = new RXContext();

            await db.Emails.AddAsync(new DbEmail
            {
                PlayerId = player.Id,
                Subject = subject,
                Body = template,
                Readed = false,
                Date = DateTime.Now
            });

            await db.SaveChangesAsync();

            await player.SendNotify("Du hast eine Email erhalten!");
        }
    }

    public static class EmailTemplates
    {
        public static string GetTicketTemplate(int ticketSum, string ticketDesc)
        {

            return $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurde ein Ticket in Höhe von <b>${ticketSum}</b> ausgestellt.<br><br>" +
                $"<b>Grund:</b><br>" +
                $"{ticketDesc}";

        }

        public static string GetTicketRemoveTemplate(string ticketDesc)
        {

            return $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurde ein Ticket erlassen.<br><br>" +
                $"<b>Ticket Grund:</b><br>" +
                $"{ticketDesc}";
        }

        /*public static string GetTicketRemoveListTemplate(List<CrimePlayerReason> crimes)
        {
            string returns = $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurden folgende Tickets erlassen.<br><br>" +
                $"<b>Tickets:</b><br>";

            foreach (CrimePlayerReason crime in crimes)
            {
                returns += crime.Name + "<br>";
            }
            return returns;
        }

        public static string GetArrestTemplate(List<CrimePlayerReason> crimes, int jailCosts, int jailTime)
        {
            string returns = $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Sie wurden ins Gefängnis eingewiesen. Der Staat hat eine Haftzeit von {jailTime} Hafteinheiten und einen Haftbetrag von ${jailCosts} festgelegt!<br><br>" +
                $"<b>Begangene Straftaten:</b><br>";

            foreach (CrimePlayerReason crime in crimes)
            {
                returns += crime.Name + "<br>";
            }
            return returns;
        }*/
    }
}
