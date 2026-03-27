using Pricer.Models;

namespace Pricer.Application.Services;

public sealed class MaterialsService(ICrudRepository<FilamentMaterial, Guid> repo) : IMaterialsService
{
	private readonly ICrudRepository<FilamentMaterial, Guid> _repo = repo;

	public Task<List<FilamentMaterial>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);
	public Task UpsertAsync(FilamentMaterial material, CancellationToken ct = default) => _repo.UpsertAsync(material, ct);
	public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);
}
