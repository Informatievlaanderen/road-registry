namespace RoadRegistry.Hosts.Infrastructure.Options;

using System.Collections.Generic;
using System.Linq;
using BackOffice;

public abstract class HealthCheckPermissionOptionsBuilder<TOptions> : HealthCheckOptionsBuilder<TOptions>
{
    private readonly Dictionary<string, Permission[]> _permissions = new();
    public override bool IsValid => _permissions.Any();

    public HealthCheckPermissionOptionsBuilder<TOptions> CheckPermission(string wellKnownTarget, params Permission[] permissions)
    {
        _permissions.Add(wellKnownTarget, permissions);
        return this;
    }

    public IEnumerable<Permission> GetPermissions(string wellKnownTarget)
    {
        return _permissions.TryGetValue(wellKnownTarget, out var permissions)
            ? permissions.AsEnumerable()
            : Enumerable.Empty<Permission>();
    }
}
