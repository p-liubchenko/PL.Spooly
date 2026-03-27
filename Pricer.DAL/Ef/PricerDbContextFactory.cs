using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Pricer.DAL.Ef;

internal sealed class PricerDbContextFactory : IDesignTimeDbContextFactory<PricerDbContext>
{
	public PricerDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<PricerDbContext>();
       optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Pricer.DesignTime;Trusted_Connection=True;TrustServerCertificate=True");
		return new PricerDbContext(optionsBuilder.Options);
	}
}
