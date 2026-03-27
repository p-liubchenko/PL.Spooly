using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.Application.Services.Stores;

public sealed class AppDataStoreAdapter(IAppDataStore store, string dataFilePath) : IAppStore
{
	private readonly IAppDataStore _store = store;
	private readonly string _dataFilePath = dataFilePath;

	public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
	{
		_ = ct;
		var data = _store.Load(_dataFilePath);
		return Task.FromResult(GetFromData<T>(data, key));
	}

	public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
	{
		_ = ct;
		var data = _store.Load(_dataFilePath);
		SetToData(data, key, value);
		_store.Save(_dataFilePath, data);
		return Task.CompletedTask;
	}

	private static T? GetFromData<T>(AppData data, string key)
	{
		return key switch
		{
			"Settings" => (T?)(object?)data.Settings,
			"Printers" => (T?)(object?)data.Printers,
			"Materials" => (T?)(object?)data.Materials,
			"Currencies" => (T?)(object?)data.Currencies,
			"PrintTransactions" => (T?)(object?)data.PrintTransactions,
			"StockTransactions" => (T?)(object?)data.StockTransactions,
			_ => default
		};
	}

	private static void SetToData<T>(AppData data, string key, T value)
	{
		switch (key)
		{
			case "Settings":
             data.Settings = (Pricer.Models.AppSettings)(object)value!;
				break;
			case "Printers":
               data.Printers = (List<Pricer.Models.Printer>)(object)value!;
				break;
			case "Materials":
             data.Materials = (List<Pricer.Models.FilamentMaterial>)(object)value!;
				break;
			case "Currencies":
                data.Currencies = (List<Currency>)(object)value!;
				break;
			case "PrintTransactions":
             data.PrintTransactions = (List<PrintTransaction>)(object)value!;
				break;
			case "StockTransactions":
             data.StockTransactions = (List<StockTransaction>)(object)value!;
				break;
			default:
				throw new NotSupportedException($"Unknown key: '{key}'.");
		}
	}
}
