using System;
using FireworkServices.Models;
using Microsoft.EntityFrameworkCore;

namespace FireworkServices.Context
{
    public class FireworkContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("SQLServer"));
        }

        public DbSet<FireworkModel> Fireworks { get; set; }

    }
}