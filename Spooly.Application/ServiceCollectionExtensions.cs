using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Spooly.DAL;

namespace Spooly.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSpoolyApplication(this IServiceCollection services)
	{
		services.AddScoped<ISettingsService, Services.SettingsService>();
		services.AddScoped<IPrintersService, Services.PrintersService>();
		services.AddScoped<IMaterialsService, Services.MaterialsService>();
		services.AddScoped<ICurrenciesService, Services.CurrenciesService>();
		services.AddScoped<ITransactionsService, Services.TransactionsService>();
		return services;
	}

	public static IServiceCollection AddSpooly(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSpoolyDataAccess(configuration);
		services.AddSpoolyApplication();
		return services;
	}
}
