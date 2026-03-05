namespace Minigram.Core.ApplicationContext
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Models;

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Relation> Relationships { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relation>()
                .Navigation(r => r.Sender)
                .AutoInclude();
            
            modelBuilder.Entity<Relation>()
                .Navigation(r => r.Receiver)
                .AutoInclude();
            
            modelBuilder.Entity<User>()
                .Navigation(u => u.Profile)
                .AutoInclude();
        }
    }
}