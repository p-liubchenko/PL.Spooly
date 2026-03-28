using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Spooly.DAL.Ef;

internal sealed class SpoolyDbContextFactory : IDesignTimeDbContextFactory<SpoolyDbContext>
{
	public SpoolyDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<SpoolyDbContext>();
       optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Spooly.DesignTime;Trusted_Connection=True;TrustServerCertificate=True");
		return new SpoolyDbContext(optionsBuilder.Options);
	}
}
