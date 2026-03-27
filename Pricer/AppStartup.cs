using Pricer.Models;

using System;
using System.IO;
using System.Linq;

namespace Pricer;

public sealed class AppStartup(IAppDataStore store)
{
	private readonly IAppDataStore _store = store;

	public AppData LoadAndTreat(string dataFilePath)
	{
        MaybeMigrateFileToDb(dataFilePath);

		var appData = _store.Load(dataFilePath);
        if (!IsDbMode())
		{
			SeedDefaultsAndMigrate(appData);
			_store.Save(dataFilePath, appData);
		}
		return appData;
	}

	private bool IsDbMode()
	{
		var storeType = _store.GetType();
		return string.Equals(storeType.Name, "UnitOfWorkAppDataStore", StringComparison.Ordinal);
	}

	private void MaybeMigrateFileToDb(string dataFilePath)
	{
		if (!File.Exists(dataFilePath))
		{
			return;
		}

       if (!IsDbMode())
		{
			return;
		}

		var fileData = DataStore.Load(dataFilePath);
		if (fileData is null)
		{
			return;
		}

		SeedDefaultsAndMigrate(fileData);
		_store.Save(dataFilePath, fileData);
		ArchiveMigratedFile(dataFilePath);
	}

	private static void ArchiveMigratedFile(string dataFilePath)
	{
		try
		{
			var backupPath = dataFilePath + $".migrated.{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.bak";
			File.Move(dataFilePath, backupPath, overwrite: true);
		}
		catch
		{
			// best-effort
		}
	}

	public static void SeedDefaultsAndMigrate(AppData store)
	{
		if (store.Settings is null)
		{
         store.Settings = new AppSettings { Id = Guid.NewGuid() };
		}
		else if (store.Settings.Id == Guid.Empty)
		{
			store.Settings.Id = Guid.NewGuid();
		}

		if (!store.Printers.Any())
		{
			var defaultPrinter = new Printer
			{
				Id = Guid.NewGuid(),
				Name = "Default printer",
				AveragePowerWatts = 120m,
              HourlyCostMoney = new Money(5.00m, store.GetBaseCurrency()?.Id)
			};
			store.Printers.Add(defaultPrinter);
			store.SelectedPrinterId = defaultPrinter.Id;
		}

		foreach (var m in store.Materials)
		{
			if (m.Id == Guid.Empty)
			{
				m.Id = Guid.NewGuid();
			}
		}

		if (!store.Currencies.Any())
		{
			var defaultCurrency = new Currency
			{
				Id = Guid.NewGuid(),
				Code = "CZK",
				Value = 1m
			};
			store.Currencies.Add(defaultCurrency);
			store.OperatingCurrencyId = defaultCurrency.Id;
		}
		else
		{
			if (store.Currencies.Count(c => c.Value == 1m) == 0)
			{
				store.Currencies[0].Value = 1m;
			}
			else if (store.Currencies.Count(c => c.Value == 1m) > 1)
			{
				var first = true;
				foreach (var c in store.Currencies.Where(c => c.Value == 1m))
				{
					if (first)
					{
						first = false;
						continue;
					}
					c.Value = 0.0001m;
				}
			}

			if (store.OperatingCurrencyId is null || store.GetOperatingCurrency() is null)
			{
				store.OperatingCurrencyId = store.Currencies.FirstOrDefault()?.Id;
			}
		}

		DataTreatment(store);
	}

	private static void DataTreatment(AppData store)
	{
		var baseCurrencyId = store.GetBaseCurrency()?.Id;

        if (store.Settings.ElectricityPricePerKwhMoney.Amount == 0)
		{
          store.Settings.ElectricityPricePerKwhMoney = new Money(6.50m, baseCurrencyId);
		}

     if (store.Settings.FixedCostPerPrintMoney.Amount == 0)
		{
            store.Settings.FixedCostPerPrintMoney = new Money(0m, baseCurrencyId);
		}

		foreach (var p in store.Printers)
		{
         if (p.HourlyCostMoney.Amount == 0)
			{
                p.HourlyCostMoney = new Money(0m, baseCurrencyId);
			}
		}

		foreach (var m in store.Materials)
		{
           if (m.AveragePricePerKgMoney.Amount == 0)
			{
              m.AveragePricePerKgMoney = new Money(0m, baseCurrencyId);
			}
		}
	}
}
