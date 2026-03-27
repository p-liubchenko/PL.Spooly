using System;
using System.Linq;

namespace Pricer;

public sealed class PrintTransactionsManager(IAppDataStore store, string dataFilePath)
{
	private readonly IAppDataStore _store = store;
	private readonly string _dataFilePath = dataFilePath;

	public void Menu(AppData appData) => throw new NotSupportedException("CLI is hosted in Pricer.Cli");

	public void RecordCompletedPrint(AppData appData, PrintCostRequest request, PrintCostResult result)
	{
		var printer = appData.GetSelectedPrinter();
		if (printer is null)
		{
			return;
		}

		var currencyId = appData.OperatingCurrencyId;
		var totalOriginal = Money.FromBase(appData, result.Total, currencyId);

		var tx = new PrintTransaction
		{
			Id = Guid.NewGuid(),
			CreatedAt = DateTimeOffset.UtcNow,
			Status = PrintTransactionStatus.Completed,
			MaterialId = request.Material.Id,
           MaterialNameSnapshot = string.IsNullOrWhiteSpace(request.Material.Color) ? request.Material.Name : $"{request.Material.Name} ({request.Material.Color})",
			FilamentKg = result.FilamentKg,
			EstimatedMetersUsed = result.EstimatedMetersUsed,
			PrinterId = printer.Id,
			PrinterNameSnapshot = printer.Name,
			PrintHours = request.PrintHours,
			MaterialCost = result.MaterialCost,
			ElectricityKwh = result.ElectricityKwh,
			ElectricityCost = result.ElectricityCost,
			PrinterWearCost = result.PrinterWearCost,
			FixedCost = result.FixedCost,
			ExtraFixedCost = result.ExtraFixedCost,
            OriginalCost = totalOriginal,
			TotalCost = totalOriginal
		};

		appData.PrintTransactions.Add(tx);
		_store.Save(_dataFilePath, appData);
	}

	public bool TryRevert(AppData appData, Guid transactionId, out string error)
	{
		error = string.Empty;
		var tx = appData.GetTransaction(transactionId);
		if (tx is null)
		{
			error = "Transaction not found.";
			return false;
		}

		if (tx.Status == PrintTransactionStatus.Reverted)
		{
			error = "Transaction already reverted.";
			return false;
		}

		var material = appData.FindMaterialById(tx.MaterialId);
		if (material is null)
		{
			error = "Material for this transaction no longer exists.";
			return false;
		}

		material.AmountKg += tx.FilamentKg;
		material.EstimatedLengthMeters += tx.EstimatedMetersUsed;

		tx.Status = PrintTransactionStatus.Reverted;
		tx.RevertedAt = DateTimeOffset.UtcNow;
		_store.Save(_dataFilePath, appData);
		return true;
	}

	public bool TryDelete(AppData appData, Guid transactionId, out string error)
	{
		error = string.Empty;
		var index = appData.PrintTransactions.FindIndex(x => x.Id == transactionId);
		if (index < 0)
		{
			error = "Transaction not found.";
			return false;
		}

		appData.PrintTransactions.RemoveAt(index);
		_store.Save(_dataFilePath, appData);
		return true;
	}
}
