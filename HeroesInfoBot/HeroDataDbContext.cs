using HeroesInfoBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesInfoBot
{
    public class HeroDataDbContext : DbContext
    {
        public DbSet<Hero> Hero { get; set; }
        public DbSet<Ability> Ability { get; set; }
        public DbSet<Talent> Talent { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=D:\GitHub Repos\HeroData.db");
        }
    }
}
