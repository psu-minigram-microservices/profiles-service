namespace Minigram.Profile
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Context;
    using Minigram.Profile.Models;

    public class ApplicationContext : BaseDbContext
    {
        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Relation> Relations { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relation>(entity =>
            {
                entity.HasOne(r => r.Sender)
                    .WithMany()
                    .HasForeignKey(r => r.SenderId)
                    .HasPrincipalKey(p => p.UserId);;
                
                entity.HasOne(r => r.Receiver)
                    .WithMany()
                    .HasForeignKey(r => r.ReceiverId)
                    .HasPrincipalKey(p => p.UserId);;

                entity.Navigation(r => r.Sender).AutoInclude();
                entity.Navigation(r => r.Receiver).AutoInclude();
            });
        }
    }
}