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
using Xunit;
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

public class ModifyRoadSegmentScenarios : RoadRegistryFixture
{
    public ModifyRoadSegmentScenarios()
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
    public OperatorName ChangedByOperator { get; }
    public OrganizationId ChangedByOrganization { get; }
    public OrganizationName ChangedByOrganizationName { get; }

    [Fact]
    public Task modify_segment_that_intersects_without_grade_separated_junction()
    {
        var pointA = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeA = Fixture.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = Fixture.Create<RoadNodeId>();
        var pointC = new Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeC = Fixture.Create<RoadNodeId>();
        var pointD = new Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeD = Fixture.Create<RoadNodeId>();
        var pointCModified = new Point(new CoordinateM(5.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var pointDModified = new Point(new CoordinateM(5.0, -10.0, 20.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var segment1 = Fixture.Create<RoadSegmentId>();
        var segment2 = Fixture.Create<RoadSegmentId>();
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
            AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
            Category = Fixture.Create<RoadSegmentCategory>(),
            Morphology = Fixture.Create<RoadSegmentMorphology>(),
            Status = Fixture.Create<RoadSegmentStatus>(),
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            LeftSideStreetNameId = Fixture.Create<CrabStreetnameId?>(),
            RightSideStreetNameId = Fixture.Create<CrabStreetnameId?>(),
            MaintenanceAuthority = ChangedByOrganization,
            Lanes = Fixture
                .CreateMany<RequestedRoadSegmentLaneAttribute>(count)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);

                    return part;
                })
                .ToArray(),
            Widths = Fixture
                .CreateMany<RequestedRoadSegmentWidthAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);

                    return part;
                })
                .ToArray(),
            Surfaces = Fixture
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(line2After.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(line2After.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(line2After.Length) / count);

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
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointA),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = nodeB,
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointB),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = segment1,
                                TemporaryId = Fixture.Create<RoadSegmentId>(),
                                Version = Fixture.Create<int>(),
                                StartNodeId = nodeA,
                                EndNodeId = nodeB,
                                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                                Category = Fixture.Create<RoadSegmentCategory>(),
                                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                                Status = Fixture.Create<RoadSegmentStatus>(),
                                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                                Geometry = GeometryTranslator.Translate(line1),
                                GeometryVersion = Fixture.Create<GeometryVersion>(),
                                MaintenanceAuthority = new MaintenanceAuthority
                                {
                                    Code = Fixture.Create<OrganizationId>(),
                                    Name = Fixture.Create<OrganizationName>()
                                },
                                LeftSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                RightSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                Lanes = Fixture
                                    .CreateMany<RoadSegmentLaneAttributes>(count)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line1.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Widths = Fixture
                                    .CreateMany<RoadSegmentWidthAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line1.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Surfaces = Fixture
                                    .CreateMany<RoadSegmentSurfaceAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line1.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line1.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line1.Length) / count);

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
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointC),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = nodeD,
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointD),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = segment2,
                                TemporaryId = Fixture.Create<RoadSegmentId>(),
                                Version = Fixture.Create<int>(),
                                StartNodeId = nodeC,
                                EndNodeId = nodeD,
                                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                                Category = Fixture.Create<RoadSegmentCategory>(),
                                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                                Status = Fixture.Create<RoadSegmentStatus>(),
                                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                                Geometry = GeometryTranslator.Translate(line1),
                                GeometryVersion = Fixture.Create<GeometryVersion>(),
                                MaintenanceAuthority = new MaintenanceAuthority
                                {
                                    Code = Fixture.Create<OrganizationId>(),
                                    Name = Fixture.Create<OrganizationName>()
                                },
                                LeftSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                RightSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                Lanes = Fixture
                                    .CreateMany<RoadSegmentLaneAttributes>(count)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Widths = Fixture
                                    .CreateMany<RoadSegmentWidthAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Surfaces = Fixture
                                    .CreateMany<RoadSegmentSurfaceAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(line2Before.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(line2Before.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(line2Before.Length) / count);

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
        var nodeA = Fixture.Create<RoadNodeId>();
        var pointB = new Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeB = Fixture.Create<RoadNodeId>();
        var pointC = new Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeC = Fixture.Create<RoadNodeId>();
        var pointD = new Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var nodeD = Fixture.Create<RoadNodeId>();
        var segment = Fixture.Create<RoadSegmentId>();
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
            AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
            Category = Fixture.Create<RoadSegmentCategory>(),
            Morphology = Fixture.Create<RoadSegmentMorphology>(),
            Status = Fixture.Create<RoadSegmentStatus>(),
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            LeftSideStreetNameId = Fixture.Create<CrabStreetnameId?>(),
            RightSideStreetNameId = Fixture.Create<CrabStreetnameId?>(),
            MaintenanceAuthority = ChangedByOrganization,
            Lanes = Fixture
                .CreateMany<RequestedRoadSegmentLaneAttribute>(count)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

                    return part;
                })
                .ToArray(),
            Widths = Fixture
                .CreateMany<RequestedRoadSegmentWidthAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

                    return part;
                })
                .ToArray(),
            Surfaces = Fixture
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                    if (index == count - 1)
                        part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

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
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointA),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = nodeB,
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Geometry = GeometryTranslator.Translate(pointB),
                                Type = RoadNodeType.EndNode
                            },
                            Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = segment,
                                TemporaryId = Fixture.Create<RoadSegmentId>(),
                                Version = Fixture.Create<int>(),
                                StartNodeId = nodeA,
                                EndNodeId = nodeB,
                                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                                Category = Fixture.Create<RoadSegmentCategory>(),
                                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                                Status = Fixture.Create<RoadSegmentStatus>(),
                                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                                Geometry = GeometryTranslator.Translate(lineBefore),
                                GeometryVersion = Fixture.Create<GeometryVersion>(),
                                MaintenanceAuthority = new MaintenanceAuthority
                                {
                                    Code = Fixture.Create<OrganizationId>(),
                                    Name = Fixture.Create<OrganizationName>()
                                },
                                LeftSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                RightSide = new RoadSegmentSideAttributes
                                {
                                    StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                },
                                Lanes = Fixture
                                    .CreateMany<RoadSegmentLaneAttributes>(count)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Widths = Fixture
                                    .CreateMany<RoadSegmentWidthAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

                                        return part;
                                    })
                                    .ToArray(),
                                Surfaces = Fixture
                                    .CreateMany<RoadSegmentSurfaceAttributes>(3)
                                    .Select((part, index) =>
                                    {
                                        part.FromPosition = index * (Convert.ToDecimal(lineBefore.Length) / count);
                                        if (index == count - 1)
                                            part.ToPosition = Convert.ToDecimal(lineBefore.Length);
                                        else
                                            part.ToPosition = (index + 1) * (Convert.ToDecimal(lineBefore.Length) / count);

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

    public Reason ReasonForChange { get; }
    public ChangeRequestId RequestId { get; }
}