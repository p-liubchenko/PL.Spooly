using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Spooly.Models;

namespace Spooly.DAL.Ef.Configurations;

internal sealed class PrinterConfiguration : IEntityTypeConfiguration<Printer>
{
	public void Configure(EntityTypeBuilder<Printer> builder)
	{
		builder.ToTable("Printers");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
		builder.Property(x => x.AveragePowerWatts).HasColumnType("decimal(18,3)");

		builder.Property(x => x.HourlyCostMoney)
			.HasConversion(new Converters.MoneyJsonConverter())
			.HasColumnType("nvarchar(max)");
	}
}
