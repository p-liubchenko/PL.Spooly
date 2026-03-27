using Pricer.DAL.Ef;
using Pricer.DAL.Repositories;
using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.DAL.UnitOfWork;

internal sealed class EfPricerUnitOfWork(PricerDbContext db) : IPricerUnitOfWork
{
	public IRepository<Currency, Guid> Currencies { get; } = new EfRepository<Currency, Guid>(db);
	public IRepository<Printer, Guid> Printers { get; } = new EfRepository<Printer, Guid>(db);
	public IRepository<FilamentMaterial, Guid> Materials { get; } = new EfRepository<FilamentMaterial, Guid>(db);
	public IRepository<PrintTransaction, Guid> PrintTransactions { get; } = new EfRepository<PrintTransaction, Guid>(db);
	public IRepository<StockTransaction, Guid> StockTransactions { get; } = new EfRepository<StockTransaction, Guid>(db);
	public IRepository<AppSettings, Guid> Settings { get; } = new EfRepository<AppSettings, Guid>(db);

	public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
