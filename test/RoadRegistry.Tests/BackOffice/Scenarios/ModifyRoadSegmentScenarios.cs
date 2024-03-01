namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Framework.Testing;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;
using AddRoadNode = RoadRegistry.BackOffice.Messages.AddRoadNode;
using GeometryTranslator = RoadRegistry.BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;
using ModifyRoadSegment = RoadRegistry.BackOffice.Messages.ModifyRoadSegment;
using Point = NetTopologySuite.Geometries.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using ProblemParameter = RoadRegistry.BackOffice.Messages.ProblemParameter;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class ModifyRoadSegmentScenarios : RoadRegistryTestBase
{
    public ModifyRoadSegmentScenarios(ITestOutputHelper testOutputHelper)
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

        ObjectProvider.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNationalRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
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
    public Task modify_segment_that_intersects_without_grade_separated_junction()
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeA = ObjectProvider.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = ObjectProvider.Create<RoadNodeId>();
        var pointC = new Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeC = ObjectProvider.Create<RoadNodeId>();
        var pointD = new Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeD = ObjectProvider.Create<RoadNodeId>();
        var pointCModified = new Point(new CoordinateM(5.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var pointDModified = new Point(new CoordinateM(5.0, -10.0, 20.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var segment1 = ObjectProvider.Create<RoadSegmentId>();
        var segment2 = ObjectProvider.Create<RoadSegmentId>();
        var line1 = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var line2Before = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { pointC.Coordinate, pointD.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var line2After = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { pointCModified.Coordinate, pointDModified.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        var count = 3;

        var modifyRoadSegment = new ModifyRoadSegment
        {
            Id = segment2,
            StartNodeId = nodeC,
            EndNodeId = nodeD,
            Geometry = GeometryTranslator.Translate(line2After),
            AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
            Category = ObjectProvider.Create<RoadSegmentCategory>(),
            Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
            Status = ObjectProvider.Create<RoadSegmentStatus>(),
            GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
            LeftSideStreetNameId = ObjectProvider.Create<StreetNameLocalId?>(),
            RightSideStreetNameId = ObjectProvider.Create<StreetNameLocalId?>(),
            MaintenanceAuthority = ChangedByOrganization,
            Lanes = ObjectProvider
                .CreateMany<RequestedRoadSegmentLaneAttribute>(count)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);
                    }

                    part.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                    part.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();

                    return part;
                })
                .ToArray(),
            Widths = ObjectProvider
                .CreateMany<RequestedRoadSegmentWidthAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);
                    }

                    part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                    return part;
                })
                .ToArray(),
            Surfaces = ObjectProvider
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);
                    }

                    part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                    return part;
                })
                .ToArray()
        };

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
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
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = nodeC,
                                TemporaryId = ObjectProvider.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointC),
                                Type = RoadNodeType.EndNode,
                                Version = 1
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = nodeD,
                                TemporaryId = ObjectProvider.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointD),
                                Type = RoadNodeType.EndNode,
                                Version = 1
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = segment2,
                                TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
                                Version = ObjectProvider.Create<int>(),
                                StartNodeId = nodeC,
                                EndNodeId = nodeD,
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
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);
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
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);
                                        }

                                        part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                                        return part;
                                    })
                                    .ToArray(),
                                Surfaces = ObjectProvider
                                    .CreateMany<RoadSegmentSurfaceAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);
                                        }

                                        part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                                        return part;
                                    })
                                    .ToArray()
                            },
                            Problems = Array.Empty<Problem>()
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = new ModifyRoadNode
                        {
                            Id = nodeC,
                            Geometry = GeometryTranslator.Translate(pointCModified),
                            Type = RoadNodeType.EndNode
                        }
                    },
                    new RequestedChange
                    {
                        ModifyRoadNode = new ModifyRoadNode
                        {
                            Id = nodeD,
                            Geometry = GeometryTranslator.Translate(pointDModified),
                            Type = RoadNodeType.EndNode
                        }
                    },
                    new RequestedChange
                    {
                        ModifyRoadSegment = modifyRoadSegment
                    }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            ModifyRoadSegment = modifyRoadSegment,
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = nameof(IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction),
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "modifiedRoadSegmentId",
                                            Value = modifyRoadSegment.Id.ToString()
                                        },
                                        new ProblemParameter
                                        {
                                            Name = "intersectingRoadSegmentId",
                                            Value = segment1.ToInt32().ToString()
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
    public Task move_segment()
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeA = ObjectProvider.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = ObjectProvider.Create<RoadNodeId>();
        var pointC = new Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeC = ObjectProvider.Create<RoadNodeId>();
        var pointD = new Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeD = ObjectProvider.Create<RoadNodeId>();
        var segment = ObjectProvider.Create<RoadSegmentId>();
        var lineBefore = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { pointA.Coordinate, pointB.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var lineAfter = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { pointC.Coordinate, pointD.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        var count = 3;

        var modifyRoadSegment = new ModifyRoadSegment
        {
            Id = segment,
            StartNodeId = nodeC,
            EndNodeId = nodeD,
            Geometry = GeometryTranslator.Translate(lineAfter),
            AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
            Category = ObjectProvider.Create<RoadSegmentCategory>(),
            Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
            Status = ObjectProvider.Create<RoadSegmentStatus>(),
            GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
            LeftSideStreetNameId = ObjectProvider.Create<StreetNameLocalId?>(),
            RightSideStreetNameId = ObjectProvider.Create<StreetNameLocalId?>(),
            MaintenanceAuthority = ChangedByOrganization,
            Lanes = ObjectProvider
                .CreateMany<RequestedRoadSegmentLaneAttribute>(count)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
                    }

                    part.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                    part.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();

                    return part;
                })
                .ToArray(),
            Widths = ObjectProvider
                .CreateMany<RequestedRoadSegmentWidthAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
                    }

                    part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                    return part;
                })
                .ToArray(),
            Surfaces = ObjectProvider
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                    {
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    }
                    else
                    {
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
                    }

                    part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                    return part;
                })
                .ToArray()
        };

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
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
                                Id = segment,
                                TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
                                Version = ObjectProvider.Create<int>(),
                                StartNodeId = nodeA,
                                EndNodeId = nodeB,
                                AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
                                Category = ObjectProvider.Create<RoadSegmentCategory>(),
                                Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
                                Status = ObjectProvider.Create<RoadSegmentStatus>(),
                                GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
                                Geometry = GeometryTranslator.Translate(lineBefore),
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
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
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
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
                                        }

                                        part.Width = ObjectProvider.Create<RoadSegmentWidth>();

                                        return part;
                                    })
                                    .ToArray(),
                                Surfaces = ObjectProvider
                                    .CreateMany<RoadSegmentSurfaceAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                        {
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        }
                                        else
                                        {
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);
                                        }

                                        part.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();

                                        return part;
                                    })
                                    .ToArray()
                            },
                            Problems = Array.Empty<Problem>()
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = new AddRoadNode
                        {
                            TemporaryId = nodeC,
                            Geometry = GeometryTranslator.Translate(pointC),
                            Type = RoadNodeType.EndNode
                        }
                    },
                    new RequestedChange
                    {
                        AddRoadNode = new AddRoadNode
                        {
                            TemporaryId = nodeD,
                            Geometry = GeometryTranslator.Translate(pointD),
                            Type = RoadNodeType.EndNode
                        }
                    },
                    new RequestedChange
                    {
                        ModifyRoadSegment = modifyRoadSegment
                    }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            ModifyRoadSegment = modifyRoadSegment,
                            Problems = new[]
                            {
                                new Problem
                                {
                                    Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = nodeA.ToInt32().ToString()
                                        }
                                    }
                                },
                                new Problem
                                {
                                    Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                    Parameters = new[]
                                    {
                                        new ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = nodeB.ToInt32().ToString()
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
}