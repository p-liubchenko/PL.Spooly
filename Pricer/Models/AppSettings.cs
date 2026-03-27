using System;
using System.Collections.Generic;
using System.Text;

namespace Pricer.Models;

public sealed class AppSettings
{
	public Money ElectricityPricePerKwhMoney { get; set; }
	public Money FixedCostPerPrintMoney { get; set; }
}