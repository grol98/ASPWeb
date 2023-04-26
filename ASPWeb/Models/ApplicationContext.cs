using Microsoft.EntityFrameworkCore;

namespace ASPWeb.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<Cards> Cards { get; set; } = null!;
        public DbSet<Controllers> Controllers { get; set; } = null!;
        public DbSet<Events> Events { get; set; } = null!;
        public DbSet<EventCodes> EventCodes { get; set; } = null!;
        public DbSet<Workers> Workers { get; set; } = null!;
        public DbSet<Groups> Groups { get; set; } = null!;
        public DbSet<AccessGroups> AccessGroups { get; set; } = null!;
        public DbSet<MessagesDB> Messages { get; set; } = null!;
        public DbSet<RelationsControllersAccessGroups> RelationsControllersAccessGroups { get; set; } = null!;
        public DbSet<RelationsControllersWorkers> RelationsControllersWorkers { get; set; } = null!;
        
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=192.168.1.6;Port=5432;Database=SKUD;Username=postgres;Password=1403413");
           // optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SKUD;Username=postgres;Password=2");
        }
    }
}