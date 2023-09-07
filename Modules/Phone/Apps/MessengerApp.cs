using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class ClientConversation
    {
        [JsonProperty(PropertyName = "i")]
        public uint ConversationId { get; set; }

        [JsonProperty(PropertyName = "messageSender")]
        public string ConversationPartnerName { get; set; }

        [JsonProperty(PropertyName = "n")]
        public uint ConversationPartnerNumber { get; set; }

        [JsonProperty(PropertyName = "de")]
        public DateTime ConversationUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "d")]
        public long DateData
        {
            get => ((DateTimeOffset)ConversationUpdatedTime).ToUnixTimeSeconds();
        }

        [JsonIgnore]
        public List<ClientConversationMessage> ConversationMessages { get; set; }
    }

    public class ClientConversationMessage
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string MessageSenderName { get; set; }

        [JsonProperty(PropertyName = "de")]
        public DateTime ConversationMessageUpdatedTime { get; set; }

        [JsonProperty(PropertyName = "m")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "r")]
        public string Receiver { get; set; }


        [JsonProperty(PropertyName = "d")]
        public long DateData
        {
            get => ((DateTimeOffset)ConversationMessageUpdatedTime).ToUnixTimeSeconds();
        }
    }

    class MessengerApp : RXModule
    {
        public MessengerApp() : base("MessengerApp", new RXWindow("MessengerListApp")) { }

        //[HandleExceptions]
        public static string GetUpdatedTimeFormated(DateTime dateTime, bool detailed = false)
        {
            string reverseDate = dateTime.Day + "/" + dateTime.Month;
            if (dateTime.AddDays(1) > DateTime.Now) // innerhalb 1 Day
            {
                reverseDate = "Heute";
            }
            else if (dateTime.AddDays(2) > DateTime.Now && dateTime.AddDays(3) < DateTime.Now)
            {
                reverseDate = "Gestern";
            }

            string result = reverseDate;
            if (detailed)
            {
                result = (dateTime.AddMinutes(1) > DateTime.Now) ? "Jetzt" : reverseDate + " " + dateTime.Hour + ":" + dateTime.Minute;
            }

            return result;
            // Heute 12:40 || Gestern 12:40 || 12.07 12:40 || Jetzt
        }
        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task RqChat(RXPlayer player, uint number)
        {
            if (!player.CanInteract()) return;

            using var db = new RXContext();


            var conversation = await db.PhoneConversations.FirstOrDefaultAsync(x => x.Id == number);

           
                ClientConversation clientConversatione = new ClientConversation
                {
                    ConversationId = conversation.Id,
                    ConversationPartnerName = ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1),
                    ConversationPartnerNumber = conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1,
                    ConversationUpdatedTime = conversation.LastUpdated,
                    ConversationMessages = new List<ClientConversationMessage>()
                };

                var messagese = await db.PhoneConversationMessages.Where(x => x.ConversationId == clientConversatione.ConversationId).ToListAsync();

                foreach (var message in messagese)
                {
                    string l_Message = message.Message;
                    l_Message = l_Message.Replace("\"", "");
                    l_Message = l_Message.Replace("'", "");
                    l_Message = l_Message.Replace("`", "");
                    l_Message = l_Message.Replace("´", "");

                    var conversationMessage = new ClientConversationMessage
                    {
                        Id = conversation.Id,
                        Message = l_Message,
                        MessageSenderName = ContactsApp.GetContactName(player, message.SenderId),
                        Receiver = message.SenderId == player.Phone ? ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1) : "Ich",
                        ConversationMessageUpdatedTime = message.TimeStamp,
                    };

                    clientConversatione.ConversationMessages.Add(conversationMessage);
                }

           await player.TriggerEventAsync("RsChat", NAPI.Util.ToJson(clientConversatione.ConversationMessages));

            


        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task RqChats(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            using var db = new RXContext();

            List<ClientConversation> clientConversations = new List<ClientConversation>();

            var conversationList = await db.PhoneConversations.Where(x => x.Player1 == player.Phone || x.Player2 == player.Phone).ToListAsync();

            foreach (DbPhoneConversation conversation in conversationList)
            {
                ClientConversation clientConversation = new ClientConversation
                {
                    ConversationId = conversation.Id,
                    ConversationPartnerName = ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1),
                    ConversationPartnerNumber = conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1,
                    ConversationUpdatedTime = conversation.LastUpdated,
                    ConversationMessages = new List<ClientConversationMessage>()
                };

                var messages = await db.PhoneConversationMessages.Where(x => x.ConversationId == clientConversation.ConversationId).ToListAsync();

                foreach (var message in messages)
                {
                    string l_Message = message.Message;
                    l_Message = l_Message.Replace("\"", "");
                    l_Message = l_Message.Replace("'", "");
                    l_Message = l_Message.Replace("`", "");
                    l_Message = l_Message.Replace("´", "");

                    var conversationMessage = new ClientConversationMessage
                    {
                        Id = conversation.Id,
                        Message = l_Message,
                        MessageSenderName = ContactsApp.GetContactName(player, message.SenderId),
                        Receiver = message.SenderId == player.Phone ? ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1) : "Ich",
                        ConversationMessageUpdatedTime = message.TimeStamp,
                    };

                    clientConversation.ConversationMessages.Add(conversationMessage);
                }

                clientConversations.Add(clientConversation);
            }

            clientConversations.Sort((a, b) =>
            {
                DateTime l_FirstElement = a.ConversationUpdatedTime;
                DateTime l_SecondElement = b.ConversationUpdatedTime;

                return l_SecondElement.CompareTo(l_FirstElement);
            });

            foreach (var conversation in clientConversations)
            {
                DateTime l_Date = conversation.ConversationUpdatedTime;
                conversation.ConversationUpdatedTime = l_Date;
            }

            await player.TriggerEventAsync("RsChats", JsonConvert.SerializeObject(clientConversations));
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task deletePhoneChat(RXPlayer player, uint conversationId)
        {
            if (!player.CanInteract()) return;

            using var db = new RXContext();

            var conversation = await db.PhoneConversations.FirstOrDefaultAsync(x => x.Id == conversationId);
            if (conversation == null) return;

            db.PhoneConversationMessages.RemoveRange(db.PhoneConversationMessages.Where(x => x.ConversationId == conversation.Id));
            db.PhoneConversations.Remove(conversation);

            await db.SaveChangesAsync();
        }


        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task CreateChat(RXPlayer player, uint number, string message)
        {

            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            message = message.Replace("\"", "");
            message = message.Replace("'", "");
            message = message.Replace("`", "");
            message = message.Replace("´", "");

            using var db = new RXContext();

            DbPhoneConversation conversation = new DbPhoneConversation
            {
                Id = await db.PhoneConversations.CountAsync() == 0 ? 1 : (await db.PhoneConversations.MaxAsync(con => con.Id) + 1),
                Player2 = number,
                Player1 = player.Phone,
                LastUpdated = DateTime.Now
            };

            await db.PhoneConversations.AddAsync(conversation);


            DbPhoneConversationMessage conversationMessage = new DbPhoneConversationMessage
            {
                Id = await db.PhoneConversationMessages.CountAsync() == 0 ? 1 : (await db.PhoneConversationMessages.MaxAsync(con => con.Id) + 1),
                ConversationId = conversation.Id,
                SenderId = player.Phone,
                Message = message,
                TimeStamp = DateTime.Now
            };

            await db.PhoneConversationMessages.AddAsync(conversationMessage);

            ClientConversationMessage clientConversationMessage = new ClientConversationMessage
            {
                Id = conversationMessage.Id,
                ConversationMessageUpdatedTime = DateTime.Now,
                Message = message,
                Receiver = ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1),
                MessageSenderName = "Ich"
            };

            uint partner = conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1;

            var target = await PlayerController.FindPlayerByPhoneNumber(partner);
            if (target != null && target.PhoneSettings != null && !target.PhoneSettings.FlyMode)
            {
                await target.SendNotify("Neue SMS von: " + ContactsApp.GetContactName(target, player.Phone));

                clientConversationMessage.Receiver = "Ich";
                clientConversationMessage.MessageSenderName = ContactsApp.GetContactName(target, player.Phone);

            }
            else
            {
                await player.SendNotify("SMS versendet.");
            }

            await db.SaveChangesAsync();
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task SendChat(RXPlayer player, uint convid, string message)
        {

            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            message = message.Replace("\"", "");
            message = message.Replace("'", "");
            message = message.Replace("`", "");
            message = message.Replace("´", "");

            using var db = new RXContext();


            var conversation = await db.PhoneConversations.FirstOrDefaultAsync(x => x.Id == convid);
            if (conversation == null)
            {
                /*            conversation = new DbPhoneConversation
                            {
                                Id = await db.PhoneConversations.CountAsync() == 0 ? 1 : (await db.PhoneConversations.MaxAsync(con => con.Id) + 1),
                                Player1 = player.Phone,
                                Player2 = ,
                                LastUpdated = DateTime.Now
                            };

                            await db.PhoneConversations.AddAsync(conversation);
                            await db.SaveChangesAsync();*/
                return;
         }

            DbPhoneConversationMessage conversationMessage = new DbPhoneConversationMessage
            {
                Id = await db.PhoneConversationMessages.CountAsync() == 0 ? 1 : (await db.PhoneConversationMessages.MaxAsync(con => con.Id) + 1),
                ConversationId = conversation.Id,
                SenderId = player.Phone,
                Message = message,
                TimeStamp = DateTime.Now
            };

            await db.PhoneConversationMessages.AddAsync(conversationMessage);

            ClientConversationMessage clientConversationMessage = new ClientConversationMessage
            {
                Id = conversationMessage.Id,
                ConversationMessageUpdatedTime = DateTime.Now,
                Message = message,
                Receiver = ContactsApp.GetContactName(player, conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1),
                MessageSenderName = "Ich"
            };

            uint partner = conversation.Player1 == player.Phone ? conversation.Player2 : conversation.Player1;

            var target = await PlayerController.FindPlayerByPhoneNumber(partner);
            if (target != null && target.PhoneSettings != null && !target.PhoneSettings.FlyMode)
            {
                await target.SendNotify("Neue SMS von: " + ContactsApp.GetContactName(target, player.Phone));

                clientConversationMessage.Receiver = "Ich";
                clientConversationMessage.MessageSenderName = ContactsApp.GetContactName(target, player.Phone);


                await target.TriggerEventAsync("RsRmChat", clientConversationMessage.Id, clientConversationMessage.Receiver, clientConversationMessage.Message, clientConversationMessage.DateData);
            }
            else
            {
                await player.SendNotify("SMS versendet.");
            }

            await db.SaveChangesAsync();
        }
    }
}
