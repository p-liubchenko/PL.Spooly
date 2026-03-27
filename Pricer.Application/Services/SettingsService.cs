using Pricer;
using Pricer.Models;

namespace Pricer.Application.Services;

public sealed class SettingsService(ICrudRepository<AppSettings, Guid> repo) : ISettingsService
{
	private readonly ICrudRepository<AppSettings, Guid> _repo = repo;

	public async Task<AppSettings?> GetAsync(CancellationToken ct = default)
		=> (await _repo.GetAllAsync(ct)).FirstOrDefault();

	public Task UpsertAsync(AppSettings settings, CancellationToken ct = default)
		=> _repo.UpsertAsync(settings, ct);
}
