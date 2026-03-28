using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Spooly.Models;

namespace Spooly.DAL.Ef.Configurations;

internal sealed class FilamentMaterialConfiguration : IEntityTypeConfiguration<FilamentMaterial>
{
	public void Configure(EntityTypeBuilder<FilamentMaterial> builder)
	{
		builder.ToTable("Materials");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
		builder.Property(x => x.Color).HasMaxLength(100).IsRequired();
		builder.Property(x => x.Grade).HasMaxLength(50);
		builder.Property(x => x.Type).HasConversion<int>();
		builder.Property(x => x.AmountKg).HasColumnType("decimal(18,6)");
		builder.Property(x => x.EstimatedLengthMeters).HasColumnType("decimal(38,18)");

		builder.Property(x => x.AveragePricePerKgMoney)
			.HasConversion(new Converters.MoneyJsonConverter())
			.HasColumnType("nvarchar(max)");
	}
}
