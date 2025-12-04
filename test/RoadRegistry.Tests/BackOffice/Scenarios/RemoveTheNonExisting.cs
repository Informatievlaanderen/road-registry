namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using Framework.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Point = RoadRegistry.BackOffice.Messages.Point;
using Problem = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem;
using ProblemParameter = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.ProblemParameter;
using ProblemSeverity = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.ProblemSeverity;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RemoveGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.RemoveGradeSeparatedJunction;
using RemoveRoadNode = RoadRegistry.BackOffice.Messages.RemoveRoadNode;
using RemoveRoadSegment = RoadRegistry.BackOffice.Messages.RemoveRoadSegment;
using RemoveRoadSegmentFromEuropeanRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromEuropeanRoad;
using RemoveRoadSegmentFromNationalRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromNationalRoad;
using RemoveRoadSegmentFromNumberedRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromNumberedRoad;
using RequestedRoadSegmentEuropeanRoadAttribute = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.RequestedRoadSegmentEuropeanRoadAttribute;
using RequestedRoadSegmentNationalRoadAttribute = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.RequestedRoadSegmentNationalRoadAttribute;
using RequestedRoadSegmentNumberedRoadAttribute = CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.RequestedRoadSegmentNumberedRoadAttribute;

public class RemoveTheNonExisting : RoadRegistryTestBase
{
    public RemoveTheNonExisting(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        ObjectProvider.CustomizePoint();
        ObjectProvider.CustomizePolylineM();

        ObjectProvider.CustomizeAttributeId();
        ObjectProvider.CustomizeOrganizationId();
        ObjectProvider.CustomizeOrganizationName();
        ObjectProvider.CustomizeRoadNodeId();
        ObjectProvider.CustomizeRoadNodeType();
        ObjectProvider.CustomizeRoadSegmentId();
        ObjectProvider.CustomizeRoadSegmentCategory();
        ObjectProvider.CustomizeRoadSegmentMorphology();
        ObjectProvider.CustomizeRoadSegmentStatus();
        ObjectProvider.CustomizeRoadSegmentAccessRestriction();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadDirection();
        ObjectProvider.CustomizeRoadSegmentGeometryDrawMethod();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadOrdinal();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentWidth();
        ObjectProvider.CustomizeEuropeanRoadNumber();
        ObjectProvider.CustomizeNationalRoadNumber();
        ObjectProvider.CustomizeNumberedRoadNumber();
        ObjectProvider.CustomizeOriginProperties();
        ObjectProvider.CustomizeGradeSeparatedJunctionId();
        ObjectProvider.CustomizeGradeSeparatedJunctionType();
        ObjectProvider.CustomizeArchiveId();
        ObjectProvider.CustomizeChangeRequestId();
        ObjectProvider.CustomizeReason();
        ObjectProvider.CustomizeOperatorName();
        ObjectProvider.CustomizeTransactionId();

        ObjectProvider.Customize<RequestedRoadSegmentEuropeanRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentNationalRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentNumberedRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.Number = ObjectProvider.Create<NumberedRoadNumber>();
                instance.Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                instance.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = ObjectProvider.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());

        ArchiveId = ObjectProvider.Create<ArchiveId>();
        RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
        ReasonForChange = ObjectProvider.Create<Reason>();
        ChangedByOperator = ObjectProvider.Create<OperatorName>();
        ChangedByOrganization = ObjectProvider.Create<OrganizationId>();
        ChangedByOrganizationName = ObjectProvider.Create<OrganizationName>();
    }

    public ArchiveId ArchiveId { get; }
    public OperatorName ChangedByOperator { get; }
    public OrganizationId ChangedByOrganization { get; }
    public OrganizationName ChangedByOrganizationName { get; }
    public Reason ReasonForChange { get; }
    public ChangeRequestId RequestId { get; }

    [Fact]
    public Task When_removing_a_non_existent_grade_separated_junction()
    {
        return Run(scenario =>
        {
            var startRoadNode1Geometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 0.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNode1Geometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startRoadNode2Geometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNode2Geometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 00.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var upperRoadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNode1Geometry.Point.X - endRoadNode1Geometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNode1Geometry.Point.Y - endRoadNode1Geometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNode1Geometry.Point, endRoadNode1Geometry.Point }
                    }
                }
            };
            var lowerRoadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNode2Geometry.Point.X - endRoadNode2Geometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNode2Geometry.Point.Y - endRoadNode2Geometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNode2Geometry.Point, endRoadNode2Geometry.Point }
                    }
                }
            };

            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = ObjectProvider.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = ObjectProvider.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = ObjectProvider.Create<RoadSegmentStatus>();
            var maintenanceAuthority = ObjectProvider.Create<OrganizationId>();
            var maintenanceAuthorityName = ObjectProvider.Create<OrganizationName>();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                    {
                        Id = 1, Version = 0, Geometry = startRoadNode1Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new ImportedRoadNode
                    {
                        Id = 2, Version = 0, Geometry = endRoadNode1Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        }
                    },
                    new ImportedRoadNode
                    {
                        Id = 3, Version = 0, Geometry = startRoadNode2Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new ImportedRoadNode
                    {
                        Id = 4, Version = 0, Geometry = endRoadNode2Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new ImportedRoadSegment
                    {
                        Id = 1,
                        Geometry = upperRoadSegmentGeometry,
                        Version = 0,
                        GeometryVersion = 0,
                        StartNodeId = 1,
                        EndNodeId = 2,
                        AccessRestriction = roadSegmentAccessRestriction,
                        Category = roadSegmentCategory,
                        GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                        Morphology = roadSegmentMorphology,
                        Status = roadSegmentStatus,
                        PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>(),
                        PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>(),
                        PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>(),
                        Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>(),
                        Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>(),
                        Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>(),
                        LeftSide = new ImportedRoadSegmentSideAttribute(),
                        RightSide = new ImportedRoadSegmentSideAttribute(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = maintenanceAuthority,
                            Name = maintenanceAuthorityName
                        },
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        },
                        RecordingDate = ObjectProvider.Create<DateTime>(),
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }, new ImportedRoadSegment
                    {
                        Id = 2,
                        Geometry = lowerRoadSegmentGeometry,
                        Version = 0,
                        GeometryVersion = 0,
                        StartNodeId = 3,
                        EndNodeId = 4,
                        AccessRestriction = roadSegmentAccessRestriction,
                        Category = roadSegmentCategory,
                        GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                        Morphology = roadSegmentMorphology,
                        Status = roadSegmentStatus,
                        PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>(),
                        PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>(),
                        PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>(),
                        Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>(),
                        Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>(),
                        Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>(),
                        LeftSide = new ImportedRoadSegmentSideAttribute(),
                        RightSide = new ImportedRoadSegmentSideAttribute(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = maintenanceAuthority,
                            Name = maintenanceAuthorityName
                        },
                        Origin = new ImportedOriginProperties
                        {
                            Application = ObjectProvider.Create<string>(),
                            Operator = ObjectProvider.Create<OperatorName>(),
                            Organization = ObjectProvider.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = ObjectProvider.Create<DateTime>(),
                            TransactionId = 0
                        },
                        RecordingDate = ObjectProvider.Create<DateTime>(),
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    })
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveGradeSeparatedJunction = new RemoveGradeSeparatedJunction
                            {
                                Id = 1
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveGradeSeparatedJunction = new RemoveGradeSeparatedJunction
                            {
                                Id = 1
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "GradeSeparatedJunctionNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters = Array.Empty<ProblemParameter>()
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }

    [Fact]
    public Task When_removing_a_non_existent_node()
    {
        return Run(scenario =>
        {
            var roadNodeGeometry = ObjectProvider.Create<RoadNodeGeometry>();
            roadNodeGeometry.SpatialReferenceSystemIdentifier =
                SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveRoadNode = new RemoveRoadNode
                            {
                                Id = 1
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveRoadNode = new RemoveRoadNode
                            {
                                Id = 1
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "RoadNodeNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters = Array.Empty<ProblemParameter>()
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }

    [Fact]
    public Task When_removing_a_non_existent_segment()
    {
        return Run(scenario =>
        {
            var startRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 0.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var roadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.X - endRoadNodeGeometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.Y - endRoadNodeGeometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNodeGeometry.Point, endRoadNodeGeometry.Point }
                    }
                }
            };

            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            var maintenanceAuthority = ObjectProvider.Create<OrganizationId>();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                })
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveRoadSegment = new RemoveRoadSegment
                            {
                                Id = 1
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveRoadSegment = new RemoveRoadSegment
                            {
                                Id = 1
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "RoadSegmentNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters =
                                    [
                                        new ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = "1"
                                        }
                                    ]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }

    [Fact]
    public Task When_removing_a_non_existent_segment_on_a_european_road()
    {
        return Run(scenario =>
        {
            var startRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 0.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var roadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.X - endRoadNodeGeometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.Y - endRoadNodeGeometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNodeGeometry.Point, endRoadNodeGeometry.Point }
                    }
                }
            };

            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = ObjectProvider.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = ObjectProvider.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = ObjectProvider.Create<RoadSegmentStatus>();
            var maintenanceAuthority = ObjectProvider.Create<OrganizationId>();
            var maintenanceAuthorityName = ObjectProvider.Create<OrganizationName>();
            var europeanRoadNumber = ObjectProvider.Create<EuropeanRoadNumber>();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadSegment
                {
                    Id = 1,
                    Geometry = roadSegmentGeometry,
                    Version = 0,
                    GeometryVersion = 0,
                    StartNodeId = 1,
                    EndNodeId = 2,
                    AccessRestriction = roadSegmentAccessRestriction,
                    Category = roadSegmentCategory,
                    GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                    Morphology = roadSegmentMorphology,
                    Status = roadSegmentStatus,
                    PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>(),
                    PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>(),
                    PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>(),
                    Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>(),
                    Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>(),
                    Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>(),
                    LeftSide = new ImportedRoadSegmentSideAttribute(),
                    RightSide = new ImportedRoadSegmentSideAttribute(),
                    MaintenanceAuthority = new MaintenanceAuthority
                    {
                        Code = maintenanceAuthority,
                        Name = maintenanceAuthorityName
                    },
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = ObjectProvider.Create<DateTime>(),
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveRoadSegmentFromEuropeanRoad = new RemoveRoadSegmentFromEuropeanRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = europeanRoadNumber
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveRoadSegmentFromEuropeanRoad = new RemoveRoadSegmentFromEuropeanRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = europeanRoadNumber
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "EuropeanRoadNumberNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "Number",
                                            Value = europeanRoadNumber
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }

    [Fact]
    public Task When_removing_a_non_existent_segment_on_a_national_road()
    {
        return Run(scenario =>
        {
            var startRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 0.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var roadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.X - endRoadNodeGeometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.Y - endRoadNodeGeometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNodeGeometry.Point, endRoadNodeGeometry.Point }
                    }
                }
            };

            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = ObjectProvider.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = ObjectProvider.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = ObjectProvider.Create<RoadSegmentStatus>();
            var maintenanceAuthority = ObjectProvider.Create<OrganizationId>();
            var maintenanceAuthorityName = ObjectProvider.Create<OrganizationName>();
            var nationalRoadNumber = ObjectProvider.Create<NationalRoadNumber>();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadSegment
                {
                    Id = 1,
                    Geometry = roadSegmentGeometry,
                    Version = 0,
                    GeometryVersion = 0,
                    StartNodeId = 1,
                    EndNodeId = 2,
                    AccessRestriction = roadSegmentAccessRestriction,
                    Category = roadSegmentCategory,
                    GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                    Morphology = roadSegmentMorphology,
                    Status = roadSegmentStatus,
                    PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>(),
                    PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>(),
                    PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>(),
                    Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>(),
                    Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>(),
                    Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>(),
                    LeftSide = new ImportedRoadSegmentSideAttribute(),
                    RightSide = new ImportedRoadSegmentSideAttribute(),
                    MaintenanceAuthority = new MaintenanceAuthority
                    {
                        Code = maintenanceAuthority,
                        Name = maintenanceAuthorityName
                    },
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = ObjectProvider.Create<DateTime>(),
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveRoadSegmentFromNationalRoad = new RemoveRoadSegmentFromNationalRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = nationalRoadNumber
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveRoadSegmentFromNationalRoad = new RemoveRoadSegmentFromNationalRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = nationalRoadNumber
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "NationalRoadNumberNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "Number",
                                            Value = nationalRoadNumber
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }

    [Fact]
    public Task When_removing_a_non_existent_segment_on_a_numbered_road()
    {
        return Run(scenario =>
        {
            var startRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 0.0,
                    Y = 0.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endRoadNodeGeometry = new RoadNodeGeometry
            {
                Point = new Point
                {
                    X = 10.0,
                    Y = 10.0
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var roadSegmentGeometry = new RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                MultiLineString = new[]
                {
                    new LineString
                    {
                        Measures = new[]
                        {
                            0.0,
                            Math.Sqrt(
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.X - endRoadNodeGeometry.Point.X)
                                    , 2.0)
                                +
                                Math.Pow(
                                    Math.Abs(startRoadNodeGeometry.Point.Y - endRoadNodeGeometry.Point.Y)
                                    , 2.0))
                        },
                        Points = new[] { startRoadNodeGeometry.Point, endRoadNodeGeometry.Point }
                    }
                }
            };

            var roadNodeType = ObjectProvider.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = ObjectProvider.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = ObjectProvider.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = ObjectProvider.Create<RoadSegmentStatus>();
            var maintenanceAuthority = ObjectProvider.Create<OrganizationId>();
            var maintenanceAuthorityName = ObjectProvider.Create<OrganizationName>();
            var numberedRoadNumber = ObjectProvider.Create<NumberedRoadNumber>();
            return scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadSegment
                {
                    Id = 1,
                    Geometry = roadSegmentGeometry,
                    Version = 0,
                    GeometryVersion = 0,
                    StartNodeId = 1,
                    EndNodeId = 2,
                    AccessRestriction = roadSegmentAccessRestriction,
                    Category = roadSegmentCategory,
                    GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                    Morphology = roadSegmentMorphology,
                    Status = roadSegmentStatus,
                    PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>(),
                    PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>(),
                    PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>(),
                    Lanes = Array.Empty<ImportedRoadSegmentLaneAttribute>(),
                    Widths = Array.Empty<ImportedRoadSegmentWidthAttribute>(),
                    Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttribute>(),
                    LeftSide = new ImportedRoadSegmentSideAttribute(),
                    RightSide = new ImportedRoadSegmentSideAttribute(),
                    MaintenanceAuthority = new MaintenanceAuthority
                    {
                        Code = maintenanceAuthority,
                        Name = maintenanceAuthorityName
                    },
                    Origin = new ImportedOriginProperties
                    {
                        Application = ObjectProvider.Create<string>(),
                        Operator = ObjectProvider.Create<OperatorName>(),
                        Organization = ObjectProvider.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = ObjectProvider.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = ObjectProvider.Create<DateTime>(),
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(
                    TheOperator.ChangesTheRoadNetwork(
                        RequestId,
                        ReasonForChange,
                        ChangedByOperator,
                        ChangedByOrganization, new RequestedChange
                        {
                            RemoveRoadSegmentFromNumberedRoad = new RemoveRoadSegmentFromNumberedRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = numberedRoadNumber
                            }
                        }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            RemoveRoadSegmentFromNumberedRoad = new RemoveRoadSegmentFromNumberedRoad
                            {
                                AttributeId = 1,
                                SegmentGeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                SegmentId = 1,
                                Number = numberedRoadNumber
                            },
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = "NumberedRoadNumberNotFound",
                                    Severity = ProblemSeverity.Error,
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "Number",
                                            Value = numberedRoadNumber
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                });
        });
    }
}
