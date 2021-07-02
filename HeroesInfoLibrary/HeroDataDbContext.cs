using HeroesInfoBot.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroesInfoBot
{
    public class HeroDataDbContext : DbContext
    {
        public DbSet<Hero> Hero { get; set; }
        public DbSet<Ability> Ability { get; set; }
        public DbSet<Talent> Talent { get; set; }
        private string DatabaseLocation { get; set; }


        public HeroDataDbContext(string dbLocation)
        {
            DatabaseLocation = dbLocation;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseLocation}");
        }
    }
}
