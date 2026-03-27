using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer;

public interface IAppDataStore
{
	AppData Load(string filePath);
	void Save(string filePath, AppData data);
}

public interface ICrudRepository<TEntity, in TKey>
	where TEntity : class
{
	Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
	Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
	Task UpsertAsync(TEntity entity, CancellationToken ct = default);
	Task DeleteAsync(TKey id, CancellationToken ct = default);
}

public interface ISettingsService
{
	Task<AppSettings?> GetAsync(CancellationToken ct = default);
	Task UpsertAsync(AppSettings settings, CancellationToken ct = default);
}

public interface IPrintersService
{
 Task<List<Printer>> GetAllAsync(CancellationToken ct = default);
	Task UpsertAsync(Printer printer, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IMaterialsService
{
    Task<List<FilamentMaterial>> GetAllAsync(CancellationToken ct = default);
	Task UpsertAsync(FilamentMaterial material, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ICurrenciesService
{
    Task<List<Currency>> GetAllAsync(CancellationToken ct = default);
	Task UpsertAsync(Currency currency, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITransactionsService
{
  Task<List<PrintTransaction>> GetPrintTransactionsAsync(CancellationToken ct = default);
	Task<List<StockTransaction>> GetStockTransactionsAsync(CancellationToken ct = default);
	Task AddPrintTransactionAsync(PrintTransaction transaction, CancellationToken ct = default);
	Task AddStockTransactionAsync(StockTransaction transaction, CancellationToken ct = default);
	Task UpsertMaterialAsync(FilamentMaterial material, CancellationToken ct = default);
}
