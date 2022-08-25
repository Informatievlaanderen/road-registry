namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Framework.Testing;
using Xunit;
using Point = RoadRegistry.BackOffice.Messages.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using ProblemParameter = RoadRegistry.BackOffice.Messages.ProblemParameter;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RemoveGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.RemoveGradeSeparatedJunction;
using RemoveRoadNode = RoadRegistry.BackOffice.Messages.RemoveRoadNode;
using RemoveRoadSegment = RoadRegistry.BackOffice.Messages.RemoveRoadSegment;
using RemoveRoadSegmentFromEuropeanRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromEuropeanRoad;
using RemoveRoadSegmentFromNationalRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromNationalRoad;
using RemoveRoadSegmentFromNumberedRoad = RoadRegistry.BackOffice.Messages.RemoveRoadSegmentFromNumberedRoad;

public class RemoveTheNonExisting : RoadRegistryFixture
{
    public RemoveTheNonExisting()
    {
        Fixture.CustomizePoint();
        Fixture.CustomizePolylineM();

        Fixture.CustomizeAttributeId();
        Fixture.CustomizeOrganizationId();
        Fixture.CustomizeOrganizationName();
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeType();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentCategory();
        Fixture.CustomizeRoadSegmentMorphology();
        Fixture.CustomizeRoadSegmentStatus();
        Fixture.CustomizeRoadSegmentAccessRestriction();
        Fixture.CustomizeRoadSegmentLaneCount();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentSurfaceType();
        Fixture.CustomizeRoadSegmentWidth();
        Fixture.CustomizeEuropeanRoadNumber();
        Fixture.CustomizeNationalRoadNumber();
        Fixture.CustomizeNumberedRoadNumber();
        Fixture.CustomizeOriginProperties();
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeGradeSeparatedJunctionType();
        Fixture.CustomizeArchiveId();
        Fixture.CustomizeChangeRequestId();
        Fixture.CustomizeReason();
        Fixture.CustomizeOperatorName();
        Fixture.CustomizeTransactionId();

        Fixture.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Number = Fixture.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        Fixture.Customize<RoadSegmentNationalRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Number = Fixture.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        Fixture.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.Number = Fixture.Create<NumberedRoadNumber>();
                instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = Fixture.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        Fixture.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                instance.AttributeId = Fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());

        ArchiveId = Fixture.Create<ArchiveId>();
        RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
        ReasonForChange = Fixture.Create<Reason>();
        ChangedByOperator = Fixture.Create<OperatorName>();
        ChangedByOrganization = Fixture.Create<OrganizationId>();
        ChangedByOrganizationName = Fixture.Create<OrganizationName>();
    }

    public ArchiveId ArchiveId { get; }
    public ChangeRequestId RequestId { get; }
    public Reason ReasonForChange { get; }
    public OperatorName ChangedByOperator { get; }
    public OrganizationId ChangedByOrganization { get; }
    public OrganizationName ChangedByOrganizationName { get; }

    [Fact]
    public Task When_removing_a_non_existent_node()
    {
        return Run(scenario =>
        {
            var roadNodeGeometry = Fixture.Create<RoadNodeGeometry>();
            roadNodeGeometry.SpatialReferenceSystemIdentifier =
                SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
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

            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
            var maintenanceAuthority = Fixture.Create<OrganizationId>();
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
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

            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = Fixture.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = Fixture.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = Fixture.Create<RoadSegmentStatus>();
            var maintenanceAuthority = Fixture.Create<OrganizationId>();
            var maintenanceAuthorityName = Fixture.Create<OrganizationName>();
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
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new ImportedRoadNode
                    {
                        Id = 2, Version = 0, Geometry = endRoadNode1Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    },
                    new ImportedRoadNode
                    {
                        Id = 3, Version = 0, Geometry = startRoadNode2Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new ImportedRoadNode
                    {
                        Id = 4, Version = 0, Geometry = endRoadNode2Geometry, Type = roadNodeType,
                        Origin = new ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
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
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        },
                        RecordingDate = Fixture.Create<DateTime>(),
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
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        },
                        RecordingDate = Fixture.Create<DateTime>(),
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

            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = Fixture.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = Fixture.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = Fixture.Create<RoadSegmentStatus>();
            var maintenanceAuthority = Fixture.Create<OrganizationId>();
            var maintenanceAuthorityName = Fixture.Create<OrganizationName>();
            var europeanRoadNumber = Fixture.Create<EuropeanRoadNumber>();
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = Fixture.Create<DateTime>(),
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

            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = Fixture.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = Fixture.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = Fixture.Create<RoadSegmentStatus>();
            var maintenanceAuthority = Fixture.Create<OrganizationId>();
            var maintenanceAuthorityName = Fixture.Create<OrganizationName>();
            var nationalRoadNumber = Fixture.Create<NationalRoadNumber>();
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = Fixture.Create<DateTime>(),
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

            var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
            var roadSegmentAccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
            var roadSegmentCategory = Fixture.Create<RoadSegmentCategory>();
            var roadSegmentGeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
            var roadSegmentMorphology = Fixture.Create<RoadSegmentMorphology>();
            var roadSegmentStatus = Fixture.Create<RoadSegmentStatus>();
            var maintenanceAuthority = Fixture.Create<OrganizationId>();
            var maintenanceAuthorityName = Fixture.Create<OrganizationName>();
            var numberedRoadNumber = Fixture.Create<NumberedRoadNumber>();
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    }
                }, new ImportedRoadNode
                {
                    Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                    Origin = new ImportedOriginProperties
                    {
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
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
                        Application = Fixture.Create<string>(),
                        Operator = Fixture.Create<OperatorName>(),
                        Organization = Fixture.Create<OrganizationName>(),
                        OrganizationId = maintenanceAuthority,
                        Since = Fixture.Create<DateTime>(),
                        TransactionId = 0
                    },
                    RecordingDate = Fixture.Create<DateTime>(),
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
