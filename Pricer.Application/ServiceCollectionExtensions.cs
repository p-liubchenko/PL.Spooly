using Microsoft.Extensions.DependencyInjection;

using Pricer.Application.DataAccess;
using Pricer.DAL;
using Pricer.DAL.Ef;
using Pricer.DAL.Repositories;
using Pricer.Models;
using Pricer.Models.Transactions;

namespace Pricer.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPricerApplication(this IServiceCollection services)
	{
        services.AddScoped<ISettingsService, Services.SettingsService>();
		services.AddScoped<IPrintersService, Services.PrintersService>();
		services.AddScoped<IMaterialsService, Services.MaterialsService>();
		services.AddScoped<ICurrenciesService, Services.CurrenciesService>();
		services.AddScoped<ITransactionsService, Services.TransactionsService>();

		services.AddScoped<ICrudRepository<AppSettings, Guid>>(sp => ResolveRepo<AppSettings>(sp));
		services.AddScoped<ICrudRepository<Printer, Guid>>(sp => ResolveRepo<Printer>(sp));
		services.AddScoped<ICrudRepository<FilamentMaterial, Guid>>(sp => ResolveRepo<FilamentMaterial>(sp));
		services.AddScoped<ICrudRepository<Currency, Guid>>(sp => ResolveRepo<Currency>(sp));
		services.AddScoped<ICrudRepository<PrintTransaction, Guid>>(sp => ResolveRepo<PrintTransaction>(sp));
		services.AddScoped<ICrudRepository<StockTransaction, Guid>>(sp => ResolveRepo<StockTransaction>(sp));
		return services;
	}

	private static ICrudRepository<TEntity, Guid> ResolveRepo<TEntity>(IServiceProvider sp)
		where TEntity : class
	{
		if (sp.GetService<PricerDbContext>() is PricerDbContext ef)
		{
			return new EfCrudRepository<TEntity, Guid>(ef);
		}

		var store = sp.GetRequiredService<IAppDataStore>();
		var dataFilePathProvider = sp.GetRequiredService<DataFilePathProvider>();
		return new FileCrudRepository<TEntity, Guid>(store, dataFilePathProvider.DataFilePath);
	}

	public static IServiceCollection AddPricer(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
	{
		services.AddPricerDataAccess(configuration);
        services.AddSingleton(new DataFilePathProvider("data.json"));
		services.AddPricerApplication();
		return services;
	}
}

public sealed record DataFilePathProvider(string DataFilePath);
