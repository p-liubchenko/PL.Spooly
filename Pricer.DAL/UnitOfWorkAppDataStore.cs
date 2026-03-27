using Microsoft.EntityFrameworkCore;

using Pricer.DAL.Ef;

namespace Pricer.DAL;

public sealed class UnitOfWorkAppDataStore(PricerDbContext db) : IAppDataStore
{
  private readonly PricerDbContext _db = db;

	public AppData Load(string filePath)
	{
		_ = filePath;

		var settings = _db.Settings.AsNoTracking().FirstOrDefault();
		var printers = _db.Printers.AsNoTracking().ToList();
		var materials = _db.Materials.AsNoTracking().ToList();
		var currencies = _db.Currencies.AsNoTracking().ToList();
		var printTransactions = _db.PrintTransactions.AsNoTracking().ToList();
		var stockTransactions = _db.StockTransactions.AsNoTracking().ToList();

		return new AppData
		{
			Settings = settings,
			Printers = printers,
			Materials = materials,
			Currencies = currencies,
			PrintTransactions = printTransactions,
			StockTransactions = stockTransactions
		};
	}

	public void Save(string filePath, AppData data)
    {
		_ = filePath;
		Persist(_db, data);
		_db.SaveChanges();
	}

	private static void Persist(DbContext db, AppData data)
	{
      Upsert(db, data.Settings);
		UpsertRange(db, data.Currencies);
		UpsertRange(db, data.Printers);
		UpsertRange(db, data.Materials);
		UpsertRange(db, data.StockTransactions);
		UpsertRange(db, data.PrintTransactions);
	}

	private static void Upsert<TEntity>(DbContext db, TEntity? entity)
		where TEntity : class
	{
		if (entity is null)
		{
			return;
		}

		var entry = db.Entry(entity);
		if (entry.IsKeySet)
		{
			entry.State = EntityState.Modified;
		}
		else
		{
			db.Add(entity);
		}
	}

	private static void UpsertRange<TEntity>(DbContext db, IEnumerable<TEntity> entities)
		where TEntity : class
	{
		foreach (var e in entities)
		{
			Upsert(db, e);
		}
	}
}
