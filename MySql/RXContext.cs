using Backend.MySql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backend.MySql
{
    public class RXContext : DbContext
    {
        private string MySqlConnection;

        public RXContext() : this(Configuration.ConnectionString) { }

        public RXContext(string connection)
        {
            this.MySqlConnection = connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(this.MySqlConnection, ServerVersion.AutoDetect(this.MySqlConnection), ob => { ob.EnableRetryOnFailure(); ob.MigrationsAssembly(typeof(RXContext).GetTypeInfo().Assembly.GetName().Name); });
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => { builder.SetMinimumLevel(LogLevel.Error); }));
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors(true);
        }

        public DbSet<DbPlayer> Players { get; set; }
        public DbSet<DbCharacter> Characters { get; set; }
        public DbSet<DbItem> Items { get; set; }
        public DbSet<DbItemModel> ItemModels { get; set; }
        public DbSet<DbContainer> Containers { get; set; }
        public DbSet<DbBankAccount> BankAccounts { get; set; }
        public DbSet<DbBankHistory> BankHistories { get; set; }
        public DbSet<DbFemaleCloth> FemaleClothes { get; set; }
        public DbSet<DbMaleCloth> MaleClothes { get; set; }
        public DbSet<DbFemaleProp> FemaleProps { get; set; }
        public DbSet<DbMaleProp> MaleProps { get; set; }
        public DbSet<DbMask> Masks { get; set; }
        public DbSet<DbAttachment> Attachments { get; set; }
        public DbSet<DbVehicle> Vehicles { get; set; }
        public DbSet<DbTeamVehicle> TeamVehicles { get; set; }
        public DbSet<DbWardrobeItem> WardrobeItems { get; set; }
        public DbSet<DbWardrobeOutfit> WardrobeOutfits { get; set; }
        public DbSet<DbCryptoMarktOffers> CryptoMarktOffers { get; set; }
        public DbSet<DbTeamMemberData> TeamMemberDatas { get; set; }
        public DbSet<DbBusinessMemberData> BusinessMemberDatas { get; set; }
        public DbSet<DbPhoneSettings> PhoneSettings { get; set; }
        public DbSet<DbPhoneWallpaper> PhoneWallpaper { get; set; }
        public DbSet<DbPlayerCrimeData> PlayerCrimeData { get; set; }
        public DbSet<DbPlayerCrimes> PlayerCrimes { get; set; }
        public DbSet<DbPhoneContact> PhoneContacts { get; set; }
        public DbSet<DbPhoneConversation> PhoneConversations { get; set; }
        public DbSet<DbPhoneConversationMessage> PhoneConversationMessages { get; set; }
        public DbSet<DbAnimationCategory> AnimationCategories { get; set; }
        public DbSet<DbAnimationItem> AnimationItems { get; set; }
        public DbSet<DbEmail> Emails { get; set; }
        public DbSet<DbPlant> Plants { get; set; }
        public DbSet<DbGangwar> Gangwar { get; set; }
        public DbSet<DbFishing> Fishing { get; set; }
        public DbSet<DbLog> Logs { get; set; }
        public DbSet<DbPlayerXMAS> XMAS { get; set; }
        public DbSet<DbCrimes> Crimes { get; set; }
        public DbSet<DbNewCrimes> NewCrimes { get; set; }
        public DbSet<DbHouse> Houses { get; set; }
        public DbSet<DbHouseServer> HouseServers { get; set; }
        public DbSet<DbPaintball> PaintballMaps { get; set; }
        public DbSet<DbPaintballSpawnpoints> PaintballSpawnpoints { get; set; }
        public DbSet<DbHouseServerLogs> HouseServerLogs { get; set; }
        public DbSet<DbBlitzer> Blitzer { get; set; }
        public DbSet<DbFarming> Farming { get; set; }
        public DbSet<DbFarmingProcess> FarmingProcess { get; set; }
        public DbSet<DbWorkstation> Workstations { get; set; }
        public DbSet<DbPlayerLicenses> PlayerLicenses { get; set; }
        public DbSet<DbWorkstationProcess> WorkstationsProcess { get; set; }
        public DbSet<DbTeam> Teams { get; set; }
        public DbSet<DbFarmingPos> FarmingPos { get; set; }
        public DbSet<DbVehicleShop> VehicleShops { get; set; }
        public DbSet<DbVehicleShopOffers> VehicleShopsOffers { get; set; }
        public DbSet<DbVehicleShopSpawn> VehicleShopsSpawns { get; set; }
        public DbSet<DbVehicleModel> VehicleModels { get; set; }
        public DbSet<DbIdentifier> BlacklistedIdentifiers { get; set; }
        public DbSet<DbItemExport> ItemExports { get; set; }
        public DbSet<DbTeamGaragePoints> TeamGaragePoints { get; set; }
        public DbSet<DbTeamArmory> TeamArmory { get; set; }
        public DbSet<DbItemExportItem> ItemExportItems { get; set; }
        public DbSet<DbDoor> Doors { get; set; }
        public DbSet<DbWeaponLab> WeaponLabs { get; set; }
        public DbSet<DbMethLab> MethLabs { get; set; }
        public DbSet<DbPTA> PTA { get; set; }
        public DbSet<DbPTASettings> PTASettings { get; set; }
        public DbSet<DbBankRobbery> BankRobberies { get; set; }
        public DbSet<DbStorage> Storages { get; set; }
        public DbSet<DbLoadingscreenSongs> LoginSongs { get; set; }
        public DbSet<DbMetallDetector> MetallDetectors { get; set; }
        public DbSet<DbJumppoint> Jumppoints { get; set; }
        public DbSet<DbCustomsBill> PlayerLSCBills { get; set; }
        public DbSet<DbInjury> Injuries { get; set; }

        // public DbSet<DbPhoneCallHistory> PhoneCallHistories { get; set; }
        // public DbSet<DbShopProduct> ShopProducts { get; set; }
    }
}
