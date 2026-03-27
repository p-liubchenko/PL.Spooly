using Pricer.Models;

using System;
using System.Collections.Generic;
using System.Text;

namespace Pricer.Abstractions;

public interface ITransaction
{
	public Guid Id { get; set; }
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>
	/// Stors total cost of the transaction at the time of creation, 
	/// used for reporting and reverting transactions to original cost if needed
	/// To display will be converted to operating currency if needed, but stored as is (amount + currency)
	/// </summary>
	public Money? TotalCost { get; set; }
	
	/// <summary>
	/// stors value at the time of transaction creation, to be displayed as is (amount and currency)
	/// </summary>
	public Money? OriginalCost {  get; set; }
}
