using Backend.MySql;
using Backend.MySql.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.MemberData
{
    class MemberDataModule : RXModule
    {
        public MemberDataModule() : base("MemberData") { }

        public static List<DbTeamMemberData> TeamMemberDatas = new List<DbTeamMemberData>();
        public static List<DbBusinessMemberData> BusinessMemberDatas = new List<DbBusinessMemberData>();

        public static MemberDataModule Instance = new MemberDataModule();

        //[HandleExceptions]
        public override async Task OnTwoSecond()
        {
            using var db = new RXContext();

            TeamMemberDatas = await db.TeamMemberDatas.ToListAsync();
            BusinessMemberDatas = await db.BusinessMemberDatas.ToListAsync();
        }

        //[HandleExceptions]
        public static async Task RefreshMemberDataAsync()
        {
            using var db = new RXContext();

            TeamMemberDatas = await db.TeamMemberDatas.ToListAsync();
            BusinessMemberDatas = await db.BusinessMemberDatas.ToListAsync();
        }
    }
}
