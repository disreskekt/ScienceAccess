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
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = 1,
                    Email = "base@base.base",
                    Password = "qwerty",
                    Name = "Base",
                    Lastname = "Admin",
                    RoleId = 1
                },
                new User()
                {
                    Id = 2,
                    Email = "init@init.init",
                    Password = "qwerty",
                    Name = "Init",
                    Lastname = "User",
                    RoleId = 2
                }
            );
            
            modelBuilder.EnumToStringConversion<Task, TaskStatuses>(t => t.Status);
        }
    }
}