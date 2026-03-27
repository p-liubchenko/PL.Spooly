using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Pricer.Models;

namespace Pricer.DAL.Ef.Configurations;

internal sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
	public void Configure(EntityTypeBuilder<Currency> builder)
	{
		builder.ToTable("Currencies");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Code).HasMaxLength(8).IsRequired();
		builder.Property(x => x.Value).HasColumnType("decimal(18,6)");
	}
}
