using Backend.Utils;
using Backend.Utils.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace Backend.Modules.Discord
{

    public class FooterObject
    {
        public string text { get; set; }
        public string icon_url { get; set; }


    }

    public class EmbedObject
    {
        public string title { get; set; }
        public string description { get; set; }
        public string color { get; set; } = "3447003";
        public string timestamp { get; set; }
        public string webhook { get; set; }
        public FooterObject footer { get; set; } = new FooterObject{   text = "Visual Roleplay", icon_url = "https://media.discordapp.net/attachments/1139567964006395914/1142552379015823410/Visual-orange-removebg-preview.png" };

    }
    class DiscordModule : RXModule
    {
        public DiscordModule() : base("Discord") { }

        public static string InventoryWebhook = "https://discord.com/api/webhooks/1142553339649863700/iczonJv8oPfH8duhQ9jhwC2dW9QpXHJd3MstBU7bD9FsV0EyjPa5azw2lTTEwV2uMCGp";
        public static string CommandWebhook = "https://discord.com/api/webhooks/1142553401750720604/aW3YheZdd3boXzxZSW_svNm3AcX-jy2ODApaPNZ4k2NQNGXWjpNcmRZXI42dPrukUlGe";

        public static string FFACreate = "https://discord.com/api/webhooks/1142553446797557891/rd72gnWVQzgXouiwG856Ypi2Sg3xJFCh5GiNLmgL87B0JJwIvhGoGTPeybpufX6oP_do";
        public static string BanKick = "https://discord.com/api/webhooks/1142553486941224992/gUWrN7a84KuSl5Pmepx_PI6Z4jjxWfA-ynt15PoeTrq8qpW583dSgkboN0oCWL_THtI5";
        public static string Bank = "https://discord.com/api/webhooks/1142553539164524675/MreXZP--rLYiMjhgIRH3FLFMcPLrlZES_7eN3hBHX1j5mPx7vTQinTegwb_3qU8H5DvU";
        public static string Blitzer = "https://discord.com/api/webhooks/1142553606722166846/vBYB8Jn_Y7k-3gSn6mCO0BLMNxsVzOaixhOl_s5nCFJUmXFlV7eAQ42XeB4JHf2YJdQZ";
        public static string Login = "https://discord.com/api/webhooks/1142553664507088936/TTxAMimmofUoGuYigKFK-yw-ieeQWXDvFyVcsz_O9s2sao8fMLA79jRuLFTGJ6sqIid1";
        public static string Startup = "https://discord.com/api/webhooks/1142552002254098453/NkpeDRWZA4edYPkZUoZfAwj5_tLCKr-kfq2tCtWwVQWZZpaJJPbHmjxsTudAigymWg4w";
        public static string Errors = "https://discord.com/api/webhooks/1143229263651291166/LSumh_lh68XqHfgn1t_lhr8He7HUFScdvqa4jdkA9W6UDETiQsBuqp3Dmd1iBrAKGkh_";
        public static string Setfrak = "https://discord.com/api/webhooks/1143392681066774578/7i4J7cqeaMw9xeRasMMeGlaWN8zySlgT3oUUCZUH_tYMbXMhnbfrb7bIqg49r4IpoMLc";
        public static string abuse = "https://discord.com/api/webhooks/1143413206254108682/Z99OmbydeMxXJH9w9u8s1MWfPE8cMO-ew8Q6cQSAvFJplHCtcjLUbr92mKcxtjwD5q26";

        public static List<DiscordLog> Logs = new List<DiscordLog>();
        public static Dictionary<string, List<EmbedObject>> Embeds = new Dictionary<string, List<EmbedObject>>();



        public override async Task OnFiveSecond()
        {
            await DiscordTick();
        }

        public async Task DiscordTick()
        {
            await Logs.forEachAlternativeAsync(log =>
            {
                //   await SendMessage(log.Title, log.Message, log.Time, log.Webhook);


                if (Embeds.ContainsKey(log.Webhook))
                {
                    Embeds.TryGetValue(log.Webhook, out var embed);
                    embed.Add(new EmbedObject { title = log.Title, description = log.Message, timestamp = log.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz") });
                }
                else
                {
                    Embeds.Add(log.Webhook, new List<EmbedObject> { new EmbedObject { title = log.Title, description = log.Message, timestamp = log.Time.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz") } });
                }
            });
            await SendMessage();


            Embeds.Clear();
            Logs.Clear();
        }

        public async Task SendMessage()
        {
            try
            {
                foreach (var log in Embeds)
                {
                    WebRequest wr = (HttpWebRequest)WebRequest.Create(log.Key);
                    wr.ContentType = "application/json";
                    wr.Method = "POST";

                    using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
                    {
                        string json = JsonConvert.SerializeObject(new
                        {
                            username = "Visual Roleplay",
                            avatar_url = "https://media.discordapp.net/attachments/1139567964006395914/1142552379015823410/Visual-orange-removebg-preview.png",
                            embeds = log.Value
        
                        });

                        sw.Write(json);
                    }

                    var response = await wr.GetResponseAsync();
                }
            }
            catch (Exception ex)
            {
                RXLogger.Print(ex.Message);
            }
        }
    }
}
