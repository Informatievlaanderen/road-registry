namespace RoadRegistry.Read.Projections;

using System;
using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.ValueObjects;

public class OrganizationReadProjection : MartenRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<OrganizationReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_organizations")
            .Identity(x => x.Id);
    }

    public OrganizationReadProjection()
    {
        When<IEvent<OrganizationWasImported>>((session, e, _) =>
        {
            var organization = new OrganizationReadItem(e.Data.OrganizationId)
            {
                Name = e.Data.Name,
                OvoCode = null,
                KboNumber = null,
                IsMaintainer = false,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            };
            session.Store(organization);

            return Task.CompletedTask;
        });
        When<IEvent<OrganizationWasCreated>>((session, e, _) =>
        {
            var organization = new OrganizationReadItem(e.Data.OrganizationId)
            {
                Name = e.Data.Name,
                OvoCode = e.Data.OvoCode,
                KboNumber = e.Data.KboNumber,
                IsMaintainer = false,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            };
            session.Store(organization);

            return Task.CompletedTask;
        });
        When<IEvent<OrganizationWasModified>>(async (session, e, ct) =>
        {
            var organization = await session.LoadAsync<OrganizationReadItem>(e.Data.OrganizationId.ToString(), ct);
            if (organization is null)
            {
                throw new InvalidOperationException($"No organization found for Id {e.Data.OrganizationId}");
            }

            // A null value means the property was not changed by this event.
            organization.Name = e.Data.Name ?? organization.Name;
            organization.OvoCode = e.Data.OvoCode ?? organization.OvoCode;
            organization.KboNumber = e.Data.KboNumber ?? organization.KboNumber;
            organization.IsMaintainer = e.Data.IsMaintainer ?? organization.IsMaintainer;
            organization.LastModified = e.Data.Provenance.ToEventTimestamp();

            session.Store(organization);
        });
        When<IEvent<OrganizationWasRemoved>>(async (session, e, ct) =>
        {
            var organization = await session.LoadAsync<OrganizationReadItem>(e.Data.OrganizationId.ToString(), ct);
            if (organization is null)
            {
                throw new InvalidOperationException($"No organization found for Id {e.Data.OrganizationId}");
            }

            organization.IsRemoved = true;
            organization.LastModified = e.Data.Provenance.ToEventTimestamp();

            session.Store(organization);
        });
    }
}

public sealed class OrganizationReadItem
{
    [JsonConstructor]
    public OrganizationReadItem(
        OrganizationId organizationId,
        string name,
        string? ovoCode,
        string? kboNumber,
        bool isMaintainer,
        EventTimestamp origin,
        EventTimestamp lastModified,
        bool isRemoved
    )
    {
        Id = organizationId.ToString();
        OrganizationId = organizationId;
        Name = name;
        OvoCode = ovoCode;
        KboNumber = kboNumber;
        IsMaintainer = isMaintainer;
        Origin = origin;
        LastModified = lastModified;
        IsRemoved = isRemoved;
    }

    public OrganizationReadItem(OrganizationId organizationId)
    {
        Id = organizationId.ToString();
        OrganizationId = organizationId;
    }

    [JsonIgnore]
    public string Id { get; }

    public OrganizationId OrganizationId { get; }

    public required string Name { get; set; }
    public string? OvoCode { get; set; }
    public string? KboNumber { get; set; }
    public bool IsMaintainer { get; set; }

    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }
    public bool IsRemoved { get; set; }
}
