using FireworkServices.Models;
using Microsoft.EntityFrameworkCore;

namespace FireworkServices.Context
{
    public class FireworkContext : DbContext
    {
        public FireworkContext(DbContextOptions<FireworkContext> options) : base(options)
        {

        }

        public DbSet<FireworkModel> Fireworks { get; set; }

    }
}