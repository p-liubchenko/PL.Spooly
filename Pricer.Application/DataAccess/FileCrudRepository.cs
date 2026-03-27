using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.Application.DataAccess;

internal sealed class FileCrudRepository<TEntity, TKey>(IAppDataStore store, string dataFilePath) : ICrudRepository<TEntity, TKey>
	where TEntity : class
{
	private readonly IAppDataStore _store = store;
	private readonly string _dataFilePath = dataFilePath;

	public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
	{
		_ = ct;
		return Task.FromResult(GetList().FirstOrDefault(e => GetId(e)!.Equals(id)));
	}

	public Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
	{
		_ = ct;
		return Task.FromResult(GetList().ToList());
	}

	public Task UpsertAsync(TEntity entity, CancellationToken ct = default)
	{
		_ = ct;
		var data = _store.Load(_dataFilePath);
		var list = GetList(data);
		var id = GetId(entity);
		var idx = list.FindIndex(e => Equals(GetId(e), id));
		if (idx >= 0)
		{
			list[idx] = entity;
		}
		else
		{
			list.Add(entity);
		}
		_store.Save(_dataFilePath, data);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(TKey id, CancellationToken ct = default)
	{
		_ = ct;
		var data = _store.Load(_dataFilePath);
		var list = GetList(data);
		var idx = list.FindIndex(e => GetId(e)!.Equals(id));
		if (idx >= 0)
		{
			list.RemoveAt(idx);
			_store.Save(_dataFilePath, data);
		}
		return Task.CompletedTask;
	}

	private List<TEntity> GetList() => GetList(_store.Load(_dataFilePath));

	private static List<TEntity> GetList(AppData data)
	{
		object list = typeof(TEntity).Name switch
		{
         nameof(Pricer.Models.Printer) => data.Printers,
			nameof(Pricer.Models.FilamentMaterial) => data.Materials,
			nameof(Currency) => data.Currencies,
			nameof(PrintTransaction) => data.PrintTransactions,
			nameof(StockTransaction) => data.StockTransactions,
			nameof(Pricer.Models.AppSettings) => new List<Pricer.Models.AppSettings> { data.Settings },
			_ => throw new NotSupportedException($"Unsupported entity type '{typeof(TEntity).Name}'.")
		};
		return (List<TEntity>)list;
	}

	private static object? GetId(TEntity entity)
	{
		var prop = typeof(TEntity).GetProperty("Id");
		return prop?.GetValue(entity);
	}
}
