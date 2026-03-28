using Spooly.Abstractions;

using System;
using System.Linq;

namespace Spooly.Models.Transactions;

public enum StockTransactionType
{
	SpoolPurchase,
	ManualConsume,
	PrintConsume,
	Restock
}

public sealed class StockTransaction: TransactionBase
{
	public StockTransactionType Type { get; set; }

	public Guid MaterialId { get; set; }
	public string MaterialNameSnapshot { get; set; } = string.Empty;

	// Positive for adding stock, negative for consuming.
	public decimal KgDelta { get; set; }
	public decimal MetersDelta { get; set; }

	public string? Note { get; set; }
}

public static class StockTransactionExtensions
{
	public static StockTransaction? GetStockTransaction(this AppData data, Guid id)
		=> data.StockTransactions.FirstOrDefault(x => x.Id == id);
}
