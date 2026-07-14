namespace RoadRegistry.Read.Projections;

using System.Threading.Tasks;
using BackOffice;
using JasperFx.Events;
using Marten;
using Newtonsoft.Json;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.ValueObjects;

public class StreetNameReadProjection : MartenRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<StreetNameReadItem>()
            .DatabaseSchemaName(WellKnownSchemas.MartenProjections)
            .DocumentAlias("read_streetnames")
            .Identity(x => x.Id);
    }

    public StreetNameReadProjection()
    {
        When<IEvent<StreetNameWasCreated>>((session, e, _) =>
        {
            session.Store(new StreetNameReadItem
            {
                StreetNameId = e.Data.StreetNameId,
                DutchName = e.Data.DutchName,
                Origin = e.Data.Provenance.ToEventTimestamp(),
                LastModified = e.Data.Provenance.ToEventTimestamp()
            });

            return Task.CompletedTask;
        });
        When<IEvent<StreetNameWasModified>>(async (session, e, ct) =>
        {
            var streetName = await session.LoadAsync<StreetNameReadItem>(e.Data.StreetNameId, ct)
                             ?? new StreetNameReadItem
                             {
                                 StreetNameId = e.Data.StreetNameId,
                                 DutchName = e.Data.DutchName,
                                 Origin = e.Data.Provenance.ToEventTimestamp(),
                                 LastModified = e.Data.Provenance.ToEventTimestamp()
                             };
            streetName.DutchName = e.Data.DutchName;
            streetName.NisCode = e.Data.NisCode;
            streetName.Status = e.Data.Status;
            streetName.LastModified = e.Data.Provenance.ToEventTimestamp();
            session.Store(streetName);
        });
        When<IEvent<StreetNameWasRemoved>>(async (session, e, ct) =>
        {
            var streetName = await session.LoadAsync<StreetNameReadItem>(e.Data.StreetNameId, ct);
            if (streetName is null)
            {
                return;
            }

            streetName.IsRemoved = true;
            streetName.LastModified = e.Data.Provenance.ToEventTimestamp();
            session.Store(streetName);
        });
    }
}

public sealed class StreetNameReadItem
{
    [JsonIgnore]
    public int Id { get; private set; }

    public required StreetNameLocalId StreetNameId
    {
        get => new(Id);
        set => Id = value;
    }

    public required string? DutchName { get; set; }
    public string? NisCode { get; set; }
    public string? Status { get; set; }

    public required EventTimestamp Origin { get; set; }
    public required EventTimestamp LastModified { get; set; }
    public bool IsRemoved { get; set; }
}
