using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Spooly.Models;
using Spooly.WebApp.Server.Authorization;

namespace Spooly.WebApp.Server.Controllers;

[ApiController]
[Route("api/materials")]
[Authorize]
public sealed class MaterialsController(
	IMaterialsService materials,
	ICurrenciesService currencies,
	ITransactionsService transactions) : ControllerBase
{
	public sealed record AddSpoolRequest(FilamentMaterial Material, decimal TotalPrice, Guid? OperatingCurrencyId);
	public sealed record RestockRequest(decimal AddKg, decimal AddMeters, decimal AddTotalPrice, Guid? OperatingCurrencyId);
	public sealed record ConsumeRequest(decimal Kg, decimal Meters);

	[HttpGet]
	[Authorize(Policy = Permissions.MaterialsView)]
	public async Task<IActionResult> GetAll(CancellationToken ct)
		=> Ok(await materials.GetAllAsync(ct));

	[HttpPost("add-spool")]
	[Authorize(Policy = Permissions.MaterialsManage)]
	public async Task<IActionResult> AddSpool([FromBody] AddSpoolRequest req, CancellationToken ct)
	{
		var allCurrencies = await currencies.GetAllAsync(ct);
		req.Material.Id = Guid.NewGuid();
		await materials.AddSpoolAsync(req.Material, req.TotalPrice, req.OperatingCurrencyId, allCurrencies, ct);
		var spoolCost = new Money(req.TotalPrice, req.OperatingCurrencyId);
		await transactions.RecordSpoolPurchaseAsync(req.Material, req.Material.AmountKg, req.Material.EstimatedLengthMeters, spoolCost, ct);
		return Ok(req.Material);
	}

	[HttpPut("{id:guid}")]
	[Authorize(Policy = Permissions.MaterialsManage)]
	public async Task<IActionResult> Upsert(Guid id, [FromBody] FilamentMaterial material, CancellationToken ct)
	{
		material.Id = id;
		await materials.UpsertAsync(material, ct);
		return Ok();
	}

	[HttpPost("{id:guid}/restock")]
	[Authorize(Policy = Permissions.MaterialsManage)]
	public async Task<IActionResult> Restock(Guid id, [FromBody] RestockRequest req, CancellationToken ct)
	{
		var allCurrencies = await currencies.GetAllAsync(ct);
		var allMaterials = await materials.GetAllAsync(ct);
		var material = allMaterials.FirstOrDefault(m => m.Id == id);
		if (material is null)
			return BadRequest("Material not found.");

		var (ok, error) = await materials.RestockAsync(id, req.AddKg, req.AddMeters, req.AddTotalPrice, req.OperatingCurrencyId, allCurrencies, ct);
		if (!ok)
			return BadRequest(error);

		var restockCost = new Money(req.AddTotalPrice, req.OperatingCurrencyId);
		await transactions.RecordRestockAsync(material, req.AddKg, req.AddMeters, restockCost, ct);
		return Ok();
	}

	[HttpPost("{id:guid}/consume")]
	[Authorize(Policy = Permissions.MaterialsManage)]
	public async Task<IActionResult> Consume(Guid id, [FromBody] ConsumeRequest req, CancellationToken ct)
	{
		var (ok, error) = await materials.ConsumeAsync(id, req.Kg, req.Meters, ct);
		return ok ? Ok() : BadRequest(error);
	}

	[HttpDelete("{id:guid}")]
	[Authorize(Policy = Permissions.MaterialsDelete)]
	public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
	{
		var (ok, error) = await materials.RemoveAsync(id, ct);
		return ok ? Ok() : BadRequest(error);
	}
}
