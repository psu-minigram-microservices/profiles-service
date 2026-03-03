namespace Minigram.Core.Repositories
{
    using Minigram.Core.Models;

    public interface IRepository<TEntity> where TEntity : BaseModel
    {
        public IQueryable<TEntity> Get();

        public Task<int> Count();

        public Task Create(TEntity entity);

        public Task CreateRange(params TEntity[] entities);

        public void Update(TEntity entity);

        public void UpdateRange(params TEntity[] entities);

        public void Delete(TEntity entity);

        public void DeleteRange(params TEntity[] entities);

        public Task<int> SaveAsync();
    }
}