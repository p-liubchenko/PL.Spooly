using Microsoft.EntityFrameworkCore;

using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.DAL.Ef;

public sealed class PricerDbContext(DbContextOptions<PricerDbContext> options) : DbContext(options)
{
	public DbSet<Currency> Currencies => Set<Currency>();
	public DbSet<Printer> Printers => Set<Printer>();
	public DbSet<FilamentMaterial> Materials => Set<FilamentMaterial>();
    public DbSet<TransactionBase> Transactions => Set<TransactionBase>();
	public DbSet<PrintTransaction> PrintTransactions => Set<PrintTransaction>();
	public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
	public DbSet<AppSettings> Settings => Set<AppSettings>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricerDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
