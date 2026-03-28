using Spooly.Abstractions;
using Spooly.Models;

namespace Spooly.Models.Transactions;

public abstract class TransactionBase : ITransaction
{
	public Guid Id { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public Money? TotalCost { get; set; }
	public Money? OriginalCost { get; set; }
}
