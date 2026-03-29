using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Spooly.Models;
using Spooly.WebApp.Server.Authorization;

namespace Spooly.WebApp.Server.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public sealed class TransactionsController(
	ITransactionsService transactions,
	ISettingsService settings,
	IPrintersService printers,
	IMaterialsService materials,
	ICurrenciesService currencies) : ControllerBase
{
	public sealed record RecordPrintRequest(
		Guid MaterialId,
		decimal FilamentGrams,
		decimal PrintHours,
		decimal ExtraFixedCost,
		Guid? PrinterId = null);

	[HttpGet("prints")]
	[Authorize(Policy = Permissions.TransactionsView)]
	public async Task<IActionResult> GetPrints(CancellationToken ct)
		=> Ok(await transactions.GetPrintTransactionsAsync(ct));

	[HttpGet("stock")]
	[Authorize(Policy = Permissions.TransactionsView)]
	public async Task<IActionResult> GetStock(CancellationToken ct)
		=> Ok(await transactions.GetStockTransactionsAsync(ct));

	/// <summary>Dry-run: calculate cost without saving anything.</summary>
	[HttpPost("calculate")]
	[Authorize(Policy = Permissions.TransactionsView)]
	public async Task<IActionResult> Calculate([FromBody] RecordPrintRequest req, CancellationToken ct)
	{
		var (appData, costRequest, _, resolveError) = await ResolveAsync(req, ct);
		if (resolveError is not null) return resolveError;

		if (!PrintCostCalculator.TryCalculate(appData!, costRequest!, out var result, out var calcError))
			return BadRequest(calcError);

		return Ok(result);
	}

	[HttpPost("record-print")]
	[Authorize(Policy = Permissions.TransactionsManage)]
	public async Task<IActionResult> RecordPrint([FromBody] RecordPrintRequest req, CancellationToken ct)
	{
		var (appData, costRequest, printer, resolveError) = await ResolveAsync(req, ct);
		if (resolveError is not null) return resolveError;

		var appSettings = appData!.Settings;

		if (!PrintCostCalculator.TryCalculate(appData!, costRequest!, out var result, out var calcError))
			return BadRequest(calcError);

		await transactions.RecordPrintAsync(
			costRequest!, result,
			printer!.Id, printer.Name,
			appSettings.OperatingCurrencyId,
			appData.Currencies,
			ct);

		return Ok(result);
	}

	[HttpPost("prints/{id:guid}/revert")]
	[Authorize(Policy = Permissions.TransactionsManage)]
	public async Task<IActionResult> RevertPrint(Guid id, CancellationToken ct)
	{
		var (ok, error) = await transactions.RevertPrintAsync(id, ct);
		return ok ? Ok() : BadRequest(error);
	}

	[HttpDelete("prints/{id:guid}")]
	[Authorize(Policy = Permissions.TransactionsDelete)]
	public async Task<IActionResult> DeletePrint(Guid id, CancellationToken ct)
	{
		var (ok, error) = await transactions.DeletePrintAsync(id, ct);
		return ok ? Ok() : BadRequest(error);
	}

	// ── Shared resolution ────────────────────────────────────────────────────

	private async Task<(AppData? appData, PrintCostRequest? costRequest, Printer? printer, IActionResult? error)>
		ResolveAsync(RecordPrintRequest req, CancellationToken ct)
	{
		var allMaterials  = await materials.GetAllAsync(ct);
		var allPrinters   = await printers.GetAllAsync(ct);
		var allCurrencies = await currencies.GetAllAsync(ct);
		var appSettings   = await settings.GetAsync(ct) ?? new AppSettings();

		var material = allMaterials.FirstOrDefault(m => m.Id == req.MaterialId);
		if (material is null)
			return (null, null, null, BadRequest("Material not found."));

		if (req.PrinterId is not null)
			appSettings.SelectedPrinterId = req.PrinterId;

		var appData = new AppData
		{
			Materials  = allMaterials,
			Printers   = allPrinters,
			Currencies = allCurrencies,
			Settings   = appSettings,
		};

		var printer = appData.GetSelectedPrinter();
		if (printer is null)
			return (null, null, null, BadRequest("No printer selected. Add a printer on the Printers page or choose one here."));

		var costRequest = new PrintCostRequest(material, req.FilamentGrams, req.PrintHours, req.ExtraFixedCost);
		return (appData, costRequest, printer, null);
	}
}
