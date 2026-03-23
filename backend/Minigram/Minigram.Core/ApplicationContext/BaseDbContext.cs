namespace Minigram.Core.ApplicationContext
{
    using Microsoft.EntityFrameworkCore;

    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options)
            : base(options) {}
    }
}