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
        if (_permissions.ContainsKey(wellKnownTarget))
        {
            _permissions[wellKnownTarget] = permissions.Union(_permissions[wellKnownTarget]).ToArray();
        }
        else
        {
            _permissions.Add(wellKnownTarget, permissions);
        }
        return this;
    }

    public IEnumerable<(string, Permission[])> GetPermissions()
    {
        return _permissions.Select(x => (x.Key, x.Value));
    }
}
