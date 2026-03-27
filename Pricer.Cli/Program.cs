using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Pricer.DAL;

using System.Runtime.CompilerServices;

namespace Pricer.Cli;

internal static class Program
{
	private const string DataFileName = "data.json";

	static void Main(string[] args)
	{
		var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
						?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
						?? "Production";
		var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile("appsettings.Development.json", optional: true)
			.AddEnvironmentVariables(prefix: "PRICER_")
			.AddUserSecrets(typeof(Program).Assembly, optional: true)
			.AddCommandLine(args)
			.Build();

		var services = new ServiceCollection();
		services.AddSingleton<IConfiguration>(configuration);
		services.AddPricerDataAccess(configuration);
		services.AddSingleton(sp => new AppStartup(sp.GetRequiredService<IAppDataStore>()));
		services.AddSingleton(sp => new StockTransactionsManager(sp.GetRequiredService<IAppDataStore>(), DataFileName));
		services.AddSingleton(sp => new FilamentWarehouse(sp.GetRequiredService<IAppDataStore>(), DataFileName, sp.GetRequiredService<StockTransactionsManager>()));
		services.AddSingleton(sp => new PrinterManager(sp.GetRequiredService<IAppDataStore>(), DataFileName));
		services.AddSingleton(sp => new CurrencyManager(sp.GetRequiredService<IAppDataStore>(), DataFileName));
		services.AddSingleton(sp => new PrintTransactionsManager(sp.GetRequiredService<IAppDataStore>(), DataFileName));

		services.AddSingleton<FilamentWarehouseCliDrawer>();
		services.AddSingleton<PrinterManagerCliDrawer>();
		services.AddSingleton<CurrencyManagerCliDrawer>();
		services.AddSingleton<PrintTransactionsCliDrawer>();
		services.AddSingleton<PrintCostCalculatorCliDrawer>();
		services.AddSingleton<AppCli>();

		var provider = services.BuildServiceProvider();
		
		provider.ApplyPendingMigrations();
		
		var startup = provider.GetRequiredService<AppStartup>();
		var appData = startup.LoadAndTreat(DataFileName);
		provider.GetRequiredService<AppCli>().Run(appData, DataFileName);
	}

}
