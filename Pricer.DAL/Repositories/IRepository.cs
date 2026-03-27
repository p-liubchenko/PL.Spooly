namespace Pricer.DAL.Repositories;

public interface IRepository<TEntity, in TKey>
	where TEntity : class
{
	Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
	Task AddAsync(TEntity entity, CancellationToken ct = default);
	void Remove(TEntity entity);
	IQueryable<TEntity> Query();
}
