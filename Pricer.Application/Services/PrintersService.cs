using Pricer;
using Pricer.Models;

namespace Pricer.Application.Services;

public sealed class PrintersService(ICrudRepository<Printer, Guid> repo) : IPrintersService
{
	private readonly ICrudRepository<Printer, Guid> _repo = repo;

	public Task<List<Printer>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);
	public Task UpsertAsync(Printer printer, CancellationToken ct = default) => _repo.UpsertAsync(printer, ct);
	public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);
}
