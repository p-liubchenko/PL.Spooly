using Pricer.Models;

using System;

namespace Pricer;

public sealed class StockTransactionsManager(IAppDataStore store, string dataFilePath)
{
	private readonly IAppDataStore _store = store;
	private readonly string _dataFilePath = dataFilePath;

  public void RecordSpoolPurchase(AppData appData, FilamentMaterial material, decimal kgAdded, decimal metersAdded, Money totalCost)
	{
		var tx = new StockTransaction
		{
			Id = Guid.NewGuid(),
			CreatedAt = DateTimeOffset.UtcNow,
			Type = StockTransactionType.SpoolPurchase,
			MaterialId = material.Id,
           MaterialNameSnapshot = string.IsNullOrWhiteSpace(material.Color) ? material.Name : $"{material.Name} ({material.Color})",
			KgDelta = kgAdded,
			MetersDelta = metersAdded,
			TotalCost = totalCost
		};

		appData.StockTransactions.Add(tx);
		_store.Save(_dataFilePath, appData);
	}
}
