#nullable enable

namespace RoadRegistry.StreetNameConsumer.ProjectionHost.Tests.Projections.Fixtures;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using NodaTime;
using RoadRegistry.StreetNameConsumer.Projections;
using Schema;
using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;
using Reason = BackOffice.Reason;

public class StreetNameConsumerProjectionFixture : ConnectedProjectionFixture<StreetNameConsumerProjection, StreetNameConsumerContext>, IAsyncLifetime
{
    private readonly StreetNameConsumerContext _dbContext;
    private Provenance _provenance;
    public string LanguageDefault = nameof(StreetNameLanguage.Dutch);
    public string MunicipalityIdDefault = "579D6F7655C244B3A91AF19D3696ADA6";
    public string StreetNameIdDefault = "CD4B7AE60F2742EFB595BA1D85C88593";

    public StreetNameConsumerProjectionFixture(StreetNameConsumerContext context)
        : base(context, Resolve.WhenEqualToHandlerMessageType(new StreetNameConsumerProjection().Handlers))
    {
        _dbContext = context;
    }

    public Provenance Provenance => _provenance ??= new Provenance(
        SystemClock.Instance.GetCurrentInstant().ToString(),
        "RoadRegistry",
        nameof(Modification.Unknown),
        nameof(Organisation.DigitaalVlaanderen),
        new Reason("TEST SCENARIO")
    );

    public async Task InitializeAsync()
    {
        await ProjectAsync(new StreetNameWasRegistered(
            StreetNameIdDefault,
            MunicipalityIdDefault,
            "12345",
            Provenance
        ));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public StreetNameConsumerItem? GetConsumerItem(string streetNameId)
    {
        return _dbContext.StreetNames.Find(streetNameId);
    }
}
