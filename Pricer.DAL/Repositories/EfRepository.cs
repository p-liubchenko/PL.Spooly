using Microsoft.EntityFrameworkCore;

using Pricer.DAL.Ef;

namespace Pricer.DAL.Repositories;

internal class EfRepository<TEntity, TKey>(PricerDbContext db) : IRepository<TEntity, TKey>
	where TEntity : class
{
	protected readonly PricerDbContext Db = db;

	public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
		=> await Db.Set<TEntity>().FindAsync([id!], ct);

	public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
		=> await Db.Set<TEntity>().AddAsync(entity, ct);

	public virtual void Remove(TEntity entity)
		=> Db.Set<TEntity>().Remove(entity);

	public virtual IQueryable<TEntity> Query() => Db.Set<TEntity>().AsQueryable();
}
