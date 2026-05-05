using Microsoft.EntityFrameworkCore;
using SaigonRide.Models.Entities;

namespace SaigonRide.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
    }
}