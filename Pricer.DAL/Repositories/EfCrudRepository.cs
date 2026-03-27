using Microsoft.EntityFrameworkCore;

namespace Pricer.DAL.Repositories;

public sealed class EfCrudRepository<TEntity, TKey>(DbContext db) : Pricer.ICrudRepository<TEntity, TKey>
	where TEntity : class
{
	private readonly DbContext _db = db;

	public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
		=> await _db.Set<TEntity>().FindAsync([id!], ct);

	public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
		=> await _db.Set<TEntity>().AsNoTracking().ToListAsync(ct);

	public Task UpsertAsync(TEntity entity, CancellationToken ct = default)
	{
		_ = ct;
		_db.Update(entity);
		return Task.CompletedTask;
	}

	public async Task DeleteAsync(TKey id, CancellationToken ct = default)
	{
		var entity = await GetByIdAsync(id, ct);
		if (entity is null)
		{
			return;
		}
		_db.Remove(entity);
	}
}
