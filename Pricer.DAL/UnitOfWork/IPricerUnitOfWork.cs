using Pricer.DAL.Repositories;
using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.DAL.UnitOfWork;

public interface IPricerUnitOfWork
{
	IRepository<Currency, Guid> Currencies { get; }
	IRepository<Printer, Guid> Printers { get; }
	IRepository<FilamentMaterial, Guid> Materials { get; }
	IRepository<PrintTransaction, Guid> PrintTransactions { get; }
	IRepository<StockTransaction, Guid> StockTransactions { get; }
	IRepository<AppSettings, Guid> Settings { get; }

	Task<int> SaveChangesAsync(CancellationToken ct = default);
}
