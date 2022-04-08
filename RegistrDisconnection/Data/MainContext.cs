//using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;

namespace RegistrDisconnection.Data
{
    /// <summary>
    /// БД програми
    /// </summary>
    //public class MainContext : AuditDbContext
    public class MainContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Prava> Rights { get; set; }
        public DbSet<Organization> Coks { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Finance> Finances { get; set; }
        public DbSet<Lichylnyk> Lichylnyks { get; set; }
        public DbSet<Poperedgenia> Poperedgenias { get; set; }
        public DbSet<Vykl> Vykls { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<DirectionDict> DirectionDicts { get; set; }
        public DbSet<DirectionCityMap> DirectionCityMaps { get; set; }
        public DbSet<ActualDataPerson> ActualDatas { get; set; }
        public DbSet<VyhAndSviat> VyhAndSviats { get; set; }
        public DbSet<Options> Options { get; set; }
        public DbSet<PoperDrukGroup> DrukGroups { get; set; }
        public DbSet<UpdateFinance> UpdateFinances { get; set; }
        public DbSet<UpdateGroup> UpdateGroups { get; set; }
        public DbSet<Saldo> Saldos { get; set; }

        public MainContext(DbContextOptions<MainContext> options)
            : base(options)
        {
            //_ = Database.EnsureDeleted();     //видалення бд
            //_ = Database.EnsureCreated();     //створення БД без міграції
        }

    }
}
