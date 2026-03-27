using Pricer.Models;

namespace Pricer.Application.Services;

public sealed class CurrenciesService(ICrudRepository<Currency, Guid> repo) : ICurrenciesService
{
	private readonly ICrudRepository<Currency, Guid> _repo = repo;

	public Task<List<Currency>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);
	public Task UpsertAsync(Currency currency, CancellationToken ct = default) => _repo.UpsertAsync(currency, ct);
	public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);
}
