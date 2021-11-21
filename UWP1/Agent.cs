using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UWP1
{
    public class AgentContext : DbContext
    {
        public DbSet<Agent> Agents { get; set; }

        public AgentContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agent>().Property(b => b.Name).IsRequired();
            modelBuilder.Entity<Agent>().Property(b => b.Name).HasMaxLength(25);

            modelBuilder.Entity<Agent>().Property(b => b.Phone).IsRequired();
            modelBuilder.Entity<Agent>().Property(b => b.Phone).HasMaxLength(11);

            modelBuilder.Entity<Agent>().Property(b => b.Email).HasMaxLength(25);

            modelBuilder.Entity<Agent>().Property(b => b.Photo).IsRequired();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=AgentDB.db");
        }
    }
    public class Agent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Photo { get; set; }

        public int PhotoWidth { get; set; }
        public int PhotoHeiht { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
