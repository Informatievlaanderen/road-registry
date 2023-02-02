//namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.Fixtures;

//using Autofac;
//using BackOffice.Abstractions.RoadSegmentsOutline;
//using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
//using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
//using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
//using Microsoft.Extensions.Configuration;
//using SqlStreamStore;

//public class WhenCreateOutlineWithInvalidSurfaceTypeFixture : WhenCreateOutlineWithValidRequestFixture
//{
//    public WhenCreateOutlineWithInvalidSurfaceTypeFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadRegistryContext roadRegistryContext, IRoadNetworkCommandQueue roadNetworkCommandQueue)
//        : base(configuration, customRetryPolicy, streamStore, roadRegistryContext, roadNetworkCommandQueue)
//    {
//    }

//    protected override CreateRoadSegmentOutlineRequest Request => base.Request with { SurfaceType = RoadSegmentSurfaceType.Unknown };
//}
