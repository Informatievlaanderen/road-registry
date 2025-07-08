namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Framework.Testing;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using TicketingService.Abstractions;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;
using AddGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.AddGradeSeparatedJunction;
using AddRoadSegmentToEuropeanRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToEuropeanRoad;
using AddRoadSegmentToNationalRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad;
using AddRoadSegmentToNumberedRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNumberedRoad;
using GeometryTranslator = RoadRegistry.BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadSegmentGeometry = RoadRegistry.BackOffice.Messages.ModifyRoadSegmentGeometry;
using Point = NetTopologySuite.Geometries.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using ProblemParameter = RoadRegistry.BackOffice.Messages.ProblemParameter;
using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RemoveOutlinedRoadSegment = RoadRegistry.BackOffice.Messages.RemoveOutlinedRoadSegment;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class RoadNetworkScenarios : RoadNetworkTestBase
{
    public RoadNetworkScenarios(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task Ticket_is_completed_when_change_accepted()
    {
        var ticketId = ObjectProvider.Create<TicketId>();


        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithTicketId(ticketId)
                .WithAddRoadNode(TestData.AddStartNode1)
                .WithAddRoadNode(TestData.AddEndNode1)
                .WithAddRoadSegment(TestData.AddSegment1)
                .Build()
            )
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                ],
                TicketId = ticketId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        TicketingMock
            .Verify(x => x.Complete(ticketId, It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenChangesAccepted_ThenExtractIsClosed()
    {
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ObjectProvider.Create<ExtractRequestId>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var currentInstant = Clock.GetCurrentInstant();

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                }
            )
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId),
                new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalRequestId,
                    DownloadId = downloadId,
                    IsInformative = false,
                    Description = ObjectProvider.Create<ExtractDescription>(),
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                }
            )
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithExtractRequestId(extractRequestId)
                .WithAddRoadNode(TestData.AddStartNode1)
                .WithAddRoadNode(TestData.AddEndNode1)
                .WithAddRoadSegment(TestData.AddSegment1)
                .Build()
            )
            .Then([
                new RecordedEvent(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = TestData.StartNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.EndNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = TestData.Segment1Added,
                            Problems = []
                        }
                    ],
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                }),
                new RecordedEvent(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractClosed
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalRequestId,
                    DownloadIds = [downloadId],
                    Reason = RoadNetworkExtractCloseReason.UploadAccepted,
                    DateRequested = currentInstant.ToDateTimeUtc(),
                    When = InstantPattern.ExtendedIso.Format(currentInstant)
                })
            ]));
    }

    [Fact]
    public async Task Ticket_is_completed_when_no_changes()
    {
        var ticketId = ObjectProvider.Create<TicketId>();

        TestData.ModifyStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.EndNode)
            .ToString();

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkChangesAccepted
            {
                RequestId = "test",
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = TestData.StartNode1Modified
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithTicketId(ticketId)
                .Build()
            )
            .Then(RoadNetworks.Stream, new NoRoadNetworkChanges
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                TicketId = ticketId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));

        TicketingMock
            .Verify(x => x.Complete(ticketId, It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Ticket_is_error_when_change_rejected()
    {
        var ticketId = ObjectProvider.Create<TicketId>();

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithTicketId(ticketId)
                .WithAddRoadNode(TestData.AddStartNode1)
                .Build()
            )
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeNotConnectedToAnySegment",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                TicketId = ticketId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );

        TicketingMock.Verify(x => x.Error(ticketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task when_adding_a_disconnected_node()
    {
        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeNotConnectedToAnySegment",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_grade_separated_junction_with_a_missing_lower_segment()
    {
        var addGradeSeparatedJunction = new AddGradeSeparatedJunction
        {
            TemporaryId = ObjectProvider.Create<GradeSeparatedJunctionId>(),
            Type = ObjectProvider.Create<GradeSeparatedJunctionType>(),
            UpperSegmentId = TestData.Segment1Added.Id,
            LowerSegmentId = ObjectProvider.Create<RoadSegmentId>()
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddGradeSeparatedJunction = addGradeSeparatedJunction
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "LowerRoadSegmentMissing",
                                Parameters = Array.Empty<ProblemParameter>()
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_grade_separated_junction_with_a_missing_upper_segment()
    {
        var addGradeSeparatedJunction = new AddGradeSeparatedJunction
        {
            TemporaryId = ObjectProvider.Create<GradeSeparatedJunctionId>(),
            Type = ObjectProvider.Create<GradeSeparatedJunctionType>(),
            UpperSegmentId = ObjectProvider.Create<RoadSegmentId>(),
            LowerSegmentId = TestData.Segment1Added.Id
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddGradeSeparatedJunction = addGradeSeparatedJunction
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "UpperRoadSegmentMissing",
                                Parameters = Array.Empty<ProblemParameter>()
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_grade_separated_junction_with_segments_that_do_not_intersect()
    {
        var addGradeSeparatedJunction = new AddGradeSeparatedJunction
        {
            TemporaryId = ObjectProvider.Create<GradeSeparatedJunctionId>(),
            Type = ObjectProvider.Create<GradeSeparatedJunctionType>(),
            UpperSegmentId = TestData.Segment1Added.Id,
            LowerSegmentId = TestData.Segment2Added.Id
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddGradeSeparatedJunction = addGradeSeparatedJunction
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "UpperAndLowerRoadSegmentDoNotIntersect",
                                Parameters = Array.Empty<ProblemParameter>()
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_grade_separated_junction_with_segments_that_intersect()
    {
        TestData.Segment1Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new[]
        {
            new LineString(
                new CoordinateArraySequence(new Coordinate[] { new CoordinateM(0.0, 0.0), new CoordinateM(50.0, 50.0) }),
                GeometryConfiguration.GeometryFactory)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            }
        }));
        TestData.Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        TestData.Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        TestData.Segment2Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new[]
        {
            new LineString(
                new CoordinateArraySequence(new Coordinate[] { new CoordinateM(0.0, 50.0), new CoordinateM(50.0, 0.0) }),
                GeometryConfiguration.GeometryFactory)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            }
        }));
        TestData.Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        TestData.Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        var addGradeSeparatedJunction = new AddGradeSeparatedJunction
        {
            TemporaryId = ObjectProvider.Create<GradeSeparatedJunctionId>(),
            Type = ObjectProvider.Create<GradeSeparatedJunctionType>(),
            UpperSegmentId = TestData.Segment1Added.Id,
            LowerSegmentId = TestData.Segment2Added.Id
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddGradeSeparatedJunction = addGradeSeparatedJunction
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
                        {
                            Id = 1,
                            TemporaryId = addGradeSeparatedJunction.TemporaryId,
                            UpperRoadSegmentId = TestData.Segment1Added.Id,
                            LowerRoadSegmentId = TestData.Segment2Added.Id,
                            Type = addGradeSeparatedJunction.Type
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_missing_segment_to_a_european_road()
    {
        var addRoadSegmentToEuropeanRoad = new AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<EuropeanRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentMissing",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = addRoadSegmentToEuropeanRoad.SegmentId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_missing_segment_to_a_national_road()
    {
        var addRoadSegmentToNationalRoad = new AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NationalRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentMissing",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = addRoadSegmentToNationalRoad.SegmentId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_missing_segment_to_a_numbered_road()
    {
        var addRoadSegmentToNumberedRoad = new AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NumberedRoadNumber>(),
            Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentMissing",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = addRoadSegmentToNumberedRoad.SegmentId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_that_intersects_without_grade_separated_junction()
    {
        var geometry1 = new MultiLineString(new[]
        {
            new LineString(
                new CoordinateArraySequence(new Coordinate[] { new CoordinateM(0.0, 0.0), new CoordinateM(50.0, 50.0) }),
                GeometryConfiguration.GeometryFactory)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            }
        });
        TestData.Segment1Added.Geometry = GeometryTranslator.Translate(geometry1);

        TestData.Segment1Added.Lanes = new[] { TestData.ObjectProvider.CreateRoadSegmentLaneAttribute(geometry1.Length) };
        TestData.Segment1Added.Widths = new[] { TestData.ObjectProvider.CreateRoadSegmentWidthAttribute(geometry1.Length) };
        TestData.Segment1Added.Surfaces = new[] { TestData.ObjectProvider.CreateRoadSegmentSurfaceAttribute(geometry1.Length) };

        var startPoint2 = new Point(new CoordinateM(0.0, 50.0));
        var endPoint2 = new Point(new CoordinateM(50.0, 0.0));
        var geometry2 = new MultiLineString(new[]
        {
            new LineString(
                new CoordinateArraySequence(new[] { startPoint2.Coordinate, endPoint2.Coordinate }),
                GeometryConfiguration.GeometryFactory)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            }
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        startPoint2.M = TestData.AddSegment2.Geometry.MultiLineString[0].Measures[0];
        endPoint2.M = TestData.AddSegment2.Geometry.MultiLineString[0].Measures[1];

        TestData.AddSegment2.Lanes = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length) };
        TestData.AddSegment2.Widths = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length) };
        TestData.AddSegment2.Surfaces = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length) };
        TestData.AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ModifiedRoadSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "IntersectingRoadSegmentId",
                                        Value = TestData.Segment1Added.Id.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_to_a_european_road()
    {
        var addRoadSegmentToEuropeanRoad = new AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<EuropeanRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToEuropeanRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToEuropeanRoad.Number,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = TestData.Segment1Added.Version
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public async Task when_only_adding_a_segment_to_a_european_road_should_bump_segment_version()
    {
        var addRoadSegmentToEuropeanRoad = new AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<EuropeanRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
            SegmentId = TestData.Segment1Added.Id
        };

        await Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworkStreamNameProvider.Default,
                new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = TestData.StartNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.EndNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = TestData.Segment1Added,
                            Problems = []
                        }
                    ]
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes =
                [
                    new AcceptedChange
                    {
                        RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToEuropeanRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToEuropeanRoad.Number,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = 2
                        },
                        Problems = []
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_to_a_national_road()
    {
        var addRoadSegmentToNationalRoad = new AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NationalRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToNationalRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToNationalRoad.Number,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = TestData.Segment1Added.Version
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_only_adding_a_segment_to_a_national_road_should_bump_segment_version()
    {
        var addRoadSegmentToNationalRoad = new AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NationalRoadNumber>(),
            SegmentGeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
            SegmentId = TestData.Segment1Added.Id
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworkStreamNameProvider.Default,
                new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = TestData.StartNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.EndNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = TestData.Segment1Added,
                            Problems = []
                        }
                    ]
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToNationalRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToNationalRoad.Number,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = 2
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_to_a_numbered_road()
    {
        var addRoadSegmentToNumberedRoad = new AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NumberedRoadNumber>(),
            Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>(),
            SegmentGeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod,
            SegmentId = TestData.AddSegment1.TemporaryId
        };
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToNumberedRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToNumberedRoad.Number,
                            Direction = addRoadSegmentToNumberedRoad.Direction,
                            Ordinal = addRoadSegmentToNumberedRoad.Ordinal,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = TestData.Segment1Added.Version
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_only_adding_a_segment_to_a_numbered_road_should_bump_segment_version()
    {
        var addRoadSegmentToNumberedRoad = new AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = ObjectProvider.Create<AttributeId>(),
            Number = ObjectProvider.Create<NumberedRoadNumber>(),
            Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>(),
            SegmentGeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
            SegmentId = TestData.Segment1Added.Id
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworkStreamNameProvider.Default,
                new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = TestData.StartNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = TestData.EndNode1Added,
                            Problems = []
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = TestData.Segment1Added,
                            Problems = []
                        }
                    ]
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                        {
                            AttributeId = 1,
                            TemporaryAttributeId = addRoadSegmentToNumberedRoad.TemporaryAttributeId,
                            Number = addRoadSegmentToNumberedRoad.Number,
                            Direction = addRoadSegmentToNumberedRoad.Direction,
                            Ordinal = addRoadSegmentToNumberedRoad.Ordinal,
                            SegmentId = TestData.Segment1Added.Id,
                            SegmentVersion = 2
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_whose_end_point_does_not_match_its_end_node_geometry()
    {
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(TestData.MiddlePoint1);

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_whose_end_point_does_not_match_its_existing_end_node_geometry()
    {
        TestData.AddSegment2.EndNodeId = TestData.EndNode1Added.Id;
        TestData.StartNode1Added.Type = RoadNodeType.EndNode.ToString();
        TestData.EndNode1Added.Type = RoadNodeType.FakeNode.ToString();
        TestData.AddStartNode2.Type = RoadNodeType.EndNode.ToString();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment2.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_whose_start_point_does_not_match_its_existing_start_node_geometry()
    {
        TestData.AddSegment2.StartNodeId = TestData.StartNode1Added.Id;
        TestData.StartNode1Added.Type = RoadNodeType.FakeNode.ToString();
        TestData.EndNode1Added.Type = RoadNodeType.EndNode.ToString();
        TestData.AddEndNode2.Type = RoadNodeType.EndNode.ToString();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment2.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_whose_start_point_does_not_match_its_start_node_geometry()
    {
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(TestData.MiddlePoint1);

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_geometry_that_has_been_taken()
    {
        TestData.StartNode1Added.Type = RoadNodeType.FakeNode.ToString();
        TestData.EndNode1Added.Type = RoadNodeType.FakeNode.ToString();
        TestData.AddSegment2.StartNodeId = TestData.StartNode1Added.Id;
        TestData.AddSegment2.EndNodeId = TestData.EndNode1Added.Id;
        TestData.AddSegment2.Geometry = TestData.Segment1Added.Geometry;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentGeometryTaken",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ByOtherSegment",
                                        Value = "1"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_geometry_that_self_intersects()
    {
        var startPoint = new Point(new CoordinateM(0.0, 10.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var middlePoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var middlePoint2 = new Point(new CoordinateM(5.0, 20.0, 21.18033988749895))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint = new Point(new CoordinateM(5.0, 0.0, 41.180339887498945))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var lineString = new LineString(
            new CoordinateArraySequence(new[] { startPoint.Coordinate, middlePoint1.Coordinate, middlePoint2.Coordinate, endPoint.Coordinate }),
            GeometryConfiguration.GeometryFactory
        );
        var multiLineString = new MultiLineString(
            new[]
            {
                lineString
            })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(multiLineString);
        TestData.AddSegment1.Lanes = TestData.AddSegment1.Lanes.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Lanes.Length);
            if (index == TestData.AddSegment1.Lanes.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Lanes.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes
            .Select((lane, index) => new RoadSegmentLaneAttributes
            {
                AttributeId = index + 1,
                Direction = lane.Direction,
                Count = lane.Count,
                FromPosition = lane.FromPosition,
                ToPosition = lane.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        TestData.AddSegment1.Widths = TestData.AddSegment1.Widths.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Widths.Length);
            if (index == TestData.AddSegment1.Widths.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Widths.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths
            .Select((width, index) => new RoadSegmentWidthAttributes
            {
                AttributeId = index + 1,
                Width = width.Width,
                FromPosition = width.FromPosition,
                ToPosition = width.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        TestData.AddSegment1.Surfaces = TestData.AddSegment1.Surfaces.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Surfaces.Length);
            if (index == TestData.AddSegment1.Surfaces.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Surfaces.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces
            .Select((surface, index) => new RoadSegmentSurfaceAttributes
            {
                AttributeId = index + 1,
                Type = surface.Type,
                FromPosition = surface.FromPosition,
                ToPosition = surface.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentGeometrySelfIntersects",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Theory]
    [MemberData(nameof(SelfOverlapsCases))]
    public Task when_adding_a_segment_with_a_geometry_that_self_overlaps(
        Point startPoint,
        Point endPoint,
        MultiLineString multiLineString)
    {
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(multiLineString);
        TestData.AddSegment1.Lanes = TestData.AddSegment1.Lanes.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Lanes.Length);
            if (index == TestData.AddSegment1.Lanes.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Lanes.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes
            .Select((lane, index) => new RoadSegmentLaneAttributes
            {
                AttributeId = index + 1,
                Direction = lane.Direction,
                Count = lane.Count,
                FromPosition = lane.FromPosition,
                ToPosition = lane.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        TestData.AddSegment1.Widths = TestData.AddSegment1.Widths.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Widths.Length);
            if (index == TestData.AddSegment1.Widths.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Widths.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths
            .Select((width, index) => new RoadSegmentWidthAttributes
            {
                AttributeId = index + 1,
                Width = width.Width,
                FromPosition = width.FromPosition,
                ToPosition = width.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        TestData.AddSegment1.Surfaces = TestData.AddSegment1.Surfaces.Select((part, index) =>
        {
            part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Surfaces.Length);
            if (index == TestData.AddSegment1.Surfaces.Length - 1)
            {
                part.ToPosition = Convert.ToDecimal(multiLineString.Length);
            }
            else
            {
                part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / TestData.AddSegment1.Surfaces.Length);
            }

            return part;
        }).ToArray();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces
            .Select((surface, index) => new RoadSegmentSurfaceAttributes
            {
                AttributeId = index + 1,
                Type = surface.Type,
                FromPosition = surface.FromPosition,
                ToPosition = surface.ToPosition,
                AsOfGeometryVersion = 1
            })
            .ToArray();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentGeometrySelfOverlaps",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_line_string_with_length_0()
    {
        var geometry = new MultiLineString(new[] { new LineString(Array.Empty<Coordinate>()) })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry);
        TestData.AddSegment1.Lanes = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(1) };
        TestData.AddSegment1.Widths = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(1) };
        TestData.AddSegment1.Surfaces = new[] { TestData.ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(1) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentGeometryLengthIsZero",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            },
                            new Problem
                            {
                                Reason = "RoadSegmentLaneAttributeToPositionNotEqualToLength",
                                Parameters = new[]
                                {
                                    new ProblemParameter { Name = "AttributeId", Value = TestData.Segment1Added.Lanes[0].AttributeId.ToString() },
                                    new ProblemParameter { Name = "ToPosition", Value = "1" },
                                    new ProblemParameter { Name = "Length", Value = "0" }
                                }
                            },
                            new Problem
                            {
                                Reason = "RoadSegmentWidthAttributeToPositionNotEqualToLength",
                                Parameters = new[]
                                {
                                    new ProblemParameter { Name = "AttributeId", Value = TestData.Segment1Added.Widths[0].AttributeId.ToString() },
                                    new ProblemParameter { Name = "ToPosition", Value = "1" },
                                    new ProblemParameter { Name = "Length", Value = "0" }
                                }
                            },
                            new Problem
                            {
                                Reason = "RoadSegmentSurfaceAttributeToPositionNotEqualToLength",
                                Parameters = new[]
                                {
                                    new ProblemParameter { Name = "AttributeId", Value = TestData.Segment1Added.Surfaces[0].AttributeId.ToString() },
                                    new ProblemParameter { Name = "ToPosition", Value = "1" },
                                    new ProblemParameter { Name = "Length", Value = "0" }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_line_string_with_length_at_least_100000()
    {
        var startPoint = new Point(new CoordinateM(0.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint = new Point(new CoordinateM(100000.0, 0.0, 100000.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var lineString = new LineString(
            new CoordinateArraySequence([startPoint.Coordinate, endPoint.Coordinate]),
            GeometryConfiguration.GeometryFactory
        );
        var geometry = new MultiLineString([lineString])
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry);
        TestData.AddSegment1.Lanes = [TestData.ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(lineString.Length)];
        TestData.AddSegment1.Widths = [TestData.ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(lineString.Length)];
        TestData.AddSegment1.Surfaces = [TestData.ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(lineString.Length)];
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes(asOfGeometryVersion: TestData.Segment1Added.GeometryVersion);

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes =
                [
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems =
                        [
                            new Problem
                            {
                                Reason = "RoadSegmentGeometryLengthTooLong",
                                Parameters =
                                [
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString()),
                                    new("TooLongSegmentLength", "100000")
                                ]
                            }
                        ]
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_missing_end_node()
    {
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentEndNodeMissing",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_segment_with_a_missing_start_node()
    {
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentStartNodeMissing",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Theory]
    [MemberData(nameof(NonAdjacentLaneAttributesCases))]
    public Task when_adding_a_segment_with_non_adjacent_lane_attributes(RequestedRoadSegmentLaneAttribute[] attributes, Problem problem)
    {
        TestData.AddSegment1.Lanes = attributes;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[] { problem }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Theory]
    [MemberData(nameof(NonAdjacentSurfaceAttributesCases))]
    public Task when_adding_a_segment_with_non_adjacent_surface_attributes(RequestedRoadSegmentSurfaceAttribute[] attributes, Problem problem)
    {
        TestData.AddSegment1.Surfaces = attributes;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[] { problem }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Theory]
    [MemberData(nameof(NonAdjacentWidthAttributesCases))]
    public Task when_adding_a_segment_with_non_adjacent_width_attributes(RequestedRoadSegmentWidthAttribute[] attributes, Problem problem)
    {
        TestData.AddSegment1.Widths = attributes;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[] { problem }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_start_and_end_node_and_segment()
    {
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_measured_and_outlined_segment_simultaneously()
    {
        TestData.AddSegment1.PermanentId = 1;

        TestData.AddSegment2.PermanentId = 2;
        TestData.AddSegment2.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        TestData.AddSegment2.Status = ObjectProvider.CreateUntil<RoadSegmentStatus>(x => x.IsValidForEdit());
        TestData.AddSegment2.Morphology = ObjectProvider.CreateUntil<RoadSegmentMorphology>(x => x.IsValidForEdit());
        TestData.AddSegment2.StartNodeId = 0;
        TestData.AddSegment2.EndNodeId = 0;
        TestData.AddSegment2.Lanes = TestData.AddSegment2.Lanes.Take(1).ToArray();
        TestData.AddSegment2.Lanes[0].FromPosition = 0;
        TestData.AddSegment2.Lanes[0].ToPosition = RoadSegmentPosition.FromDouble(GeometryTranslator.Translate(TestData.AddSegment2.Geometry).Length);
        TestData.AddSegment2.Lanes[0].Count = ObjectProvider.CreateUntil<RoadSegmentLaneCount>(x => x.IsValidForEdit());
        TestData.AddSegment2.Surfaces = TestData.AddSegment2.Surfaces.Take(1).ToArray();
        TestData.AddSegment2.Surfaces[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.AddSegment2.Surfaces[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.AddSegment2.Widths = TestData.AddSegment2.Widths.Take(1).ToArray();
        TestData.AddSegment2.Widths[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.AddSegment2.Widths[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.AddSegment2.Widths[0].Width = ObjectProvider.CreateUntil<RoadSegmentWidth>(x => x.IsValidForEdit());

        TestData.Segment2Added.GeometryDrawMethod = TestData.AddSegment2.GeometryDrawMethod;
        TestData.Segment2Added.Status = TestData.AddSegment2.Status;
        TestData.Segment2Added.Morphology = TestData.AddSegment2.Morphology;
        TestData.Segment2Added.StartNodeId = TestData.AddSegment2.StartNodeId;
        TestData.Segment2Added.EndNodeId = TestData.AddSegment2.EndNodeId;
        TestData.Segment2Added.Lanes = TestData.Segment2Added.Lanes.Take(1).ToArray();
        TestData.Segment2Added.Lanes[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.Segment2Added.Lanes[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.Segment2Added.Lanes[0].Count = TestData.AddSegment2.Lanes[0].Count;
        TestData.Segment2Added.Surfaces = TestData.Segment2Added.Surfaces.Take(1).ToArray();
        TestData.Segment2Added.Surfaces[0].FromPosition = TestData.AddSegment2.Surfaces[0].FromPosition;
        TestData.Segment2Added.Surfaces[0].ToPosition = TestData.AddSegment2.Surfaces[0].ToPosition;
        TestData.Segment2Added.Widths = TestData.Segment2Added.Widths.Take(1).ToArray();
        TestData.Segment2Added.Widths[0].FromPosition = TestData.AddSegment2.Widths[0].FromPosition;
        TestData.Segment2Added.Widths[0].ToPosition = TestData.AddSegment2.Widths[0].ToPosition;
        TestData.Segment2Added.Widths[0].Width = TestData.AddSegment2.Widths[0].Width;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .Then(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(TestData.Segment2Added.Id)), new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_measured_and_outlined_segment_simultaneously_but_measured_change_is_rejected()
    {
        TestData.AddSegment1.PermanentId = 1;

        TestData.AddSegment2.PermanentId = 2;
        TestData.AddSegment2.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        TestData.AddSegment2.Status = ObjectProvider.CreateUntil<RoadSegmentStatus>(x => x.IsValidForEdit());
        TestData.AddSegment2.Morphology = ObjectProvider.CreateUntil<RoadSegmentMorphology>(x => x.IsValidForEdit());
        TestData.AddSegment2.StartNodeId = 0;
        TestData.AddSegment2.EndNodeId = 0;
        TestData.AddSegment2.Lanes = TestData.AddSegment2.Lanes.Take(1).ToArray();
        TestData.AddSegment2.Lanes[0].FromPosition = 0;
        TestData.AddSegment2.Lanes[0].ToPosition = RoadSegmentPosition.FromDouble(GeometryTranslator.Translate(TestData.AddSegment2.Geometry).Length);
        TestData.AddSegment2.Lanes[0].Count = ObjectProvider.CreateUntil<RoadSegmentLaneCount>(x => x.IsValidForEdit());
        TestData.AddSegment2.Surfaces = TestData.AddSegment2.Surfaces.Take(1).ToArray();
        TestData.AddSegment2.Surfaces[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.AddSegment2.Surfaces[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.AddSegment2.Widths = TestData.AddSegment2.Widths.Take(1).ToArray();
        TestData.AddSegment2.Widths[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.AddSegment2.Widths[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.AddSegment2.Widths[0].Width = ObjectProvider.CreateUntil<RoadSegmentWidth>(x => x.IsValidForEdit());

        TestData.Segment2Added.GeometryDrawMethod = TestData.AddSegment2.GeometryDrawMethod;
        TestData.Segment2Added.Status = TestData.AddSegment2.Status;
        TestData.Segment2Added.Morphology = TestData.AddSegment2.Morphology;
        TestData.Segment2Added.StartNodeId = TestData.AddSegment2.StartNodeId;
        TestData.Segment2Added.EndNodeId = TestData.AddSegment2.EndNodeId;
        TestData.Segment2Added.Lanes = TestData.Segment2Added.Lanes.Take(1).ToArray();
        TestData.Segment2Added.Lanes[0].FromPosition = TestData.AddSegment2.Lanes[0].FromPosition;
        TestData.Segment2Added.Lanes[0].ToPosition = TestData.AddSegment2.Lanes[0].ToPosition;
        TestData.Segment2Added.Lanes[0].Count = TestData.AddSegment2.Lanes[0].Count;
        TestData.Segment2Added.Surfaces = TestData.Segment2Added.Surfaces.Take(1).ToArray();
        TestData.Segment2Added.Surfaces[0].FromPosition = TestData.AddSegment2.Surfaces[0].FromPosition;
        TestData.Segment2Added.Surfaces[0].ToPosition = TestData.AddSegment2.Surfaces[0].ToPosition;
        TestData.Segment2Added.Widths = TestData.Segment2Added.Widths.Take(1).ToArray();
        TestData.Segment2Added.Widths[0].FromPosition = TestData.AddSegment2.Widths[0].FromPosition;
        TestData.Segment2Added.Widths[0].ToPosition = TestData.AddSegment2.Widths[0].ToPosition;
        TestData.Segment2Added.Widths[0].Width = TestData.AddSegment2.Widths[0].Width;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadSegmentEndNodeMissing",
                                Parameters = new ProblemParameter[]
                                {
                                    new("Identifier", TestData.AddSegment1.TemporaryId.ToString())
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_removing_an_outlined_segment()
    {
        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        TestData.Segment1Added.Status = ObjectProvider.CreateUntil<RoadSegmentStatus>(x => x.IsValidForEdit());
        TestData.Segment1Added.Morphology = ObjectProvider.CreateUntil<RoadSegmentMorphology>(x => x.IsValidForEdit());
        TestData.Segment1Added.StartNodeId = 0;
        TestData.Segment1Added.EndNodeId = 0;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(TestData.Segment1Added.Id)), new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    RemoveOutlinedRoadSegment = new RemoveOutlinedRoadSegment
                    {
                        Id = TestData.Segment1Added.Id
                    }
                }
            ))
            .Then(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(TestData.Segment1Added.Id)), new RoadNetworkChangesAccepted
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
                        OutlinedRoadSegmentRemoved = new OutlinedRoadSegmentRemoved
                        {
                            Id = TestData.Segment1Added.Id
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_and_end_node_and_segment_to_an_existing_segment()
    {
        var nextWidthsAttributeId = TestData.AddSegment1.Widths.Length + 1;
        var nextSurfacesAttributeId = TestData.AddSegment1.Surfaces.Length + 1;

        TestData.StartNode1Added.Geometry = new RoadNodeGeometry
        {
            Point = new RoadRegistry.BackOffice.Messages.Point { X = 0, Y = 0 },
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.EndNode1Added.Geometry = new RoadNodeGeometry
        {
            Point = new RoadRegistry.BackOffice.Messages.Point { X = 0, Y = 10 },
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.Segment1Added.Geometry = new RoadSegmentGeometry
        {
            MultiLineString = new[]
            {
                new RoadRegistry.BackOffice.Messages.LineString
                {
                    Measures = new[] { 0.0, 10 },
                    Points = new[] { TestData.StartNode1Added.Geometry.Point, TestData.EndNode1Added.Geometry.Point }
                }
            },
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.Segment1Added.Lanes = new[]
        {
            new RoadSegmentLaneAttributes
            {
                FromPosition = 0,
                ToPosition = 10,
                Count = 1,
                Direction = RoadSegmentLaneDirection.Unknown
            }
        };

        TestData.ModifyEndNode1.Type = RoadNodeType.FakeNode;
        TestData.ModifyEndNode1.Geometry = TestData.EndNode1Added.Geometry;
        TestData.AddEndNode2.Geometry = new RoadNodeGeometry
        {
            Point = new RoadRegistry.BackOffice.Messages.Point { X = 0, Y = 20 },
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = new RoadSegmentGeometry
        {
            MultiLineString = new[]
            {
                new RoadRegistry.BackOffice.Messages.LineString
                {
                    Measures = new[] { 0.0, 10 },
                    Points = new[] { TestData.ModifyEndNode1.Geometry.Point, TestData.AddEndNode2.Geometry.Point }
                }
            },
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Lanes = new[]
        {
            new RequestedRoadSegmentLaneAttribute
            {
                FromPosition = 0,
                ToPosition = 10,
                Count = 1,
                Direction = RoadSegmentLaneDirection.Forward
            }
        };
        TestData.AddSegment2.Widths = new[]
        {
            new RequestedRoadSegmentWidthAttribute
            {
                FromPosition = 0,
                ToPosition = 10,
                Width = 2
            }
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            new RequestedRoadSegmentSurfaceAttribute
            {
                FromPosition = 0,
                ToPosition = 10,
                Type = RoadSegmentSurfaceType.Unknown
            }
        };

        TestData.AddSegment2.StartNodeId = TestData.ModifyEndNode1.Id;
        TestData.AddSegment2.EndNodeId = TestData.AddEndNode2.TemporaryId;

        TestData.EndNode2Added.Geometry = TestData.AddEndNode2.Geometry;
        TestData.EndNode2Added.Id = 3;

        TestData.Segment2Added.Geometry = TestData.AddSegment2.Geometry;
        TestData.Segment2Added.Lanes = new[]
        {
            new RoadSegmentLaneAttributes
            {
                FromPosition = TestData.AddSegment2.Lanes[0].FromPosition,
                ToPosition = TestData.AddSegment2.Lanes[0].ToPosition,
                Count = TestData.AddSegment2.Lanes[0].Count,
                Direction = TestData.AddSegment2.Lanes[0].Direction,
                AsOfGeometryVersion = 1,
                AttributeId = 1
            }
        };
        TestData.Segment2Added.Widths = new[]
        {
            new RoadSegmentWidthAttributes
            {
                FromPosition = TestData.AddSegment2.Widths[0].FromPosition,
                ToPosition = TestData.AddSegment2.Widths[0].ToPosition,
                Width = TestData.AddSegment2.Widths[0].Width,
                AsOfGeometryVersion = 1,
                AttributeId = nextWidthsAttributeId
            }
        };
        TestData.Segment2Added.Surfaces = new[]
        {
            new RoadSegmentSurfaceAttributes
            {
                FromPosition = TestData.AddSegment2.Surfaces[0].FromPosition,
                ToPosition = TestData.AddSegment2.Surfaces[0].ToPosition,
                Type = TestData.AddSegment2.Surfaces[0].Type,
                AsOfGeometryVersion = 1,
                AttributeId = nextSurfacesAttributeId
            }
        };
        TestData.Segment2Added.StartNodeId = TestData.ModifyEndNode1.Id;
        TestData.Segment2Added.EndNodeId = TestData.EndNode2Added.Id;

        TestData.EndNode1Modified.Geometry = TestData.ModifyEndNode1.Geometry;
        TestData.EndNode1Modified.Type = TestData.ModifyEndNode1.Type;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    ModifyRoadNode = TestData.ModifyEndNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeModified = TestData.EndNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_adding_a_start_node_connected_to_a_single_segment_as_a_node_other_than_an_end_node()
    {
        TestData.AddStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.EndNode)
            .ToString();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "EndNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "EndNode"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
    {
        TestData.AddStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
            .ToString();

        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint3 = new Point(new CoordinateM(0.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
        TestData.AddEndNode1.Type = RoadNodeType.EndNode.ToString();
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
        TestData.AddEndNode2.Type = RoadNodeType.EndNode.ToString();
        TestData.AddEndNode3.Geometry = GeometryTranslator.Translate(endPoint3);
        TestData.AddEndNode3.Type = RoadNodeType.EndNode.ToString();

        var geometry1 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint1.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.AddSegment1.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry1.Length) };
        TestData.AddSegment1.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry1.Length) };
        TestData.AddSegment1.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry1.Length) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment2.StartNodeId = TestData.AddStartNode1.TemporaryId;
        var geometry2 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint2.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length) };
        TestData.AddSegment2.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length) };
        TestData.AddSegment2.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length) };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment3.StartNodeId = TestData.AddStartNode1.TemporaryId;
        var geometry3 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint3.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment3.Geometry = GeometryTranslator.Translate(geometry3);
        TestData.AddSegment3.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry3.Length) };
        TestData.AddSegment3.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry3.Length) };
        TestData.AddSegment3.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry3.Length) };
        TestData.Segment3Added.Lanes = TestData.AddSegment3.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment3Added.Widths = TestData.AddSegment3.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment3Added.Surfaces = TestData.AddSegment3.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode3
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment3
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment3,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Theory(Skip = "Not working for some reason")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute(int testCase)
    {
        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.StartNode1Added.Geometry = TestData.AddStartNode1.Geometry;
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
        TestData.EndNode1Added.Geometry = TestData.AddEndNode1.Geometry;
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
        TestData.EndNode2Added.Geometry = TestData.AddEndNode2.Geometry;
        TestData.AddSegment1.Lanes = Array.Empty<RequestedRoadSegmentLaneAttribute>();
        TestData.Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.AddSegment1.Widths = Array.Empty<RequestedRoadSegmentWidthAttribute>();
        TestData.Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        TestData.AddSegment1.Surfaces = Array.Empty<RequestedRoadSegmentSurfaceAttribute>();
        TestData.Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint.Coordinate, endPoint1.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        TestData.Segment1Added.Geometry = TestData.AddSegment1.Geometry;
        TestData.AddSegment2.Lanes = Array.Empty<RequestedRoadSegmentLaneAttribute>();
        TestData.Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.AddSegment2.Widths = Array.Empty<RequestedRoadSegmentWidthAttribute>();
        TestData.Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        TestData.AddSegment2.Surfaces = Array.Empty<RequestedRoadSegmentSurfaceAttribute>();
        TestData.Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint.Coordinate, endPoint2.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        TestData.Segment2Added.Geometry = TestData.AddSegment2.Geometry;
        TestData.AddStartNode1.Type = RoadNodeType.FakeNode.ToString();
        TestData.StartNode1Added.Type = TestData.AddStartNode1.Type;
        TestData.AddSegment2.StartNodeId = TestData.AddStartNode1.TemporaryId;
        TestData.Segment2Added.StartNodeId = TestData.StartNode1Added.Id;
        TestData.EndNode2Added.Id = 3;
        TestData.Segment2Added.EndNodeId = TestData.EndNode2Added.Id;

        var addGradeSeparatedJunction = new AddGradeSeparatedJunction
        {
            TemporaryId = ObjectProvider.Create<GradeSeparatedJunctionId>(),
            Type = ObjectProvider.Create<GradeSeparatedJunctionType>(),
            UpperSegmentId = TestData.Segment1Added.Id,
            LowerSegmentId = TestData.Segment2Added.Id
        };

        TestData.AddSegment2.Status = TestData.AddSegment1.Status;
        TestData.Segment2Added.Status = TestData.AddSegment1.Status;
        TestData.AddSegment2.Morphology = TestData.AddSegment1.Morphology;
        TestData.Segment2Added.Morphology = TestData.AddSegment1.Morphology;
        TestData.AddSegment2.Category = TestData.AddSegment1.Category;
        TestData.Segment2Added.Category = TestData.AddSegment1.Category;
        TestData.AddSegment2.MaintenanceAuthority = TestData.AddSegment1.MaintenanceAuthority;
        TestData.Segment2Added.MaintenanceAuthority.Code = TestData.AddSegment1.MaintenanceAuthority;
        TestData.AddSegment2.AccessRestriction = TestData.AddSegment1.AccessRestriction;
        TestData.Segment2Added.AccessRestriction = TestData.AddSegment1.AccessRestriction;
        TestData.AddSegment2.LeftSideStreetNameId = TestData.AddSegment1.LeftSideStreetNameId;
        TestData.Segment2Added.LeftSide.StreetNameId = TestData.AddSegment1.LeftSideStreetNameId;
        TestData.AddSegment2.RightSideStreetNameId = TestData.AddSegment1.RightSideStreetNameId;
        TestData.Segment2Added.RightSide.StreetNameId = TestData.AddSegment1.RightSideStreetNameId;

        switch (testCase)
        {
            case 0:
                TestData.AddSegment2.Status = new Generator<RoadSegmentStatus>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.Status);
                TestData.Segment2Added.Status = TestData.AddSegment2.Status;
                break;
            case 1:
                TestData.AddSegment2.Morphology = new Generator<RoadSegmentMorphology>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.Morphology);
                TestData.Segment2Added.Morphology = TestData.AddSegment2.Morphology;
                break;
            case 2:
                TestData.AddSegment2.Category = new Generator<RoadSegmentCategory>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.Category);
                ;
                TestData.Segment2Added.Category = TestData.AddSegment2.Category;
                break;
            case 3:
                TestData.AddSegment2.MaintenanceAuthority = new Generator<OrganizationId>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.MaintenanceAuthority);
                TestData.Segment2Added.MaintenanceAuthority.Code = TestData.AddSegment2.MaintenanceAuthority;
                TestData.Segment2Added.MaintenanceAuthority.Name = ObjectProvider.Create<OrganizationName>();
                //TestData.AddSegment2.MaintenanceAuthority = TestData.ChangedByOrganization;
                //TestData.Segment2Added.MaintenanceAuthority.Code = TestData.ChangedByOrganization;
                //TestData.Segment2Added.MaintenanceAuthority.Name = TestData.ChangedByOrganizationName;
                break;
            case 4:
                TestData.AddSegment2.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.AccessRestriction);
                TestData.Segment2Added.AccessRestriction = TestData.AddSegment2.AccessRestriction;
                break;
            case 5:
                TestData.AddSegment2.LeftSideStreetNameId = new Generator<StreetNameLocalId?>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.LeftSideStreetNameId);
                TestData.Segment2Added.LeftSide.StreetNameId = TestData.AddSegment2.LeftSideStreetNameId;
                break;
            case 6:
                TestData.AddSegment2.RightSideStreetNameId = new Generator<StreetNameLocalId?>(ObjectProvider)
                    .First(candidate => candidate != TestData.AddSegment1.RightSideStreetNameId);
                TestData.Segment2Added.RightSide.StreetNameId = TestData.AddSegment2.RightSideStreetNameId;
                break;
        }

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(Organizations.ToStreamName(new OrganizationId(TestData.AddSegment2.MaintenanceAuthority)),
                new ImportedOrganization
                {
                    Code = TestData.Segment2Added.MaintenanceAuthority.Code,
                    Name = TestData.Segment2Added.MaintenanceAuthority.Name,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                },
                new RequestedChange
                {
                    AddGradeSeparatedJunction = addGradeSeparatedJunction
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
                        {
                            Id = 1,
                            TemporaryId = addGradeSeparatedJunction.TemporaryId,
                            UpperRoadSegmentId = TestData.Segment1Added.Id,
                            LowerRoadSegmentId = TestData.Segment2Added.Id,
                            Type = addGradeSeparatedJunction.Type
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
    {
        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
        TestData.EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint1);
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
        TestData.EndNode2Added.Geometry = GeometryTranslator.Translate(endPoint2);

        var geometry1 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint1.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.Segment1Added.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.AddSegment1.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry1.Length, attributeId: 1) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes();

        var geometry2 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint2.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.Segment2Added.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddStartNode1.Type = RoadNodeType.FakeNode.ToString();
        TestData.StartNode1Added.Type = RoadNodeType.FakeNode.ToString();

        TestData.AddSegment2.StartNodeId = TestData.AddStartNode1.TemporaryId;
        TestData.AddSegment2.Status = TestData.AddSegment1.Status;
        TestData.AddSegment2.Morphology = TestData.AddSegment1.Morphology;
        TestData.AddSegment2.Category = TestData.AddSegment1.Category;
        TestData.AddSegment2.MaintenanceAuthority = TestData.AddSegment1.MaintenanceAuthority;
        TestData.AddSegment2.AccessRestriction = TestData.AddSegment1.AccessRestriction;
        TestData.AddSegment2.LeftSideStreetNameId = TestData.AddSegment1.LeftSideStreetNameId;
        TestData.AddSegment2.RightSideStreetNameId = TestData.AddSegment1.RightSideStreetNameId;
        TestData.AddSegment2.GeometryDrawMethod = TestData.AddSegment1.GeometryDrawMethod;

        TestData.Segment2Added.StartNodeId = TestData.StartNode1Added.Id;
        TestData.Segment2Added.Status = TestData.Segment1Added.Status;
        TestData.Segment2Added.Morphology = TestData.Segment1Added.Morphology;
        TestData.Segment2Added.Category = TestData.Segment1Added.Category;
        TestData.Segment2Added.MaintenanceAuthority = TestData.Segment1Added.MaintenanceAuthority;
        TestData.Segment2Added.AccessRestriction = TestData.Segment1Added.AccessRestriction;
        TestData.Segment2Added.LeftSide = TestData.Segment1Added.LeftSide;
        TestData.Segment2Added.RightSide = TestData.Segment1Added.RightSide;
        TestData.Segment2Added.GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod;

        TestData.EndNode2Added.Id = 3;
        TestData.Segment2Added.EndNodeId = 3;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    }
                                }
                            }
                        }
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    }
                                }
                            }
                        }
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
    {
        TestData.AddStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
            .ToString();

        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);

        var geometry1 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint1.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.AddSegment1.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry1.Length, attributeId: 1) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment2.StartNodeId = TestData.AddStartNode1.TemporaryId;
        var geometry2 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint.Coordinate, endPoint2.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length, attributeId: 1) };
        TestData.AddSegment2.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length, attributeId: 1) };
        TestData.AddSegment2.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length, attributeId: 1) };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddStartNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_node_that_is_within_two_meters_of_another_segment()
    {
        do
        {
            var random = new Random();
            var startPoint = new Point(new CoordinateM(
                    TestData.StartPoint1.X + random.Next(1, 1000) / 1000.0 * Distances.TooClose,
                    TestData.StartPoint1.Y + random.Next(1, 1000) / 1000.0 * Distances.TooClose
                ))
                { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            TestData.AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                        new[]
                        {
                            new LineString(
                                new CoordinateArraySequence(new Coordinate[]
                                {
                                    new CoordinateM(startPoint.X, startPoint.Y, 0.0),
                                    new CoordinateM(TestData.MiddlePoint2.X, TestData.MiddlePoint2.Y, startPoint.Distance(TestData.MiddlePoint2)),
                                    new CoordinateM(TestData.EndPoint2.X, TestData.EndPoint2.Y,
                                        startPoint.Distance(TestData.MiddlePoint2) + TestData.MiddlePoint2.Distance(TestData.EndPoint2))
                                }),
                                GeometryConfiguration.GeometryFactory
                            )
                        })
                    { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() }
            );
            TestData.AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint);

            TestData.StartNode2Added.Geometry = TestData.AddStartNode2.Geometry;
            TestData.Segment2Added.Geometry = TestData.AddSegment2.Geometry;
        } while (GeometryTranslator.Translate(TestData.Segment1Added.Geometry).Intersects(GeometryTranslator.Translate(TestData.AddSegment2.Geometry)));

        var geometry2 = GeometryTranslator.Translate(TestData.AddSegment2.Geometry);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode2Added,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTooClose",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ToOtherSegment",
                                        Value = "1"
                                    }
                                }
                            }
                        }
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_a_start_node_with_a_geometry_that_has_been_taken()
    {
        TestData.AddStartNode2.Geometry = TestData.StartNode1Added.Geometry;

        var geometry2 = new MultiLineString(
                new[]
                {
                    new LineString(
                        new CoordinateArraySequence(new Coordinate[]
                        {
                            new CoordinateM(TestData.StartPoint1.X, TestData.StartPoint1.Y, 0.0),
                            new CoordinateM(TestData.MiddlePoint2.X, TestData.MiddlePoint2.Y, TestData.StartPoint1.Distance(TestData.MiddlePoint2)),
                            new CoordinateM(TestData.EndPoint2.X, TestData.EndPoint2.Y,
                                TestData.StartPoint1.Distance(TestData.MiddlePoint2) +
                                TestData.MiddlePoint2.Distance(TestData.EndPoint2))
                        }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddStartNode2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeGeometryTaken",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ByOtherNode",
                                        Value = "1"
                                    }
                                }
                            },
                            new Problem
                            {
                                Reason = "RoadNodeTooClose",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ToOtherSegment",
                                        Value = "1"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ModifiedRoadSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "IntersectingRoadSegmentId",
                                        Value = TestData.Segment1Added.Id.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_an_end_node_connected_to_a_single_segment_as_a_node_other_than_an_end_node()
    {
        TestData.AddEndNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.EndNode)
            .ToString();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddEndNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "EndNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "EndNode"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_an_end_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
    {
        TestData.AddEndNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
            .ToString();

        var endPoint = new Point(new CoordinateM(10.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint1 = new Point(new CoordinateM(10.0, 10.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint2 = new Point(new CoordinateM(20.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint3 = new Point(new CoordinateM(0.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
        TestData.AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
        TestData.AddStartNode3.Geometry = GeometryTranslator.Translate(startPoint3);

        var geometry1 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint1.Coordinate, endPoint.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.AddSegment1.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry1.Length, attributeId: 1) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment2.EndNodeId = TestData.AddEndNode1.TemporaryId;
        var geometry2 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint2.Coordinate, endPoint.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment3.EndNodeId = TestData.AddEndNode1.TemporaryId;
        var geometry3 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint3.Coordinate, endPoint.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment3.Geometry = GeometryTranslator.Translate(geometry3);
        TestData.AddSegment3.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry3.Length,
                attributeId: TestData.Segment2Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment3.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry3.Length,
                attributeId: TestData.Segment2Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment3.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry3.Length,
                attributeId: TestData.Segment2Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment3Added.Lanes = TestData.AddSegment3.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment3Added.Widths = TestData.AddSegment3.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment3Added.Surfaces = TestData.AddSegment3.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode3
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment3
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddEndNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment3,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "3"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment3.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "RealNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "MiniRoundabout"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_an_end_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
    {
        TestData.AddEndNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
            .ToString();

        var endPoint = new Point(new CoordinateM(10.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint1 = new Point(new CoordinateM(10.0, 10.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint2 = new Point(new CoordinateM(20.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        TestData.AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
        TestData.AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);

        var geometry1 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint1.Coordinate, endPoint.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment1.Geometry = GeometryTranslator.Translate(geometry1);
        TestData.AddSegment1.Lanes = new[] { ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry1.Length, attributeId: 1) };
        TestData.AddSegment1.Surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry1.Length, attributeId: 1) };
        TestData.Segment1Added.Lanes = TestData.AddSegment1.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment1Added.Widths = TestData.AddSegment1.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment1Added.Surfaces = TestData.AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddSegment2.EndNodeId = TestData.AddEndNode1.TemporaryId;
        TestData.AddSegment2.Lanes = Array.Empty<RequestedRoadSegmentLaneAttribute>();
        TestData.Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.AddSegment2.Widths = Array.Empty<RequestedRoadSegmentWidthAttribute>();
        TestData.Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        TestData.AddSegment2.Surfaces = Array.Empty<RequestedRoadSegmentSurfaceAttribute>();
        TestData.Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        var geometry2 = new MultiLineString(new[]
        {
            new LineString(new CoordinateArraySequence(new[]
            {
                startPoint2.Coordinate, endPoint.Coordinate
            }), GeometryConfiguration.GeometryFactory)
        })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddEndNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = TestData.AddEndNode1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment1.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = TestData.AddEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "TurningLoopNode"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact(Skip = "This test should be about being within two meters of another segment")]
    public Task when_adding_an_end_node_that_is_within_two_meters_of_another_node()
    {
        var random = new Random();
        var endPoint = new Point(new CoordinateM(
            TestData.EndPoint1.X + random.NextDouble() / 2.0 * Distances.TooClose,
            TestData.EndPoint1.Y + random.NextDouble() / 2.0 * Distances.TooClose
        ));
        TestData.AddSegment2.Lanes = Array.Empty<RequestedRoadSegmentLaneAttribute>();
        TestData.Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(
            new MultiLineString(
                    new[]
                    {
                        new LineString(
                            new CoordinateArraySequence(new[] { TestData.StartPoint2.Coordinate, TestData.MiddlePoint2.Coordinate, endPoint.Coordinate }),
                            GeometryConfiguration.GeometryFactory
                        )
                    })
                { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() }
        );
        TestData.AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint);
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddEndNode2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTooClose",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ToOtherNode",
                                        Value = "2"
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_an_end_node_with_a_geometry_that_has_been_taken()
    {
        var geometry2 = new MultiLineString(
                new[]
                {
                    new LineString(
                        new CoordinateArraySequence(new Coordinate[]
                        {
                            new CoordinateM(TestData.StartPoint2.X, TestData.StartPoint2.Y),
                            new CoordinateM(TestData.MiddlePoint2.X, TestData.MiddlePoint2.Y),
                            new CoordinateM(TestData.EndPoint1.X, TestData.EndPoint1.Y)
                        }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        TestData.AddSegment2.Geometry = GeometryTranslator.Translate(geometry2);
        TestData.AddSegment2.Lanes = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Lanes.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Widths = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Widths.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.AddSegment2.Surfaces = new[]
        {
            ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(geometry2.Length,
                attributeId: TestData.Segment1Added.Surfaces.Select(x => x.AttributeId).Max() + 1)
        };
        TestData.Segment2Added.Lanes = TestData.AddSegment2.Lanes.ToRoadSegmentLaneAttributes();
        TestData.Segment2Added.Widths = TestData.AddSegment2.Widths.ToRoadSegmentWidthAttributes();
        TestData.Segment2Added.Surfaces = TestData.AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes();

        TestData.AddEndNode2.Geometry = TestData.EndNode1Added.Geometry;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        AddRoadNode = TestData.AddEndNode2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeGeometryTaken",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ByOtherNode",
                                        Value = "2"
                                    }
                                }
                            },
                            new Problem
                            {
                                Reason = "RoadNodeTooClose",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ToOtherSegment",
                                        Value = "1"
                                    }
                                }
                            }
                        }
                    },
                    new RejectedChange
                    {
                        AddRoadSegment = TestData.AddSegment2,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "ModifiedRoadSegmentId",
                                        Value = TestData.AddSegment2.TemporaryId.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "IntersectingRoadSegmentId",
                                        Value = TestData.Segment1Added.Id.ToString()
                                    }
                                }
                            }
                        }
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
    {
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode2
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode2
                },
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment2
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_changes_are_out_of_order()
    {
        // Permanent identity assignment is influenced by the order in which the change
        // appears in the list of changes (determinism allows for easier testing).
        TestData.StartNode1Added.Id = 2;
        TestData.EndNode1Added.Id = 1;
        TestData.Segment1Added.StartNodeId = 2;
        TestData.Segment1Added.EndNodeId = 1;

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadSegment = TestData.AddSegment1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadNode = TestData.AddStartNode1
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_modifying_a_segment_geometry()
    {
        var org2Code = ObjectProvider.CreateWhichIsDifferentThan(new OrganizationId(TestData.ChangedByOrganization));
        var org2Name = TestData.ChangedByOrganizationName;

        var geometry = ObjectProvider.Create<RoadSegmentGeometry>();
        var geometryLength = (decimal)GeometryTranslator.Translate(geometry).Length;

        var modifyRoadSegmentGeometry = new ModifyRoadSegmentGeometry
        {
            Id = TestData.Segment1Added.Id,
            GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
            Geometry = geometry,
            Lanes = new[]
            {
                new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
                    Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
                }
            },
            Surfaces = new[]
            {
                new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
                }
            },
            Widths = new[]
            {
                new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Width = ObjectProvider.Create<RoadSegmentWidth>()
                }
            }
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(Organizations.ToStreamName(org2Code),
                new ImportedOrganization
                {
                    Code = org2Code,
                    Name = org2Name,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    ModifyRoadSegmentGeometry = modifyRoadSegmentGeometry
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        RoadSegmentGeometryModified = new RoadSegmentGeometryModified
                        {
                            Id = modifyRoadSegmentGeometry.Id,
                            Version = TestData.Segment1Added.Version + 1,
                            GeometryVersion = TestData.Segment1Added.GeometryVersion + 1,
                            Geometry = modifyRoadSegmentGeometry.Geometry,
                            Lanes = modifyRoadSegmentGeometry.Lanes.Select(attribute => new RoadSegmentLaneAttributes
                            {
                                AttributeId = attribute.AttributeId,
                                FromPosition = attribute.FromPosition,
                                ToPosition = attribute.ToPosition,
                                Count = attribute.Count,
                                Direction = attribute.Direction,
                                AsOfGeometryVersion = 1
                            }).ToArray(),
                            Surfaces = modifyRoadSegmentGeometry.Surfaces.Select(attribute => new RoadSegmentSurfaceAttributes
                            {
                                AttributeId = attribute.AttributeId,
                                FromPosition = attribute.FromPosition,
                                ToPosition = attribute.ToPosition,
                                Type = attribute.Type,
                                AsOfGeometryVersion = 1
                            }).ToArray(),
                            Widths = modifyRoadSegmentGeometry.Widths.Select(attribute => new RoadSegmentWidthAttributes
                            {
                                AttributeId = attribute.AttributeId,
                                FromPosition = attribute.FromPosition,
                                ToPosition = attribute.ToPosition,
                                Width = attribute.Width,
                                AsOfGeometryVersion = 1
                            }).ToArray()
                        },
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_modifying_a_segment_geometry_with_length_at_least_100000()
    {
        var startPoint = new Point(new CoordinateM(0.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint = new Point(new CoordinateM(100000.0, 0.0, 100000.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var lineString = new LineString(
            new CoordinateArraySequence([startPoint.Coordinate, endPoint.Coordinate]),
            GeometryConfiguration.GeometryFactory
        );
        var geometry = new MultiLineString([lineString])
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        var geometryLength = (decimal)geometry.Length;

        var modifyRoadSegmentGeometry = new ModifyRoadSegmentGeometry
        {
            Id = TestData.Segment1Added.Id,
            GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
            Geometry = GeometryTranslator.Translate(geometry),
            Lanes = new[]
            {
                new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
                    Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
                }
            },
            Surfaces = new[]
            {
                new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
                }
            },
            Widths = new[]
            {
                new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = 1,
                    FromPosition = 0,
                    ToPosition = geometryLength,
                    Width = ObjectProvider.Create<RoadSegmentWidth>()
                }
            }
        };

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = TestData.ChangedByOrganization,
                    Name = TestData.ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            ).Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(
                TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
                new RequestedChange
                {
                    ModifyRoadSegmentGeometry = modifyRoadSegmentGeometry
                }
            ))
            .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
            {
                RequestId = TestData.RequestId,
                Reason = TestData.ReasonForChange,
                Operator = TestData.ChangedByOperator,
                OrganizationId = TestData.ChangedByOrganization,
                Organization = TestData.ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes =
                [
                    new RejectedChange
                    {
                        ModifyRoadSegmentGeometry = new ModifyRoadSegmentGeometry
                        {
                            Id = TestData.Segment1Added.Id,
                            Geometry = GeometryTranslator.Translate(geometry),
                            GeometryDrawMethod = modifyRoadSegmentGeometry.GeometryDrawMethod,
                            Lanes =
                            [
                                new RequestedRoadSegmentLaneAttribute
                                {
                                    AttributeId = 1,
                                    FromPosition = 0,
                                    ToPosition = geometryLength,
                                    Count = modifyRoadSegmentGeometry.Lanes.Single().Count,
                                    Direction = modifyRoadSegmentGeometry.Lanes.Single().Direction
                                }
                            ],
                            Surfaces =
                            [
                                new RequestedRoadSegmentSurfaceAttribute
                                {
                                    AttributeId = 1,
                                    FromPosition = 0,
                                    ToPosition = geometryLength,
                                    Type = modifyRoadSegmentGeometry.Surfaces.Single().Type,
                                }
                            ],
                            Widths =
                            [
                                new RequestedRoadSegmentWidthAttribute
                                {
                                    AttributeId = 1,
                                    FromPosition = 0,
                                    ToPosition = geometryLength,
                                    Width = modifyRoadSegmentGeometry.Widths.Single().Width,
                                }
                            ]
                        },
                        Problems =
                        [
                            new Problem
                            {
                                Reason = "RoadSegmentGeometryLengthTooLong",
                                Parameters =
                                [
                                    new("Identifier", TestData.Segment1Added.Id.ToString()),
                                    new("TooLongSegmentLength", "100000")
                                ]
                            }
                        ]
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }
}
