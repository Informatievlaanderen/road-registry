//namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.Abstractions.Fixtures;

//using AutoFixture;
//using BackOffice;
//using BackOffice.Abstractions.RoadNetworks;
//using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
//using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
//using Framework;
//using Handlers;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using NodaTime;
//using Requests;
//using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
//using RoadRegistry.BackOffice.Core;
//using RoadRegistry.BackOffice.Framework;
//using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
//using RoadRegistry.Dbase;
//using RoadRegistry.Hosts;
//using SqlStreamStore;
//using Sqs.RoadNetworks;
//using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;

//public abstract class WhenCreateRoadNetworkSnapshotFixture : SqsLambdaHandlerFixture<CreateRoadNetworkSnapshotSqsLambdaRequestHandler, CreateRoadNetworkSnapshotSqsLambdaRequest, CreateRoadNetworkSnapshotSqsRequest>
//{
//    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

//    protected WhenCreateRoadNetworkSnapshotFixture(SqsLambdaHandlerOptions options, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadNetworkCommandQueue roadNetworkCommandQueue, IClock clock)
//        : base(options, customRetryPolicy, streamStore, roadNetworkCommandQueue, clock)
//    {
//        Organisation = Organisation.DigitaalVlaanderen;

//        ObjectProvider.Customize<ProvenanceData>(customization =>
//            customization.FromSeed(generator => new ProvenanceData(new Provenance(Clock.GetCurrentInstant(),
//                Application.RoadRegistry,
//                new Reason("TEST"),
//                new Operator("TEST"),
//                Modification.Unknown,
//                Organisation)))
//        );
//    }

//    protected Organisation Organisation { get; }
//    protected abstract CreateRoadNetworkSnapshotRequest Request { get; }

//    protected override CreateRoadNetworkSnapshotSqsRequest SqsRequest => new()
//    {
//        Request = Request,
//        TicketId = Guid.NewGuid(),
//        Metadata = new Dictionary<string, object?>(),
//        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
//    };

//    protected override CreateRoadNetworkSnapshotSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

//    protected override CreateRoadNetworkSnapshotSqsLambdaRequestHandler SqsLambdaRequestHandler => throw new NotImplementedException();

//    protected override CommandHandlerDispatcher BuildCommandHandlerDispatcher()
//    {
//        throw new NotImplementedException();
//    }
//}
