namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.Fixtures;

using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using NodaTime;
using SqlStreamStore;

public class WhenCreateOutlineWithInvalidStatusFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidStatusFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadRegistryContext roadRegistryContext, IRoadNetworkCommandQueue roadNetworkCommandQueue, IIdempotentCommandHandler idempotentCommandHandler, IClock clock)
        : base(configuration, customRetryPolicy, streamStore, roadRegistryContext, roadNetworkCommandQueue, idempotentCommandHandler, clock)
    {
    }

    protected override CreateRoadSegmentOutlineRequest Request => base.Request with
    {
        Status = RoadSegmentStatus.Unknown
    };

    protected override Task<bool> VerifyTicketAsync()
    {
        return Task.FromResult(VerifyThatTicketHasError("InvalidStatus", "The 'Status' is not a valid RoadSegmentStatus."));
    }
}
