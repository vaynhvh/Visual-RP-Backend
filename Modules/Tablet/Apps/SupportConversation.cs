using Backend.Controllers;
using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.Modules.Tablet.Apps
{
    public class Conversation
    {
        public RXPlayer Player { get; set; }

        public bool Receiver { get; set; }

        public string Message { get; set; }

        public DateTime Created_at { get; set; }

        public Conversation(RXPlayer player, bool receiver, string message)
        {
            Player = player;
            Receiver = receiver;
            Message = Regex.Replace(message, @"[^a-zA-Z0-9\s]", ""); ;
            Created_at = DateTime.Now;
        }
    }

    public class ConvObject
    {
        public List<ConversationObject> konversation { get; set; }

        public bool status { get; set; }
    }

    public class ConversationObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "sender")]
        public string Sender { get; set; }

        [JsonProperty(PropertyName = "receiver")]
        public bool Receiver { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }
    }

    class SupportConversation : RXModule
    {
        public SupportConversation() : base("SupportConversation", new RXWindow("SupportKonversation")) { }

        public static Dictionary<uint , List<Conversation>> conversations = new Dictionary<uint, List<Conversation>>();

        public static List<Conversation> GetTicketConversation(RXPlayer player)
        {
            List<Conversation> ticketConversation;

            if (!conversations.ContainsKey(player.Id))
            {
                ticketConversation = new List<Conversation>();
                conversations.Add(player.Id, ticketConversation);
            }
            else
            {
                ticketConversation = conversations[player.Id];
            }

            return ticketConversation;
        }

        public static void AddConversationMessage(RXPlayer player, Conversation conversationMessage)
        {
            var ticketConversation = GetTicketConversation(player);
            ticketConversation.Add(conversationMessage);
        }

        public static bool RemoveConversation(RXPlayer player)
        {
            var ticketConversation = GetTicketConversation(player);
            if (ticketConversation.Count == 0) return false;

            bool status = conversations.Remove(player.Id);
            return status;
        }

        [RemoteEvent]
        public async Task requestSupportKonversation(RXPlayer player, string name)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(name) || !player.InAduty) return;

            var target = await PlayerController.FindPlayerByStartsName(name, true);
            if (target == null) return;

            List<ConversationObject> conversationObjects = new List<ConversationObject>();
            var messages = GetTicketConversation(target);

            await messages.forEach(async message =>
                conversationObjects.Add(new ConversationObject { Id = message.Player.Id, Sender = await message.Player.GetNameAsync(), Receiver = message.Receiver, Message = message.Message, Date = message.Created_at }));

            var conversationClientObject = new ConvObject { konversation = conversationObjects, status = await SupportApp.getCurrentChatStatus(target) };

            await this.Window.TriggerEvent(player, "responseSupportKonversation", JsonConvert.SerializeObject(conversationClientObject));
        }

        [RemoteEvent]
        public async Task supportMessageSent(RXPlayer player, string name, string message)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(name) || !player.InAduty) return;

            var target = await PlayerController.FindPlayerByStartsName(name, true);
            if (target == null) return;

            Conversation conversationMessage = new Conversation(player, true, message);
            AddConversationMessage(target, conversationMessage);

            var conversationClientMessage = new ConversationObject { Id = conversationMessage.Player.Id, Sender = await conversationMessage.Player.GetNameAsync(), Receiver = conversationMessage.Receiver, Message = conversationMessage.Message, Date = conversationMessage.Created_at };

            await this.Window.TriggerEvent(player, "updateSupportKonversation", JsonConvert.SerializeObject(conversationClientMessage));

            await target.SendNotify("Antwort von " + await player.GetNameAsync() + ": " + message, 20000, "red", "Administration");
            await player.SendNotify("Die Antwort wurde an " + await target.GetNameAsync() + " gesendet.", 3500, "red", "Administration");
        }

        [RemoteEvent]
        public async Task allowTicketResponse(RXPlayer player, string name, bool status)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(name) || !player.InAduty) return;

            var target = await PlayerController.FindPlayerByStartsName(name, true);
            if (target == null) return;

            if (status)
            {
                await SupportApp.ChangeChatStatus(target, true);
                await target.SendNotify("Das Teammitglied " + await player.GetNameAsync() + " hat eine Konversation mit dir begonnen. Um zu kommunizieren nutze: /chat <Nachricht>", 15000, "red", "Administration");
            }
            else
            {
                await SupportApp.ChangeChatStatus(target, false);
                await target.SendNotify("Das Teammitglied " + await player.GetNameAsync() + " hat den Chat geschlossen! Der /chat Befehl steht dir nicht mehr zur Verfügung!", 10000, "red", "Administration");
            }
        }

        public static async Task SendMessage(RXPlayer player, string json)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(json)) return;

            var window = new RXWindow("SupportKonversation");

            await window.TriggerEvent(player, "updateSupportKonversation", json);
        }
    }
}
