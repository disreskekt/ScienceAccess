using Api.Extensions;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Task> Tasks { get; set; }

        public Context(DbContextOptions<Context> options)
            : base(options)
        {
            Database.Migrate();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role() {Id = 1, RoleName = "Admin"},
                new Role() {Id = 2, RoleName = "User"}
                );
            
            modelBuilder.EnumToStringConversion<Task, TaskStatuses>(t => t.Status);
        }
    }
}