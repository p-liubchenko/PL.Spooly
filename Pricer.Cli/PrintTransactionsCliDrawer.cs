using System;
using System.Linq;

namespace Pricer;

public sealed class PrintTransactionsCliDrawer
{
	public void Menu(AppData appData, PrintTransactionsManager manager)
	{
		while (true)
		{
			Console.Clear();
			ConsoleEx.PrintHeader("Print History / Transactions");

			Console.WriteLine($"Transactions: {appData.PrintTransactions.Count}");
			Console.WriteLine();
			ConsoleEx.DrawMenuItem("1) List transactions");
			ConsoleEx.DrawMenuItem("2) Revert transaction");
			ConsoleEx.DrawMenuItem("3) Delete transaction", ConsoleEx.Severity.Critical);
			ConsoleEx.DrawMenuItem("0) Back");
			Console.WriteLine();

			switch (ConsoleEx.ReadMenuChoice("Choose an option"))
			{
				case "1":
                  ListAll(appData);
					break;
				case "2":
					Revert(appData, manager);
					break;
				case "3":
					Delete(appData, manager);
					break;
				case "0":
					return;
				default:
					ConsoleEx.ShowMessage("Unknown option.");
					break;
			}
		}
	}

  private static void ListAll(AppData appData)
	{
		Console.Clear();
        ConsoleEx.PrintHeader("Transactions");

		if (!appData.PrintTransactions.Any() && !appData.StockTransactions.Any())
		{
			Console.WriteLine("No transactions recorded.");
			ConsoleEx.Pause();
			return;
		}

        var printTx = appData.PrintTransactions.Select(tx => BuildPrintLine(appData, tx));
		var stockTx = appData.StockTransactions.Select(tx => BuildStockLine(appData, tx));

		var merged = printTx
			.Concat(stockTx)
			.OrderByDescending(x => x.CreatedAt)
			.Take(50)
			.ToList();

		for (int i = 0; i < merged.Count; i++)
		{
            RenderMergedLine(i + 1, merged[i]);
		}

		Console.WriteLine();
		Console.WriteLine("Showing latest 50.");
		ConsoleEx.Pause();
	}

	private static void RenderMergedLine(int index, TransactionLine line)
	{
		Console.Write($"{index}) {line.Prefix} | ");

		var kgColor = line.KgSeverity;
		ConsoleEx.WriteInline(line.KgText.PadLeft(9), kgColor);
		Console.Write(" | ");
		Console.Write(line.Tail);
		Console.Write(" | ");
		ConsoleEx.WriteInline(line.MoneyText, line.MoneySeverity);
		Console.WriteLine();
	}

	private readonly record struct TransactionLine(
		DateTimeOffset CreatedAt,
		string Prefix,
		string KgText,
		ConsoleEx.Severity KgSeverity,
		string Tail,
		string MoneyText,
		ConsoleEx.Severity MoneySeverity);

  private static TransactionLine BuildPrintLine(AppData appData, PrintTransaction tx)
	{
		var status = tx.Status == PrintTransactionStatus.Reverted ? "REVERTED" : "OK";
		var totalBase = tx.TotalCost?.ToBase(appData) ?? 0m;
        var moneyText = MoneyFormatter.Format(appData, totalBase);
		var materialDisplay = GetMaterialDisplayName(appData, tx.MaterialId, tx.MaterialNameSnapshot);

		return new TransactionLine(
			CreatedAt: tx.CreatedAt,
            Prefix: $"{tx.CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm} | PRINT | {status} | {materialDisplay}",
			KgText: $"-{tx.FilamentKg:F3} kg",
			KgSeverity: ConsoleEx.Severity.Critical,
			Tail: $"{tx.PrintHours:F2} h",
			MoneyText: moneyText,
			MoneySeverity: ConsoleEx.Severity.Unsafe);
	}

  private static TransactionLine BuildStockLine(AppData appData, StockTransaction tx)
	{
		var costBase = tx.TotalCost?.ToBase(appData) ?? 0m;
        var moneyText = MoneyFormatter.Format(appData, costBase);
        var materialDisplay = GetMaterialDisplayName(appData, tx.MaterialId, tx.MaterialNameSnapshot);
		var signKg = tx.KgDelta >= 0 ? "+" : "";
     var signM = tx.MetersDelta >= 0 ? "+" : "";
		var kgSeverity = tx.KgDelta >= 0 ? ConsoleEx.Severity.Safe : ConsoleEx.Severity.Critical;
		var moneySeverity = tx.Type == StockTransactionType.SpoolPurchase || tx.Type == StockTransactionType.Restock
			? ConsoleEx.Severity.Unsafe
			: ConsoleEx.Severity.Safe;

		return new TransactionLine(
			CreatedAt: tx.CreatedAt,
           Prefix: $"{tx.CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm} | STOCK | {tx.Type} | {materialDisplay}",
			KgText: $"{signKg}{tx.KgDelta:F3} kg",
			KgSeverity: kgSeverity,
			Tail: $"{signM}{tx.MetersDelta:F1} m",
			MoneyText: moneyText,
			MoneySeverity: moneySeverity);
	}

	private static string GetMaterialDisplayName(AppData appData, Guid materialId, string fallback)
	{
		var material = appData.Materials.FirstOrDefault(m => m.Id == materialId);
		if (material is null)
		{
			return fallback;
		}

		return string.IsNullOrWhiteSpace(material.Color) ? material.Name : $"{material.Name} ({material.Color})";
	}

	private static void Revert(AppData appData, PrintTransactionsManager manager)
	{
		Console.Clear();
		ConsoleEx.PrintHeader("Revert Transaction");

		if (!appData.PrintTransactions.Any())
		{
			ConsoleEx.ShowMessage("No transactions to revert.");
			return;
		}

		var list = appData.PrintTransactions.OrderByDescending(x => x.CreatedAt).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Console.WriteLine($"{i + 1}) {list[i].CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm} | {list[i].Status} | {list[i].MaterialNameSnapshot}");
		}

		var index = ConsoleEx.ReadInt("Select transaction", 1, list.Count) - 1;
		var tx = list[index];
     ConsoleEx.RequestConfirmation("Revert this transaction and restore stock?", ConsoleEx.Severity.Unsafe, () =>
		{
			if (!manager.TryRevert(appData, tx.Id, out var error))
			{
               ConsoleEx.ShowMessage(error, ConsoleEx.Severity.Unsafe);
				return;
			}

            ConsoleEx.ShowMessage("Transaction reverted (stock restored).", ConsoleEx.Severity.Unsafe);
		});
	}

	private static void Delete(AppData appData, PrintTransactionsManager manager)
	{
		Console.Clear();
		ConsoleEx.PrintHeader("Delete Transaction");

		if (!appData.PrintTransactions.Any())
		{
			ConsoleEx.ShowMessage("No transactions to delete.");
			return;
		}

		var list = appData.PrintTransactions.OrderByDescending(x => x.CreatedAt).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Console.WriteLine($"{i + 1}) {list[i].CreatedAt.LocalDateTime:yyyy-MM-dd HH:mm} | {list[i].Status} | {list[i].MaterialNameSnapshot}");
		}

		var index = ConsoleEx.ReadInt("Select transaction", 1, list.Count) - 1;
		var tx = list[index];
     ConsoleEx.RequestConfirmation("Delete this transaction?", ConsoleEx.Severity.Critical, () =>
		{
			if (!manager.TryDelete(appData, tx.Id, out var error))
			{
               ConsoleEx.ShowMessage(error, ConsoleEx.Severity.Critical);
				return;
			}

          ConsoleEx.ShowMessage("Transaction deleted.", ConsoleEx.Severity.Critical);
		});
	}
}
