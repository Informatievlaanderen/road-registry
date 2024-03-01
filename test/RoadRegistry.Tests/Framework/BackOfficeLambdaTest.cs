namespace RoadRegistry.Tests.Framework;

using Autofac;
using AutoFixture;
using BackOffice;
using BackOffice.Scenarios;
using Be.Vlaanderen.Basisregisters.CommandHandling;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Editor.Schema;
using Hosts;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using TicketingService.Abstractions;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;
using GeometryTranslator = RoadRegistry.BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public abstract class BackOfficeLambdaTest : RoadNetworkTestBase
{
    protected readonly IdempotencyContext IdempotencyContext;
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly SqsLambdaHandlerOptions Options;
    protected readonly EditorContext EditorContext;
    protected readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager;
    protected readonly FileEncoding FileEncoding;

    protected BackOfficeLambdaTest(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper)
    {
        IdempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        LoggerFactory = loggerFactory;
        Options = new FakeSqsLambdaHandlerOptions();
        RecyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        FileEncoding = FileEncoding.UTF8;

        EditorContext = new FakeEditorContextFactory().CreateDbContext(Array.Empty<string>());
    }


    protected async Task AddRoadSegment(RoadSegmentId roadSegmentId)
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeA = ObjectProvider.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = ObjectProvider.Create<RoadNodeId>();
        var segment1 = roadSegmentId;
        var line1 = new MultiLineString(
                new[]
                {
                    new LineString(
                        new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        var count = 3;

        var roadNetworkChangesAccepted = new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            TransactionId = new TransactionId(1),
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = new RoadNodeAdded
                    {
                        Id = nodeA,
                        TemporaryId = ObjectProvider.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(pointA),
                        Type = RoadNodeType.EndNode,
                        Version = 1
                    },
                    Problems = Array.Empty<Problem>()
                },
                new AcceptedChange
                {
                    RoadNodeAdded = new RoadNodeAdded
                    {
                        Id = nodeB,
                        TemporaryId = ObjectProvider.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(pointB),
                        Type = RoadNodeType.EndNode,
                        Version = 1
                    },
                    Problems = Array.Empty<Problem>()
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = new RoadSegmentAdded
                    {
                        Id = segment1,
                        TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
                        Version = ObjectProvider.Create<int>(),
                        StartNodeId = nodeA,
                        EndNodeId = nodeB,
                        AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
                        Category = ObjectProvider.Create<RoadSegmentCategory>(),
                        Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
                        Status = ObjectProvider.Create<RoadSegmentStatus>(),
                        GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
                        Geometry = GeometryTranslator.Translate(line1),
                        GeometryVersion = ObjectProvider.Create<GeometryVersion>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = ObjectProvider.Create<OrganizationId>(),
                            Name = ObjectProvider.Create<OrganizationName>()
                        },
                        LeftSide = new RoadSegmentSideAttributes
                        {
                            StreetNameId = ObjectProvider.Create<StreetNameLocalId?>()
                        },
                        RightSide = new RoadSegmentSideAttributes
                        {
                            StreetNameId = ObjectProvider.Create<StreetNameLocalId?>()
                        },
                        Lanes = ObjectProvider
                            .CreateMany<RoadSegmentLaneAttributes>(count)
                            .Select((part, index) =>
                            {
                                part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                if (index == count - 1)
                                {
                                    part.ToPosition = Convert.ToDecimal(line1.Length);
                                }
                                else
                                {
                                    part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);
                                }

                                part.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                                part.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();

                                return part;
                            })
                            .ToArray(),
                        Widths = ObjectProvider
                            .CreateMany<RoadSegmentWidthAttributes>(3)
                            .Select((part, index) =>
                            {
                                part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                if (index == count - 1)
                                {
                                    part.ToPosition = Convert.ToDecimal(line1.Length);
                                }
                                else
                                {
                                    part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);
                                }

                                part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                                return part;
                            })
                            .ToArray(),
                        Surfaces = ObjectProvider
                            .CreateMany<RoadSegmentSurfaceAttributes>(3)
                            .Select((part, index) =>
                            {
                                part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                if (index == count - 1)
                                {
                                    part.ToPosition = Convert.ToDecimal(line1.Length);
                                }
                                else
                                {
                                    part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);
                                }

                                part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                                return part;
                            })
                            .ToArray()
                    },
                    Problems = Array.Empty<Problem>()
                }
            }
        };

        await Given(RoadNetworks.Stream, roadNetworkChangesAccepted);
    }

    public async Task DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
    {
        await using var scope = Container.BeginLifetimeScope();
        var bus = scope.Resolve<ICommandHandlerResolver>();
        await bus.Dispatch(command.CreateCommandId(), command);
    }

    protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>()
        where TException : Exception, new()
    {
        var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
        idempotentCommandHandler
            .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
            .Throws<TException>();
        return idempotentCommandHandler;
    }

    protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>(Func<TException> exceptionFactory)
        where TException : Exception
    {
        var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
        idempotentCommandHandler
            .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
            .Throws(exceptionFactory());
        return idempotentCommandHandler;
    }

    protected Mock<ITicketing> MockTicketing(Action<ETagResponse> ticketingCompleteCallback)
    {
        var ticketing = new Mock<ITicketing>();
        ticketing
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
            .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
            {
                var eTagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticketResult.ResultAsJson!)!;
                ticketingCompleteCallback(eTagResponse);
            });

        return ticketing;
    }

    protected void VerifyThatTicketHasCompleted(Mock<ITicketing> ticketing, string location, string eTag)
    {
        ticketing.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(
                    new ETagResponse(location, eTag)
                ),
                CancellationToken.None
            )
        );
    }

    protected void VerifyThatTicketHasError(Mock<ITicketing> ticketing, string code, string message)
    {
        ticketing.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                new TicketError(new[] {new TicketError(
                    message,
                    code)}),
                CancellationToken.None));
    }
}
