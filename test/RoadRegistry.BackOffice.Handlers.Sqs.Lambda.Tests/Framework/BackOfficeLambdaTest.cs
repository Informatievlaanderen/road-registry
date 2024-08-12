namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions;
using Autofac;
using Autofac.Core.Lifetime;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.Organizations.V1;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Editor.Schema;
using Editor.Schema.Extensions;
using Hosts;
using MediatR;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Newtonsoft.Json;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;
using GeometryTranslator = BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using MessageMetadata = Be.Vlaanderen.Basisregisters.Aws.Lambda.MessageMetadata;
using Point = NetTopologySuite.Geometries.Point;
using Problem = Messages.Problem;
using RoadSegmentLaneAttributes = Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = Messages.RoadSegmentWidthAttributes;

public abstract class BackOfficeLambdaTest: RoadNetworkTestBase
{
    private const string ConfigurationDetailUrl = "http://base/{0}";

    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;

    protected EditorContext EditorContext { get; }
    protected RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; }
    protected FileEncoding FileEncoding { get; }
    protected OrganizationDbaseRecord OrganizationDbaseRecord { get; }
    protected ApplicationMetadata ApplicationMetadata { get; }
    protected IOrganizationCache OrganizationCache { get; }

    protected SqsLambdaHandlerOptions SqsLambdaHandlerOptions { get; }

    protected BackOfficeLambdaTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        SqsLambdaHandlerOptions = new FakeSqsLambdaHandlerOptions();
        RecyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        FileEncoding = FileEncoding.UTF8;
        EditorContext = new FakeEditorContextFactory().CreateDbContext([]);
        OrganizationDbaseRecord = new OrganizationDbaseRecord
        {
            ORG = { Value = "AGIV" },
            LBLORG = { Value = "Agentschap voor Geografische Informatie Vlaanderen" }
        };

        ApplicationMetadata = new(RoadRegistryApplication.Lambda);
        OrganizationCache = new FakeOrganizationCache();
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder
            .Register(_ => new EventSourcedEntityMap())
            .AsSelf()
            .SingleInstance();

        containerBuilder
            .Register(c => new RoadRegistryIdempotentCommandHandler(c.Resolve<CommandHandlerDispatcher>()))
            .As<IIdempotentCommandHandler>();

        containerBuilder
            .Register(c => new ChangeRoadNetworkDispatcher(
                new RoadNetworkCommandQueue(Store, ApplicationMetadata),
                c.Resolve<IIdempotentCommandHandler>(),
                ScopedContainer.Resolve<EventSourcedEntityMap>(),
                OrganizationCache,
                LoggerFactory.CreateLogger<ChangeRoadNetworkDispatcher>()
            ))
            .As<IChangeRoadNetworkDispatcher>();
    }

    protected async Task GivenOrganization()
    {
        await GivenEvents(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)),
            new ImportedOrganization
            {
                Code = OrganizationDbaseRecord.ORG.Value,
                Name = OrganizationDbaseRecord.LBLORG.Value,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            });

        await EditorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            SortableCode = OrganizationDbaseRecord.ORG.Value,
            DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion,
            DbaseRecord = OrganizationDbaseRecord.ToBytes(RecyclableMemoryStreamManager, FileEncoding)
        });
        await EditorContext.SaveChangesAsync();
    }

    protected Mock<ITicketing> MockTicketing()
    {
        return new Mock<ITicketing>();
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

    protected Task GivenEvents(StreamName streamName, params object[] events)
    {
        return Store.Given(Mapping, Settings, StreamNameConverter, streamName, events);
    }

    protected async Task AddMeasuredRoadSegment(RoadSegmentId roadSegmentId)
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeA = ObjectProvider.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = ObjectProvider.Create<RoadNodeId>();
        var segment1 = roadSegmentId;
        var line1 = new MultiLineString(
            [
                new LineString(
                        new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
            ])
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
            Changes =
            [
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
                    Problems = []
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
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
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
                    Problems = []
                }
            ]
        };

        await GivenEvents(RoadNetworks.Stream, roadNetworkChangesAccepted);
    }

    protected async Task AddOutlinedRoadSegment(RoadSegmentId roadSegmentId)
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var segment1 = roadSegmentId;
        var line1 = new MultiLineString(
            [
                new LineString(
                        new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
            ])
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        var roadNetworkChangesAccepted = new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            TransactionId = new TransactionId(1),
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = new RoadSegmentAdded
                    {
                        Id = segment1,
                        TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
                        Version = ObjectProvider.Create<int>(),
                        StartNodeId = 0,
                        EndNodeId = 0,
                        AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
                        Category = ObjectProvider.Create<RoadSegmentCategory>(),
                        Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
                        Status = ObjectProvider.Create<RoadSegmentStatus>(),
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
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
                            .CreateMany<RoadSegmentLaneAttributes>(1)
                            .Select(part =>
                            {
                                part.FromPosition = 0;
                                part.ToPosition = Convert.ToDecimal(line1.Length);

                                part.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                                part.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();

                                return part;
                            })
                            .ToArray(),
                        Widths = ObjectProvider
                            .CreateMany<RoadSegmentWidthAttributes>(1)
                            .Select(part =>
                            {
                                part.FromPosition = 0;
                                part.ToPosition = Convert.ToDecimal(line1.Length);

                                part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                                return part;
                            })
                            .ToArray(),
                        Surfaces = ObjectProvider
                            .CreateMany<RoadSegmentSurfaceAttributes>(1)
                            .Select(part =>
                            {
                                part.FromPosition = 0;
                                part.ToPosition = Convert.ToDecimal(line1.Length);

                                part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                                return part;
                            })
                            .ToArray()
                    },
                    Problems = []
                }
            ]
        };

        await GivenEvents(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(roadSegmentId), roadNetworkChangesAccepted);
    }

    protected async Task ThrowIfLastCommandIsRoadNetworkChangesRejected()
    {
        var rejectCommand = await Store.GetLastCommandIfTypeIs<RoadNetworkChangesRejected>();
        if (rejectCommand != null)
        {
            var problems = rejectCommand.Changes.SelectMany(change => change.Problems).ToArray();
            if (problems.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, problems.Select(x => x.ToString())));
            }
        }
    }

    protected async Task VerifyThatTicketHasCompleted(RoadSegmentId roadSegmentId)
    {
        var roadNetwork = await RoadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(roadSegmentId, CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);

        VerifyThatTicketHasCompleted(string.Format(ConfigurationDetailUrl, roadSegmentId), roadSegment?.LastEventHash ?? string.Empty);
    }

    protected void VerifyThatTicketHasCompleted(string location, string eTag)
    {
        VerifyThatTicketHasCompleted(TicketingMock, location, eTag);
    }
    protected void VerifyThatTicketHasCompleted(object response)
    {
        VerifyThatTicketHasCompleted(TicketingMock, response);
    }

    protected void VerifyThatTicketHasCompleted(Mock<ITicketing> ticketing, string location, string eTag)
    {
        VerifyThatTicketHasCompleted(ticketing, new ETagResponse(location, eTag));
    }
    protected void VerifyThatTicketHasCompleted(Mock<ITicketing> ticketing, object response)
    {
        ticketing.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(response),
                CancellationToken.None
            )
        );
    }

    protected void VerifyThatTicketHasError(string code, string message)
    {
        VerifyThatTicketHasError(TicketingMock, code, message);
    }

    protected void VerifyThatTicketHasError(Mock<ITicketing> ticketing, string code, string message)
    {
        ticketing.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                new TicketError(message, code),
                CancellationToken.None));
    }
    protected void VerifyThatTicketHasErrorList(string code, string message)
    {
        VerifyThatTicketHasErrorList(TicketingMock, code, message);
    }
    protected void VerifyThatTicketHasErrorList(Mock<ITicketing> ticketing, string code, string message)
    {
        ticketing.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                new TicketError(new[] {
                    new TicketError(message, code)
                }),
                CancellationToken.None));
    }

    protected async Task WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<TSqsRequest, TSqsLambdaRequest, TBackOfficeRequest>()
        where TSqsRequest : SqsRequest, IHasBackOfficeRequest<TBackOfficeRequest>
        where TSqsLambdaRequest : SqsLambdaRequest, IHasBackOfficeRequest<TBackOfficeRequest>
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new SqsOptions()));

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        containerBuilder.RegisterInstance(blobClient);
        var container = containerBuilder.Build();

        var messageData = ObjectProvider.Create<TSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container, container.Resolve<SqsMessagesBlobClient>());

        // Act
        await sut.HandleMessage(
            messageData,
            messageMetadata,
            CancellationToken.None);

        // Assert
        mediator
            .Verify(x => x.Send<SqsLambdaRequest>(It.Is<TSqsLambdaRequest>(request =>
                request.TicketId == messageData.TicketId &&
                request.MessageGroupId == messageMetadata.MessageGroupId &&
                Equals(request.Request, messageData.Request) &&
                request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                request.Metadata == messageData.Metadata
            ), CancellationToken.None), Times.Once);
    }
}
