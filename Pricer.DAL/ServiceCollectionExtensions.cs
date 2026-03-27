using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Pricer.DAL.Ef;
using Pricer.DAL.Options;
using Pricer.DAL.UnitOfWork;

namespace Pricer.DAL;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricerDataAccess(this IServiceCollection services, IConfiguration configuration)
	{
		var options = configuration.GetSection(DataAccessOptions.SectionName).Get<DataAccessOptions>()
				  ?? new DataAccessOptions();

		switch (options.Mode)
		{
			case DataAccessMode.File:
				services.AddSingleton<IAppDataStore, FileBackedStore>();
				return services;

			case DataAccessMode.Mssql:
				string connectionString = configuration.GetConnectionString("DefaultConnection");
				if (string.IsNullOrWhiteSpace(connectionString))
				{
					throw new InvalidOperationException($"'{DataAccessOptions.SectionName}:ConnectionString' is required when Mode is Mssql.");
				}

				services.AddDbContext<PricerDbContext>(db =>
					db.UseSqlServer(connectionString, sql =>
						sql.MigrationsAssembly(typeof(PricerDbContext).Assembly.FullName)));

                services.AddScoped<IAppDataStore, UnitOfWorkAppDataStore>();

				return services;

			default:
				throw new NotSupportedException($"Unsupported data access mode: '{options.Mode}'.");
		}
	}

	public static void ApplyPendingMigrations(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<PricerDbContext>();
		if (dbContext.Database.GetPendingMigrations().Any())
		{
			dbContext.Database.Migrate();
		}
	}
}
