namespace Pricer.Application.Services.Stores;

public interface IAppStore
{
	Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
	Task SetAsync<T>(string key, T value, CancellationToken ct = default);
}
