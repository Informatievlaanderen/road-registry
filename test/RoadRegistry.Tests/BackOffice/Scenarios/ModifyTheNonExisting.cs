namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Framework.Testing;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using ModifyGradeSeparatedJunction = RoadRegistry.BackOffice.Messages.ModifyGradeSeparatedJunction;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;
using ModifyRoadSegment = RoadRegistry.BackOffice.Messages.ModifyRoadSegment;
using ModifyRoadSegmentOnNumberedRoad = RoadRegistry.BackOffice.Messages.ModifyRoadSegmentOnNumberedRoad;
using Point = RoadRegistry.BackOffice.Messages.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using ProblemParameter = RoadRegistry.BackOffice.Messages.ProblemParameter;
using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RoadSegmentEuropeanRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentEuropeanRoadAttribute;
using RoadSegmentNationalRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentNationalRoadAttribute;
using RoadSegmentNumberedRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentNumberedRoadAttribute;

public class ModifyTheNonExisting : RoadRegistryTestBase
{
    public ModifyTheNonExisting(ITestOutputHelper testOutputHelper)
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
        ObjectProvider.CustomizeRoadSegmentLaneAttribute();
        ObjectProvider.CustomizeRoadSegmentLaneAttributes();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadDirection();
        ObjectProvider.CustomizeRoadSegmentGeometryDrawMethod();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadOrdinal();
        ObjectProvider.CustomizeRoadSegmentSurfaceAttribute();
        ObjectProvider.CustomizeRoadSegmentSurfaceAttributes();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentWidthAttribute();
        ObjectProvider.CustomizeRoadSegmentWidthAttributes();
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

        ObjectProvider.Customize<RoadSegmentEuropeanRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNationalRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNumberedRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.Number = ObjectProvider.Create<NumberedRoadNumber>();
                instance.Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
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
    public Task when_modifying_a_non_existent_grade_separated_junction()
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
            var gradeSeparatedJunctionType = ObjectProvider.Create<GradeSeparatedJunctionType>();
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
                            ModifyGradeSeparatedJunction = new ModifyGradeSeparatedJunction
                            {
                                Id = 1,
                                Type = gradeSeparatedJunctionType,
                                UpperSegmentId = 1,
                                LowerSegmentId = 2
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
                            ModifyGradeSeparatedJunction = new ModifyGradeSeparatedJunction
                            {
                                Id = 1,
                                Type = gradeSeparatedJunctionType,
                                UpperSegmentId = 1,
                                LowerSegmentId = 2
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
    public Task when_modifying_a_non_existent_node()
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
                            ModifyRoadNode = new ModifyRoadNode
                            {
                                Id = 1,
                                Geometry = roadNodeGeometry,
                                Type = roadNodeType
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
                            ModifyRoadNode = new ModifyRoadNode
                            {
                                Id = 1,
                                Geometry = roadNodeGeometry,
                                Type = roadNodeType
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
    public Task when_modifying_a_non_existent_numbered_road()
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
            var roadSegmentNumberedRoadDirection = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
            var numberedRoadNumber = ObjectProvider.Create<NumberedRoadNumber>();
            var roadSegmentNumberedRoadOrdinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
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
                            ModifyRoadSegmentOnNumberedRoad = new ModifyRoadSegmentOnNumberedRoad
                            {
                                AttributeId = 1,
                                SegmentId = 1,
                                Direction = roadSegmentNumberedRoadDirection,
                                Number = numberedRoadNumber,
                                Ordinal = roadSegmentNumberedRoadOrdinal
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
                            ModifyRoadSegmentOnNumberedRoad = new ModifyRoadSegmentOnNumberedRoad
                            {
                                AttributeId = 1,
                                SegmentId = 1,
                                Direction = roadSegmentNumberedRoadDirection,
                                Number = numberedRoadNumber,
                                Ordinal = roadSegmentNumberedRoadOrdinal
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

    [Fact]
    public Task when_modifying_a_non_existent_segment()
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
            var lanes = new[]{ ObjectProvider.CreateRequestedRoadSegmentLaneAttribute(GeometryTranslator.Translate(roadSegmentGeometry).Length) };
            var widths = new[] { ObjectProvider.CreateRequestedRoadSegmentWidthAttribute(GeometryTranslator.Translate(roadSegmentGeometry).Length) };
            var surfaces = new[] { ObjectProvider.CreateRequestedRoadSegmentSurfaceAttribute(GeometryTranslator.Translate(roadSegmentGeometry).Length) };

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
                            ModifyRoadSegment = new ModifyRoadSegment
                            {
                                Id = 1,
                                Geometry = roadSegmentGeometry,
                                StartNodeId = 1,
                                EndNodeId = 2,
                                AccessRestriction = roadSegmentAccessRestriction,
                                Category = roadSegmentCategory,
                                GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                Morphology = roadSegmentMorphology,
                                Status = roadSegmentStatus,
                                Lanes = lanes,
                                Widths = widths,
                                Surfaces = surfaces,
                                LeftSideStreetNameId = 0,
                                RightSideStreetNameId = 0,
                                MaintenanceAuthority = maintenanceAuthority
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
                            ModifyRoadSegment = new ModifyRoadSegment
                            {
                                Id = 1,
                                Geometry = roadSegmentGeometry,
                                StartNodeId = 1,
                                EndNodeId = 2,
                                AccessRestriction = roadSegmentAccessRestriction,
                                Category = roadSegmentCategory,
                                GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                Morphology = roadSegmentMorphology,
                                Lanes = lanes,
                                Widths = widths,
                                Surfaces = surfaces,
                                Status = roadSegmentStatus,
                                LeftSideStreetNameId = 0,
                                RightSideStreetNameId = 0,
                                MaintenanceAuthority = maintenanceAuthority
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
}
