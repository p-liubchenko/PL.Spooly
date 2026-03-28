using System;
using System.Collections.Generic;
using System.Text;

namespace Spooly.Models;

public sealed class Printer
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public decimal AveragePowerWatts { get; set; }
	public Money HourlyCostMoney { get; set; }
}