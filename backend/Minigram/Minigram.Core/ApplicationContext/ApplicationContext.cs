namespace Minigram.Core.ApplicationContext
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Models;

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Relationship> Relationships { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options) {}
    }
}