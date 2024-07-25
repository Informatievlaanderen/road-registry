namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Datadog.Trace.Ci;
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
using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class ModifyRoadSegmentScenarios : RoadNetworkTestBase
{
    public ModifyRoadSegmentScenarios(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
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
                                            Name = "ModifiedRoadSegmentId",
                                            Value = modifyRoadSegment.Id.ToString()
                                        },
                                        new ProblemParameter
                                        {
                                            Name = "IntersectingRoadSegmentId",
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

    [Fact]
    public Task GivenUpgradedCategory_WhenCategoryNotModified_ThenIgnoreNewValueAndGiveWarning()
    {
        var upgradedCategory = RoadSegmentCategory.EuropeanMainRoad;
        var notUpgradedCategory = RoadSegmentCategory.Unknown;

        var initial = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(TestData.StartNode1Added)
            .WithRoadNodeAdded(TestData.EndNode1Added)
            .WithRoadSegmentAdded(TestData.Segment1Added, segment =>
            {
                segment.Category = upgradedCategory;
            })
            .Build();

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithModifyRoadSegment(TestData.ModifySegment1, segment =>
            {
                segment.Category = notUpgradedCategory;
                segment.CategoryModified = false;
            })
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentModified(TestData.Segment1Modified,
                segment =>
                {
                    segment.Category = upgradedCategory;
                },
                [
                    new Problem
                    {
                        Severity = ProblemSeverity.Warning,
                        Reason = "RoadSegmentCategoryNotChangedBecauseAlreadyIsNewerVersion",
                        Parameters =
                        [
                            new ProblemParameter
                            {
                                Name = "Identifier",
                                Value = "1"
                            }
                        ]
                    }
                ])
            .Build();

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, initial)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public Task GivenUpgradedCategory_WhenCategoryNotModifiedAndSegmentIsOutlined_ThenKeepNewValue()
    {
        var testData = new RoadNetworkTestData(fixture =>
        {
            fixture.CustomizeRoadSegmentOutlineStatus();
            fixture.CustomizeRoadSegmentOutlineMorphology();
            fixture.CustomizeRoadSegmentOutlineLaneCount();
            fixture.CustomizeRoadSegmentOutlineWidth();
        });

        var upgradedCategory = RoadSegmentCategory.EuropeanMainRoad;
        var notUpgradedCategory = RoadSegmentCategory.Unknown;

        var initial = new RoadNetworkChangesAcceptedBuilder(testData)
            .WithOutlinedRoadSegment(testData.Segment1Added, segment =>
            {
                segment.Category = upgradedCategory;
            })
            .Build();

        var command = new ChangeRoadNetworkBuilder(testData)
            .WithModifyOutlinedRoadSegment(testData.ModifySegment1, segment =>
            {
                segment.Category = notUpgradedCategory;
                segment.CategoryModified = false;
            })
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(testData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithOutlinedRoadSegmentModified(testData.Segment1Modified,
                segment =>
                {
                    segment.Category = notUpgradedCategory;
                })
            .Build();

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(testData.ChangedByOrganization), testData.ChangedByImportedOrganization)
                .Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(testData.Segment1Added.Id)), initial)
                .When(command)
                .Then(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(testData.Segment1Added.Id)), expected)
        );
    }

    [Fact]
    public Task GivenUpgradedCategory_WhenCategoryModified_ThenKeepNewValue()
    {
        var upgradedCategory = RoadSegmentCategory.EuropeanMainRoad;
        var category = ObjectProvider.Create<RoadSegmentCategory>();

        var initial = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(TestData.StartNode1Added)
            .WithRoadNodeAdded(TestData.EndNode1Added)
            .WithRoadSegmentAdded(TestData.Segment1Added, segment =>
            {
                segment.Category = upgradedCategory;
            })
            .Build();

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithModifyRoadSegment(TestData.ModifySegment1, segment =>
            {
                segment.Category = category;
                segment.CategoryModified = true;
            })
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentModified(TestData.Segment1Modified,
                segment =>
                {
                    segment.Category = category;
                })
            .Build();

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, initial)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
