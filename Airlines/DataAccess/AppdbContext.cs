using Microsoft.EntityFrameworkCore;
using Airlines.Models;

namespace Airlines.DataAccess
{
    public class AppdbContext(DbContextOptions<AppdbContext> options) : DbContext(options)
    {

        public DbSet<Transport> Transport { get; set; }
        public DbSet<Flight> Flight { get; set; }
        public DbSet<Journey> Journey { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}