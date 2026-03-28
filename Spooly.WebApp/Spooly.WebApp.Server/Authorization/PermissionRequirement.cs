using Microsoft.AspNetCore.Authorization;

namespace Spooly.WebApp.Server.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
	public string Permission => permission;
}
