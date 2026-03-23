namespace Minigram.Profile.ApplicationContext
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.ApplicationContext;
    using Minigram.Profile.ApplicationContext.Models;

    public class ApplicationDbContext : BaseDbContext
    {
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Relation> Relations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relation>(entity =>
            {
                entity.Navigation(r => r.Sender).AutoInclude();
                entity.Navigation(r => r.Receiver).AutoInclude();
            });
        }
    }
}