namespace RoadRegistry.SyncHost.Tests.StreetName.Projections.Fixtures;

using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using RoadRegistry.StreetName;
using Sync.StreetNameRegistry;

public class StreetNameProjectionFixture : ConnectedProjectionFixture<StreetNameProjection, StreetNameProjectionContext>
{
    private readonly StreetNameProjectionContext _dbContext;

    public StreetNameProjectionFixture(StreetNameProjectionContext context)
        : base(context, Resolve.WhenEqualToHandlerMessageType(new StreetNameProjection().Handlers))
    {
        _dbContext = context;

        StreetName1 = new StreetNameSnapshotOsloRecord
        {
            Identificator = new DeseriazableIdentificator("https://data.vlaanderen.be/id/straatnaam", "1", "a"),
            Gemeente = new StraatnaamDetailGemeente
            {
                ObjectId = "44021",
                Detail = "http://gemeente/44021",
                Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gent", Taal.NL))
            },
            Straatnamen = new List<DeseriazableGeografischeNaam>
            {
                new("Straat", Taal.NL),
                new("Rue", Taal.FR),
                new("Street", Taal.EN),
                new("Strasse", Taal.DE)
            },
            HomoniemToevoegingen = new List<DeseriazableGeografischeNaam>
            {
                new("NL", Taal.NL),
                new("FR", Taal.FR),
                new("EN", Taal.EN),
                new("DE", Taal.DE)
            },
            StraatnaamStatus = StreetNameStatus.Current
        };
    }

    public StreetNameSnapshotOsloRecord StreetName1 { get; }

    public StreetNameRecord? GetStreetNameRecord(string streetNameId)
    {
        return _dbContext.StreetNames.Find(streetNameId);
    }
}
