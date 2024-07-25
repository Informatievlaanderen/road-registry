namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Microsoft.Extensions.Configuration;
using Moq;
using NodaTime;
using TicketingService.Abstractions;

public class WhenCreateOutlineWithInvalidRequestFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
    }

    protected override CreateRoadSegmentOutlineRequest Request => base.Request with
    {
        Morphology = RoadSegmentMorphology.Unknown,
        Status = RoadSegmentStatus.Unknown
    };

    protected override Task<bool> VerifyTicketAsync()
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        var ticketError = new TicketError(new[]
        {
            new TicketError("Wegsegment status is foutief. 'Unknown' is geen geldige waarde.", "WegsegmentStatusNietCorrect"),
            new TicketError("Morfologische wegklasse is foutief. 'Unknown' is geen geldige waarde.", "MorfologischeWegklasseNietCorrect")
        });

        TicketingMock.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                ticketError,
                CancellationToken.None));

        return Task.FromResult(true);
    }
}
