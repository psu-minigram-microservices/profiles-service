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
    }
}