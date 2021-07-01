namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using NodaTime.Text;
    using RoadRegistry.Framework.Testing;
    using Xunit;

    public class ModifyTheNonExisting : RoadRegistryFixture
    {
        public ModifyTheNonExisting()
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

            Fixture.Customize<Messages.RoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.Number = Fixture.Create<EuropeanRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.Number = Fixture.Create<NationalRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Number = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentLaneAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentWidthAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentSurfaceAttribute>(composer =>
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
        public Task when_modifying_a_non_existent_node()
        {
            return Run(scenario =>
            {
                var roadNodeGeometry = Fixture.Create<Messages.RoadNodeGeometry>();
                roadNodeGeometry.SpatialReferenceSystemIdentifier =
                    SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
                var roadNodeType = Fixture.Create<RoadNodeType>().ToString();
                return scenario
                    .Given(Organizations.ToStreamName(ChangedByOrganization),
                        new Messages.ImportedOrganization
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
                            ChangedByOrganization,
                            new Messages.RequestedChange[]
                            {
                                new Messages.RequestedChange
                                {
                                    ModifyRoadNode = new Messages.ModifyRoadNode
                                    {
                                        Id = 1,
                                        Geometry = roadNodeGeometry,
                                        Type = roadNodeType
                                    }
                                }
                            }))
                    .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.RejectedChange
                            {
                                ModifyRoadNode = new Messages.ModifyRoadNode
                                {
                                    Id = 1,
                                    Geometry = roadNodeGeometry,
                                    Type = roadNodeType
                                },
                                Problems = new Messages.Problem[]
                                {
                                    new Messages.Problem
                                    {
                                        Reason = "RoadNodeNotFound",
                                        Severity = Messages.ProblemSeverity.Error,
                                        Parameters = new Messages.ProblemParameter[0]
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
                var startRoadNodeGeometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 0.0,
                        Y = 0.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endRoadNodeGeometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 10.0,
                        Y = 10.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var roadSegmentGeometry = new Messages.RoadSegmentGeometry
                {
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    MultiLineString = new[]
                    {
                        new Messages.LineString
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
                            Points = new[] {startRoadNodeGeometry.Point, endRoadNodeGeometry.Point}
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
                return scenario
                    .Given(Organizations.ToStreamName(ChangedByOrganization),
                        new Messages.ImportedOrganization
                        {
                            Code = ChangedByOrganization,
                            Name = ChangedByOrganizationName,
                            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                        }
                    )
                    .Given(RoadNetworks.Stream, new Messages.ImportedRoadNode
                    {
                        Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                        Origin = new Messages.ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new Messages.ImportedRoadNode
                    {
                        Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                        Origin = new Messages.ImportedOriginProperties
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
                            ChangedByOrganization,
                            new Messages.RequestedChange[]
                            {
                                new Messages.RequestedChange
                                {
                                    ModifyRoadSegment = new Messages.ModifyRoadSegment
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
                                        Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0],
                                        Widths = new Messages.RequestedRoadSegmentWidthAttribute[0],
                                        Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0],
                                        LeftSideStreetNameId = 0,
                                        RightSideStreetNameId = 0,
                                        MaintenanceAuthority = maintenanceAuthority
                                    }
                                }
                            }))
                    .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.RejectedChange
                            {
                                ModifyRoadSegment = new Messages.ModifyRoadSegment
                                {
                                    Id = 1,
                                    Geometry = roadSegmentGeometry,
                                    StartNodeId = 1,
                                    EndNodeId = 2,
                                    AccessRestriction = roadSegmentAccessRestriction,
                                    Category = roadSegmentCategory,
                                    GeometryDrawMethod = roadSegmentGeometryDrawMethod,
                                    Morphology = roadSegmentMorphology,
                                    Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0],
                                    Widths = new Messages.RequestedRoadSegmentWidthAttribute[0],
                                    Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0],
                                    Status = roadSegmentStatus,
                                    LeftSideStreetNameId = 0,
                                    RightSideStreetNameId = 0,
                                    MaintenanceAuthority = maintenanceAuthority
                                },
                                Problems = new Messages.Problem[]
                                {
                                    new Messages.Problem
                                    {
                                        Reason = "RoadSegmentNotFound",
                                        Severity = Messages.ProblemSeverity.Error,
                                        Parameters = new Messages.ProblemParameter[0]
                                    }
                                }
                            }
                        },
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    });
            });
        }

        [Fact]
        public Task when_modifying_a_non_existent_grade_separated_junction()
        {
            return Run(scenario =>
            {
                var startRoadNode1Geometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 0.0,
                        Y = 0.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endRoadNode1Geometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 10.0,
                        Y = 10.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var startRoadNode2Geometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 10.0,
                        Y = 0.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endRoadNode2Geometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 00.0,
                        Y = 10.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var upperRoadSegmentGeometry = new Messages.RoadSegmentGeometry
                {
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    MultiLineString = new[]
                    {
                        new Messages.LineString
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
                            Points = new[] {startRoadNode1Geometry.Point, endRoadNode1Geometry.Point}
                        }
                    }
                };
                var lowerRoadSegmentGeometry = new Messages.RoadSegmentGeometry
                {
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    MultiLineString = new[]
                    {
                        new Messages.LineString
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
                            Points = new[] {startRoadNode2Geometry.Point, endRoadNode2Geometry.Point}
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
                var gradeSeparatedJunctionType = Fixture.Create<GradeSeparatedJunctionType>();
                return scenario
                    .Given(Organizations.ToStreamName(ChangedByOrganization),
                        new Messages.ImportedOrganization
                        {
                            Code = ChangedByOrganization,
                            Name = ChangedByOrganizationName,
                            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                        }
                    )
                    .Given(RoadNetworks.Stream, new Messages.ImportedRoadNode
                        {
                            Id = 1, Version = 0, Geometry = startRoadNode1Geometry, Type = roadNodeType,
                            Origin = new Messages.ImportedOriginProperties
                            {
                                Application = Fixture.Create<string>(),
                                Operator = Fixture.Create<OperatorName>(),
                                Organization = Fixture.Create<OrganizationName>(),
                                OrganizationId = maintenanceAuthority,
                                Since = Fixture.Create<DateTime>(),
                                TransactionId = 0
                            }
                        }, new Messages.ImportedRoadNode
                        {
                            Id = 2, Version = 0, Geometry = endRoadNode1Geometry, Type = roadNodeType,
                            Origin = new Messages.ImportedOriginProperties
                            {
                                Application = Fixture.Create<string>(),
                                Operator = Fixture.Create<OperatorName>(),
                                Organization = Fixture.Create<OrganizationName>(),
                                OrganizationId = maintenanceAuthority,
                                Since = Fixture.Create<DateTime>(),
                                TransactionId = 0
                            }
                        },
                        new Messages.ImportedRoadNode
                        {
                            Id = 3, Version = 0, Geometry = startRoadNode2Geometry, Type = roadNodeType,
                            Origin = new Messages.ImportedOriginProperties
                            {
                                Application = Fixture.Create<string>(),
                                Operator = Fixture.Create<OperatorName>(),
                                Organization = Fixture.Create<OrganizationName>(),
                                OrganizationId = maintenanceAuthority,
                                Since = Fixture.Create<DateTime>(),
                                TransactionId = 0
                            }
                        }, new Messages.ImportedRoadNode
                        {
                            Id = 4, Version = 0, Geometry = endRoadNode2Geometry, Type = roadNodeType,
                            Origin = new Messages.ImportedOriginProperties
                            {
                                Application = Fixture.Create<string>(),
                                Operator = Fixture.Create<OperatorName>(),
                                Organization = Fixture.Create<OrganizationName>(),
                                OrganizationId = maintenanceAuthority,
                                Since = Fixture.Create<DateTime>(),
                                TransactionId = 0
                            }
                        }, new Messages.ImportedRoadSegment
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
                            PartOfEuropeanRoads = new Messages.ImportedRoadSegmentEuropeanRoadAttribute[0],
                            PartOfNationalRoads = new Messages.ImportedRoadSegmentNationalRoadAttribute[0],
                            PartOfNumberedRoads = new Messages.ImportedRoadSegmentNumberedRoadAttribute[0],
                            Lanes = new Messages.ImportedRoadSegmentLaneAttribute[0],
                            Widths = new Messages.ImportedRoadSegmentWidthAttribute[0],
                            Surfaces = new Messages.ImportedRoadSegmentSurfaceAttribute[0],
                            LeftSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            RightSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            MaintenanceAuthority = new Messages.MaintenanceAuthority
                            {
                                Code = maintenanceAuthority,
                                Name = maintenanceAuthorityName
                            },
                            Origin = new Messages.ImportedOriginProperties
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
                        }, new Messages.ImportedRoadSegment
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
                            PartOfEuropeanRoads = new Messages.ImportedRoadSegmentEuropeanRoadAttribute[0],
                            PartOfNationalRoads = new Messages.ImportedRoadSegmentNationalRoadAttribute[0],
                            PartOfNumberedRoads = new Messages.ImportedRoadSegmentNumberedRoadAttribute[0],
                            Lanes = new Messages.ImportedRoadSegmentLaneAttribute[0],
                            Widths = new Messages.ImportedRoadSegmentWidthAttribute[0],
                            Surfaces = new Messages.ImportedRoadSegmentSurfaceAttribute[0],
                            LeftSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            RightSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            MaintenanceAuthority = new Messages.MaintenanceAuthority
                            {
                                Code = maintenanceAuthority,
                                Name = maintenanceAuthorityName
                            },
                            Origin = new Messages.ImportedOriginProperties
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
                            ChangedByOrganization,
                            new Messages.RequestedChange[]
                            {
                                new Messages.RequestedChange
                                {
                                    ModifyGradeSeparatedJunction = new Messages.ModifyGradeSeparatedJunction
                                    {
                                        Id = 1,
                                        Type = gradeSeparatedJunctionType,
                                        UpperSegmentId = 1,
                                        LowerSegmentId = 2
                                    }
                                }
                            }))
                    .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.RejectedChange
                            {
                                ModifyGradeSeparatedJunction = new Messages.ModifyGradeSeparatedJunction
                                {
                                    Id = 1,
                                    Type = gradeSeparatedJunctionType,
                                    UpperSegmentId = 1,
                                    LowerSegmentId = 2
                                },
                                Problems = new Messages.Problem[]
                                {
                                    new Messages.Problem
                                    {
                                        Reason = "GradeSeparatedJunctionNotFound",
                                        Severity = Messages.ProblemSeverity.Error,
                                        Parameters = new Messages.ProblemParameter[0]
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
                var startRoadNodeGeometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 0.0,
                        Y = 0.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endRoadNodeGeometry = new Messages.RoadNodeGeometry
                {
                    Point = new Messages.Point
                    {
                        X = 10.0,
                        Y = 10.0
                    },
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var roadSegmentGeometry = new Messages.RoadSegmentGeometry
                {
                    SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                    MultiLineString = new[]
                    {
                        new Messages.LineString
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
                            Points = new[] {startRoadNodeGeometry.Point, endRoadNodeGeometry.Point}
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
                var roadSegmentNumberedRoadDirection = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                var numberedRoadNumber = Fixture.Create<NumberedRoadNumber>();
                var roadSegmentNumberedRoadOrdinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                return scenario
                    .Given(Organizations.ToStreamName(ChangedByOrganization),
                        new Messages.ImportedOrganization
                        {
                            Code = ChangedByOrganization,
                            Name = ChangedByOrganizationName,
                            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                        }
                    )
                    .Given(RoadNetworks.Stream, new Messages.ImportedRoadNode
                    {
                        Id = 1, Version = 0, Geometry = startRoadNodeGeometry, Type = roadNodeType,
                        Origin = new Messages.ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new Messages.ImportedRoadNode
                    {
                        Id = 2, Version = 0, Geometry = endRoadNodeGeometry, Type = roadNodeType,
                        Origin = new Messages.ImportedOriginProperties
                        {
                            Application = Fixture.Create<string>(),
                            Operator = Fixture.Create<OperatorName>(),
                            Organization = Fixture.Create<OrganizationName>(),
                            OrganizationId = maintenanceAuthority,
                            Since = Fixture.Create<DateTime>(),
                            TransactionId = 0
                        }
                    }, new Messages.ImportedRoadSegment
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
                            PartOfEuropeanRoads = new Messages.ImportedRoadSegmentEuropeanRoadAttribute[0],
                            PartOfNationalRoads = new Messages.ImportedRoadSegmentNationalRoadAttribute[0],
                            PartOfNumberedRoads = new Messages.ImportedRoadSegmentNumberedRoadAttribute[0],
                            Lanes = new Messages.ImportedRoadSegmentLaneAttribute[0],
                            Widths = new Messages.ImportedRoadSegmentWidthAttribute[0],
                            Surfaces = new Messages.ImportedRoadSegmentSurfaceAttribute[0],
                            LeftSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            RightSide = new Messages.ImportedRoadSegmentSideAttribute(),
                            MaintenanceAuthority = new Messages.MaintenanceAuthority
                            {
                                Code = maintenanceAuthority,
                                Name = maintenanceAuthorityName
                            },
                            Origin = new Messages.ImportedOriginProperties
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
                            ChangedByOrganization,
                            new Messages.RequestedChange[]
                            {
                                new Messages.RequestedChange
                                {
                                    ModifyRoadSegmentOnNumberedRoad = new Messages.ModifyRoadSegmentOnNumberedRoad
                                    {
                                        AttributeId = 1,
                                        SegmentId = 1,
                                        Direction = roadSegmentNumberedRoadDirection,
                                        Number = numberedRoadNumber,
                                        Ordinal = roadSegmentNumberedRoadOrdinal
                                    }
                                }
                            }))
                    .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.RejectedChange
                            {
                                ModifyRoadSegmentOnNumberedRoad = new Messages.ModifyRoadSegmentOnNumberedRoad
                                {
                                    AttributeId = 1,
                                    SegmentId = 1,
                                    Direction = roadSegmentNumberedRoadDirection,
                                    Number = numberedRoadNumber,
                                    Ordinal = roadSegmentNumberedRoadOrdinal
                                },
                                Problems = new Messages.Problem[]
                                {
                                    new Messages.Problem
                                    {
                                        Reason = "NumberedRoadNumberNotFound",
                                        Severity = Messages.ProblemSeverity.Error,
                                        Parameters = new Messages.ProblemParameter[]
                                        {
                                            new Messages.ProblemParameter
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
}
