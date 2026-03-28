using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Spooly.DAL.Ef.Converters;
using Spooly.Models.Transactions;

namespace Spooly.DAL.Ef.Configurations;

internal sealed class TransactionBaseConfiguration : IEntityTypeConfiguration<TransactionBase>
{
	public void Configure(EntityTypeBuilder<TransactionBase> builder)
	{
		builder.ToTable("Transactions");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.CreatedAt).IsRequired();

		builder.Property(x => x.OriginalCost)
			.HasConversion(new MoneyJsonConverter())
			.HasColumnType("nvarchar(max)");

		builder.Property(x => x.TotalCost)
			.HasConversion(new MoneyJsonConverter())
			.HasColumnType("nvarchar(max)");

		builder
			.HasDiscriminator<string>("TransactionType")
			.HasValue<PrintTransaction>("Print")
			.HasValue<StockTransaction>("Stock");

		builder.Property("TransactionType")
			.HasMaxLength(32)
			.IsRequired();
	}
}
