//namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.Fixtures;

//using Abstractions.Fixtures;
//using BackOffice;
//using BackOffice.Abstractions.RoadNetworks;
//using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
//using Microsoft.Extensions.Configuration;
//using NodaTime;
//using SqlStreamStore;

//public class WhenCreateRoadNetworkSnapshotWithValidRequestFixture : WhenCreateRoadNetworkSnapshotFixture
//{
//    public WhenCreateRoadNetworkSnapshotWithValidRequestFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadNetworkCommandQueue roadNetworkCommandQueue, IClock clock)
//        : base(configuration, customRetryPolicy, streamStore, roadNetworkCommandQueue, clock)
//    {
//    }

//    protected override CreateRoadNetworkSnapshotRequest Request => new();

//    protected override Task SetupAsync()
//    {
//        throw new NotImplementedException();
//    }

//    protected override Task<bool> VerifyTicketAsync()
//    {
//        throw new NotImplementedException();
//    }
//}
