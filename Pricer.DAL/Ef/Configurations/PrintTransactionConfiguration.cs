using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Pricer.Models.Transactions;

namespace Pricer.DAL.Ef.Configurations;

internal sealed class PrintTransactionConfiguration : IEntityTypeConfiguration<PrintTransaction>
{
	public void Configure(EntityTypeBuilder<PrintTransaction> builder)
	{
     builder.Property(x => x.CreatedAt);
		builder.Property(x => x.Status).HasConversion<int>();
		builder.Property(x => x.MaterialNameSnapshot).HasMaxLength(200).IsRequired();
		builder.Property(x => x.PrinterNameSnapshot).HasMaxLength(200).IsRequired();

		builder.Property(x => x.FilamentKg).HasColumnType("decimal(18,6)");
		builder.Property(x => x.EstimatedMetersUsed).HasColumnType("decimal(38,18)");
		builder.Property(x => x.PrintHours).HasColumnType("decimal(18,6)");

		builder.Property(x => x.MaterialCost).HasColumnType("decimal(18,6)");
		builder.Property(x => x.ElectricityKwh).HasColumnType("decimal(18,6)");
		builder.Property(x => x.ElectricityCost).HasColumnType("decimal(18,6)");
		builder.Property(x => x.PrinterWearCost).HasColumnType("decimal(18,6)");
		builder.Property(x => x.FixedCost).HasColumnType("decimal(18,6)");
		builder.Property(x => x.ExtraFixedCost).HasColumnType("decimal(18,6)");

        builder.Property(x => x.Note).HasMaxLength(4000);
	}
}
