namespace RoadRegistry.WmsWfsV2.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Schema;

// The street name and organization names that the flattened road segment rows denormalize into their label columns.
//
// Resolving those labels straight from the two cache tables costs a query per road segment event, which is the last
// per-event round trip left on the insert path once a created segment skips its existence lookups. Both tables are
// small and this projection is their only writer, so during a catch-up they are held in memory: loaded once, then kept
// current by the street name and organization sub-projections as they apply their own events.
//
// It is only populated while catching up. Live, the per-event cost is irrelevant and the memory is not worth holding,
// so lookups fall back to the database and the cache stays empty.
public sealed class DerivedLabelCache
{
    private Dictionary<int, string?>? _streetNames;
    private Dictionary<string, string?>? _organizations;

    public bool IsLoaded => _streetNames is not null && _organizations is not null;

    // Seeds from the database and then overlays whatever the current batch has already changed but not yet committed.
    // Reading the change tracker matters: a street name created earlier in this same batch is not in the database yet,
    // and a database-only read would resolve its label to null.
    public async Task LoadAsync(WmsWfsV2Context context, CancellationToken cancellationToken)
    {
        var streetNames = await context.StreetNameCache
            .AsNoTracking()
            .ToDictionaryAsync(x => x.StraatnaamId, x => x.Naam, cancellationToken);
        var organizations = await context.OrganizationCache
            .AsNoTracking()
            .ToDictionaryAsync(x => x.OrganisatieId!, x => x.Naam, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries<Schema.Records.StreetNameCacheRecord>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Unchanged:
                    streetNames[entry.Entity.StraatnaamId] = entry.Entity.Naam;
                    break;
                case EntityState.Deleted:
                    streetNames.Remove(entry.Entity.StraatnaamId);
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<Schema.Records.OrganizationCacheRecord>())
        {
            if (entry.Entity.OrganisatieId is null)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Unchanged:
                    organizations[entry.Entity.OrganisatieId] = entry.Entity.Naam;
                    break;
                case EntityState.Deleted:
                    organizations.Remove(entry.Entity.OrganisatieId);
                    break;
            }
        }

        _streetNames = streetNames;
        _organizations = organizations;
    }

    public void Clear()
    {
        _streetNames = null;
        _organizations = null;
    }

    // The write-through half. A no-op while the cache is not loaded, which is what keeps a half-filled cache from ever
    // existing: either it was loaded and every later mutation lands here too, or it is not in use at all.
    public void SetStreetName(int streetNameId, string? name)
    {
        if (_streetNames is not null)
        {
            _streetNames[streetNameId] = name;
        }
    }

    public void RemoveStreetName(int streetNameId)
    {
        _streetNames?.Remove(streetNameId);
    }

    public void SetOrganization(string organisatieId, string? name)
    {
        if (_organizations is not null)
        {
            _organizations[organisatieId] = name;
        }
    }

    public void RemoveOrganization(string organisatieId)
    {
        _organizations?.Remove(organisatieId);
    }

    public Dictionary<int, string?> GetStreetNames(IReadOnlyCollection<int> streetNameIds)
    {
        return streetNameIds.ToDictionary(
            id => id,
            id => _streetNames!.TryGetValue(id, out var name) ? name : null);
    }

    public Dictionary<string, string?> GetOrganizations(IReadOnlyCollection<string> organisatieIds)
    {
        return organisatieIds.ToDictionary(
            id => id,
            id => _organizations!.TryGetValue(id, out var name) ? name : null);
    }
}
