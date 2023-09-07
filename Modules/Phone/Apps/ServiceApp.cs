using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Laptop.Apps;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.Modules.Phone.Apps
{
    class ServiceApp : RXModule
    {
        public ServiceApp() : base("ServiceApp", new RXWindow("ServiceSendRequestApp")) { }

        public static string GetFactionNameByDepartmentID(int department)
        {
            switch (department)
            {
                case 1:
                    return "Los Santos Police Department";
                case 2:
                    return "Los Santos Medical Center";
                case 20:
                    return "Department of Motor Vehicles";
                case 6:
                    return "Department of Public Order and Safety";
            }

            return "";
        }

        [RemoteEvent]
        public static async Task SendService(RXPlayer dbPlayer, uint department, string message)
        {
            if (dbPlayer == null) return;

            var requestSuccess = false;

            if (dbPlayer.HasData("CurrentServiceIndex") && dbPlayer.GetData<uint>("CurrentServiceIndex") > 0)
            {
                await dbPlayer.SendNotify("Du hast bereits einen Service abgesendet!");
                return;
            }

            var telnr = dbPlayer.Phone.ToString();
            string departmentFactionName = GetFactionNameByDepartmentID((int)department);
            if (departmentFactionName == "") return;

            message = replaceContent(message);

            switch (department)
            {
                case 1:
                    if (telnr == "0")
                    {
                        TeamModule.Teams.Find(x => x.Id == 1).SendNotification($"Es ist ein neuer Service mit dem Grund: {message} eingegangen!", 15000);
                    } 
                    else
                    {
                        TeamModule.Teams.Find(x => x.Id == 1).SendNotification($"Es ist ein neuer Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) mit dem Grund: {message} eingegangen!", 15000);
                    }

                    requestSuccess = true;
                    
                    if (requestSuccess)
                    {
                        Service service = new Service(await dbPlayer.GetPositionAsync(), message, 1, dbPlayer, "", telnr);
                        bool status = await ServiceListApp.Add(dbPlayer, 1, service);

                        dbPlayer.SetData<uint>("CurrentServiceIndex", 1);
                        dbPlayer.SetData<uint>("CurrentServiceTeamID", 1);

                        if (status)
                        {
                            await dbPlayer.SendNotify($"Du hast ein Service an {departmentFactionName} gesendet!", 8000, "lightblue", "SERVICE");
                        }
                    }

                    break;
                case 2:
                    if (await ServiceListApp.IsServiceInRangeOfTeam(3, await dbPlayer.GetPositionAsync()))
                    {
                        await dbPlayer.SendNotify("In deiner Umgebung wurde bereits ein Notruf gemeldet!");
                        return;
                    }
                    if (telnr == "0")
                    {
                        TeamModule.Teams.Find(x => x.Id == 3).SendNotification($"Es ist ein neuer Service mit dem Grund: {message} eingegangen!", 15000);
                    }
                    else
                    {
                        TeamModule.Teams.Find(x => x.Id == 3).SendNotification($"Es ist ein neuer Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) mit dem Grund: {message} eingegangen!", 15000);

                    }
                    requestSuccess = true;


                    if (requestSuccess)
                    {
                        Service service = new Service(await dbPlayer.GetPositionAsync(), message, 3, dbPlayer, "", telnr);
                        bool status = await ServiceListApp.Add(dbPlayer, 3, service);

                        dbPlayer.SetData<uint>("CurrentServiceIndex", 2);
                        dbPlayer.SetData<uint>("CurrentServiceTeamID", 3);

                        if (status)
                        {
                            await dbPlayer.SendNotify($"Du hast ein Service an {departmentFactionName} gesendet!", 8000, "lightblue", "SERVICE");
                        }
                    }

                    break;
                case 20:
                    if (telnr == "0")
                    {
                        TeamModule.Teams.Find(x => x.Id == 20).SendNotification($"Es ist ein neuer Service mit dem Grund: {message} eingegangen!", 15000);
                    }
                    else
                    {
                        TeamModule.Teams.Find(x => x.Id == 20).SendNotification($"Es ist ein neuer Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) mit dem Grund: {message} eingegangen!", 15000);

                    }

                    requestSuccess = true;


                    if (requestSuccess)
                    {
                        Service service = new Service(await dbPlayer.GetPositionAsync(), message, 20, dbPlayer, "", telnr);
                        bool status = await ServiceListApp.Add(dbPlayer, 20, service);

                        dbPlayer.SetData<uint>("CurrentServiceIndex", 20);
                        dbPlayer.SetData<uint>("CurrentServiceTeamID", 20);

                        if (status)
                        {
                            await dbPlayer.SendNotify($"Du hast ein Service an {departmentFactionName} gesendet!", 8000, "lightblue", "SERVICE");
                        }
                    }

                    break;
                case 6:

                    if (telnr == "0")
                    {
                        TeamModule.Teams.Find(x => x.Id == 6).SendNotification($"Es ist ein neuer Service mit dem Grund: {message} eingegangen!", 15000);
                    }
                    else
                    {
                        TeamModule.Teams.Find(x => x.Id == 6).SendNotification($"Es ist ein neuer Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) mit dem Grund: {message} eingegangen!", 15000);

                    }

                    requestSuccess = true;


                    if (requestSuccess)
                    {
                        Service service = new Service(await dbPlayer.GetPositionAsync(), message, 6, dbPlayer, "", telnr);
                        bool status = await ServiceListApp.Add(dbPlayer, 6, service);

                        dbPlayer.SetData<uint>("CurrentServiceIndex", 6);
                        dbPlayer.SetData<uint>("CurrentServiceTeamID", 6);

                        if (status)
                        {
                            await dbPlayer.SendNotify($"Du hast ein Service an {departmentFactionName} gesendet!", 8000, "lightblue", "SERVICE");
                        }
                    }

                    break;
            }
        }

        public static string replaceContent(string input)
        {
            input = input.Replace("\"", "");
            input = input.Replace("'", "");
            input = input.Replace("`", "");
            input = input.Replace("´", "");
            //return Regex.Replace(input, @"^[a-zA-Z0-9\s]+$", "");
            return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
        }

        [RemoteEvent]
        public async Task CancelService(RXPlayer dbPlayer, int serviceid)
        {
            if (dbPlayer == null) return;

            if (dbPlayer.HasData("CurrentServiceIndex") && dbPlayer.HasData("CurrentServiceTeamID"))
            {
                uint serviceDataIndex = dbPlayer.GetData<uint>("CurrentServiceIndex");
                if (serviceDataIndex == 0 || serviceDataIndex != serviceid) return;

                uint serviceDataTeamID = dbPlayer.GetData<uint>("CurrentServiceTeamID");
                if (serviceDataTeamID == 0) return;

                var telnr = dbPlayer.Phone.ToString();

                string departmentFactionName = GetFactionNameByDepartmentID(serviceid);
                if (departmentFactionName == "") return;

                bool status = await ServiceListApp.CancelOwnService(dbPlayer, serviceDataTeamID);

                if (status)
                {
                    switch (serviceid)
                    {
                        case 1:
                            TeamModule.Teams.Find(x => x.Id == 1).SendNotification($"Der Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) wurde abgebrochen!", 15000);
                            break;
                        case 2:
                            TeamModule.Teams.Find(x => x.Id == 3).SendNotification($"Der Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) wurde abgebrochen!", 15000);
                            break;
                        case 20:
                            TeamModule.Teams.Find(x => x.Id == 20).SendNotification($"Der Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) wurde abgebrochen!", 15000);
                            break;
                        case 6:
                            TeamModule.Teams.Find(x => x.Id == 6).SendNotification($"Der Service von {await dbPlayer.GetNameAsync()} Tel-NR: ({telnr}) wurde abgebrochen!", 15000);
                            break;
                    }

                    dbPlayer.ResetData("CurrentServiceIndex");
                    dbPlayer.ResetData("CurrentServiceTeamID");
                    await dbPlayer.SendNotify($"Du hast deinen Service bei/m {departmentFactionName} abgebrochen!", 8000, "red", "SERVICE");
                }
            }
        }
    }
}
