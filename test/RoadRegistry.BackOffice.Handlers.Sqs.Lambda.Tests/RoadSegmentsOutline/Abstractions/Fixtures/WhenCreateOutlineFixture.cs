namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Dbase;
using Framework;
using Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using Requests;
using SqlStreamStore;
using Sqs.RoadSegments;

public abstract class WhenCreateOutlineFixture : SqsLambdaHandlerFixture<CreateRoadSegmentOutlineSqsLambdaRequestHandler, CreateRoadSegmentOutlineSqsLambdaRequest, CreateRoadSegmentOutlineSqsRequest>
{
    protected WhenCreateOutlineFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadRegistryContext roadRegistryContext, IRoadNetworkCommandQueue roadNetworkCommandQueue, IIdempotentCommandHandler idempotentCommandHandler, IClock clock)
        : base(configuration, customRetryPolicy, streamStore, roadRegistryContext, roadNetworkCommandQueue, idempotentCommandHandler, clock)
    {
        Organisation = Organisation.DigitaalVlaanderen;

        ObjectProvider.Customize<ProvenanceData>(customization =>
            customization.FromSeed(generator => new ProvenanceData(new Provenance(Clock.GetCurrentInstant(),
                Application.RoadRegistry,
                new Reason("TEST"),
                new Operator("TEST"),
                Modification.Unknown,
                Organisation)))
        );
    }

    protected Organisation Organisation { get; }

    protected abstract CreateRoadSegmentOutlineRequest Request { get; }

    protected override CreateRoadSegmentOutlineSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override CreateRoadSegmentOutlineSqsLambdaRequest SqsLambdaRequest => new(RoadNetworkInfo.Identifier.ToString(), SqsRequest);

    protected override CreateRoadSegmentOutlineSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Configuration,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        RoadNetworkCommandQueue,
        new NullLogger<CreateRoadSegmentOutlineSqsLambdaRequestHandler>()
    );
}
