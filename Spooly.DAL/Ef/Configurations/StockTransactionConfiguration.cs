using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Spooly.Models.Transactions;

namespace Spooly.DAL.Ef.Configurations;

internal sealed class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
	public void Configure(EntityTypeBuilder<StockTransaction> builder)
	{
     builder.Property(x => x.CreatedAt);
		builder.Property(x => x.Type).HasConversion<int>();
		builder.Property(x => x.MaterialNameSnapshot).HasMaxLength(200).IsRequired();
		builder.Property(x => x.KgDelta).HasColumnType("decimal(18,6)");
		builder.Property(x => x.MetersDelta).HasColumnType("decimal(38,18)");
		builder.Property(x => x.Note).HasMaxLength(4000);
	}
}
