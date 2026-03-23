namespace Minigram.Core.ApplicationContext.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.ApplicationContext.Models;

    public class BaseRepository<TEntity> : IRepository<TEntity>
        where TEntity : BaseModel
    {
        private readonly BaseDbContext _context;

        public BaseRepository(BaseDbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        public async Task<int> Count()
        {
            return await _context.Set<TEntity>().CountAsync();
        }

        public async Task Create(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task CreateRange(params TEntity[] entities)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void UpdateRange(params TEntity[] entities)
        {
            _context.Set<TEntity>().UpdateRange(entities);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void DeleteRange(params TEntity[] entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}