using Pricer.Abstractions;
using Pricer.Models;

namespace Pricer.Models.Transactions;

public abstract class TransactionBase : ITransaction
{
	public Guid Id { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public Money? TotalCost { get; set; }
	public Money? OriginalCost { get; set; }
}
