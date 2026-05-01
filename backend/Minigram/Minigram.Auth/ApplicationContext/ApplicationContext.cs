namespace Minigram.Auth
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.ApplicationContext;
    using Minigram.Auth.Models;

    public class ApplicationDbContext : BaseDbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<RefreshSession> RefreshSessions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                    : base(options) {}
        
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    base.OnModelCreating(modelBuilder);
        
                    modelBuilder.Entity<RefreshSession>(entity =>
                    {
                        entity.Property(e => e.CreatedAt)
                            .HasDefaultValueSql("NOW()")
                            .ValueGeneratedOnAdd();
        
                        entity.Property(e => e.UpdatedAt)
                            .HasDefaultValueSql("NOW()")
                            .ValueGeneratedOnAddOrUpdate();
                    });
                }
    }
}