namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Microsoft.Extensions.Configuration;
using NodaTime;

public class WhenCreateOutlineWithInvalidStatusFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidStatusFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
    }

    protected override CreateRoadSegmentOutlineRequest Request => base.Request with
    {
        Status = RoadSegmentStatus.Unknown
    };

    protected override Task<bool> VerifyTicketAsync()
    {
        return Task.FromResult(VerifyThatTicketHasError("WegsegmentStatusNietCorrect", "Wegsegment status is foutief. 'Unknown' is geen geldige waarde."));
    }
}