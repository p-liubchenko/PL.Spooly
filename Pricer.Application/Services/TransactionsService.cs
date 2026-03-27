using Pricer.Models.Transactions;

namespace Pricer.Application.Services;

public sealed class TransactionsService(
	ICrudRepository<PrintTransaction, Guid> printRepo,
	ICrudRepository<StockTransaction, Guid> stockRepo,
    ICrudRepository<Pricer.Models.FilamentMaterial, Guid> materialRepo) : ITransactionsService
{
	private readonly ICrudRepository<PrintTransaction, Guid> _printRepo = printRepo;
	private readonly ICrudRepository<StockTransaction, Guid> _stockRepo = stockRepo;
  private readonly ICrudRepository<Pricer.Models.FilamentMaterial, Guid> _materialRepo = materialRepo;

	public Task<List<PrintTransaction>> GetPrintTransactionsAsync(CancellationToken ct = default) => _printRepo.GetAllAsync(ct);
	public Task<List<StockTransaction>> GetStockTransactionsAsync(CancellationToken ct = default) => _stockRepo.GetAllAsync(ct);

	public Task AddPrintTransactionAsync(PrintTransaction transaction, CancellationToken ct = default)
		=> _printRepo.UpsertAsync(transaction, ct);

	public Task AddStockTransactionAsync(StockTransaction transaction, CancellationToken ct = default)
		=> _stockRepo.UpsertAsync(transaction, ct);

  public Task UpsertMaterialAsync(Pricer.Models.FilamentMaterial material, CancellationToken ct = default)
		=> _materialRepo.UpsertAsync(material, ct);
}
