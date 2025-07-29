namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenModifyingRoadSegment;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Newtonsoft.Json;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Tests.Framework.Testing;
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
using RemoveOutlinedRoadSegment = RoadRegistry.BackOffice.Messages.RemoveOutlinedRoadSegment;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class ModifyRoadSegmentScenarios : RoadNetworkTestBase
{
    public ModifyRoadSegmentScenarios(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task WhenModifyRoadSegment_ThenRoadSegmentModified()
    {
        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAcceptedBuilder(TestData)
                    .WithRoadNodeAdded(TestData.StartNode1Added)
                    .WithRoadNodeAdded(TestData.EndNode1Added)
                    .WithRoadSegmentAdded(TestData.Segment1Added)
                    .Build()
                )
                .When(new ChangeRoadNetworkBuilder(TestData)
                    .WithModifyRoadSegment(TestData.ModifySegment1)
                    .Build()
                )
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAcceptedBuilder(TestData)
                    .WithTransactionId(new TransactionId(2))
                    .WithClock(Clock)
                    .WithRoadSegmentModified(TestData.Segment1Modified)
                    .Build())
        );
    }

    [Fact]
    public Task WhenChangingOnlyOneAttribute_ThenEventContainsAllAttributes()
    {
        var modifyRoadSegment = new ModifyRoadSegment
        {
            Id = TestData.ModifySegment1.Id,
            GeometryDrawMethod = TestData.ModifySegment1.GeometryDrawMethod,
            Category = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentCategory.Parse(TestData.Segment1Added.Category))
        };

        return Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAcceptedBuilder(TestData)
                    .WithRoadNodeAdded(TestData.StartNode1Added)
                    .WithRoadNodeAdded(TestData.EndNode1Added)
                    .WithRoadSegmentAdded(TestData.Segment1Added)
                    .Build()
                )
                .When(new ChangeRoadNetworkBuilder(TestData)
                    .WithModifyRoadSegment(modifyRoadSegment)
                    .Build()
                )
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAcceptedBuilder(TestData)
                    .WithTransactionId(new TransactionId(2))
                    .WithClock(Clock)
                    .WithRoadSegmentModified(new RoadSegmentModified
                    {
                        Id = TestData.Segment1Added.Id,
                        OriginalId = TestData.Segment1Added.OriginalId,
                        GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
                        Version = TestData.Segment1Added.Version + 1,
                        GeometryVersion = TestData.Segment1Added.GeometryVersion,
                        Geometry = TestData.Segment1Added.Geometry,
                        StartNodeId = TestData.Segment1Added.StartNodeId,
                        EndNodeId = TestData.Segment1Added.EndNodeId,
                        Category = modifyRoadSegment.Category,
                        AccessRestriction = TestData.Segment1Added.AccessRestriction,
                        Morphology = TestData.Segment1Added.Morphology,
                        MaintenanceAuthority = TestData.Segment1Added.MaintenanceAuthority,
                        Status = TestData.Segment1Added.Status,
                        LeftSide = TestData.Segment1Added.LeftSide,
                        RightSide = TestData.Segment1Added.RightSide,
                        Lanes = TestData.Segment1Added.Lanes,
                        Surfaces = TestData.Segment1Added.Surfaces,
                        Widths = TestData.Segment1Added.Widths
                    })
                    .Build())
        );
    }

    [Fact]
    public Task when_converting_an_outlined_roadsegment_to_measured()
    {
        var geometry = GeometryTranslator.Translate(TestData.AddSegment1.Geometry);

        var addOutlinedSegment = TestData.AddSegment1;
        addOutlinedSegment.PermanentId = 1;
        addOutlinedSegment.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        addOutlinedSegment.Status = ObjectProvider.CreateUntil<RoadSegmentStatus>(x => x.IsValidForEdit());
        addOutlinedSegment.Morphology = ObjectProvider.CreateUntil<RoadSegmentMorphology>(x => x.IsValidForEdit());
        addOutlinedSegment.StartNodeId = 0;
        addOutlinedSegment.EndNodeId = 0;
        addOutlinedSegment.Lanes = addOutlinedSegment.Lanes.Take(1).ToArray();
        addOutlinedSegment.Lanes[0].AttributeId = 1;
        addOutlinedSegment.Lanes[0].FromPosition = 0;
        addOutlinedSegment.Lanes[0].ToPosition = RoadSegmentPosition.FromDouble(geometry.Length);
        addOutlinedSegment.Lanes[0].Count = ObjectProvider.CreateUntil<RoadSegmentLaneCount>(x => x.IsValidForEdit());
        addOutlinedSegment.Surfaces = addOutlinedSegment.Surfaces.Take(1).ToArray();
        addOutlinedSegment.Surfaces[0].AttributeId = 1;
        addOutlinedSegment.Surfaces[0].FromPosition = addOutlinedSegment.Lanes[0].FromPosition;
        addOutlinedSegment.Surfaces[0].ToPosition = addOutlinedSegment.Lanes[0].ToPosition;
        addOutlinedSegment.Widths = addOutlinedSegment.Widths.Take(1).ToArray();
        addOutlinedSegment.Widths[0].AttributeId = 1;
        addOutlinedSegment.Widths[0].FromPosition = addOutlinedSegment.Lanes[0].FromPosition;
        addOutlinedSegment.Widths[0].ToPosition = addOutlinedSegment.Lanes[0].ToPosition;
        addOutlinedSegment.Widths[0].Width = ObjectProvider.CreateUntil<RoadSegmentWidth>(x => x.IsValidForEdit());

        var outlinedSegmentAdded = TestData.Segment1Added;
        outlinedSegmentAdded.Version = 1;
        outlinedSegmentAdded.GeometryVersion = 1;
        outlinedSegmentAdded.GeometryDrawMethod = addOutlinedSegment.GeometryDrawMethod;
        outlinedSegmentAdded.Status = addOutlinedSegment.Status;
        outlinedSegmentAdded.Morphology = addOutlinedSegment.Morphology;
        outlinedSegmentAdded.StartNodeId = addOutlinedSegment.StartNodeId;
        outlinedSegmentAdded.EndNodeId = addOutlinedSegment.EndNodeId;
        outlinedSegmentAdded.Lanes = outlinedSegmentAdded.Lanes.Take(1).ToArray();
        outlinedSegmentAdded.Lanes[0].AttributeId = addOutlinedSegment.Lanes[0].AttributeId;
        outlinedSegmentAdded.Lanes[0].FromPosition = addOutlinedSegment.Lanes[0].FromPosition;
        outlinedSegmentAdded.Lanes[0].ToPosition = addOutlinedSegment.Lanes[0].ToPosition;
        outlinedSegmentAdded.Lanes[0].Count = addOutlinedSegment.Lanes[0].Count;
        outlinedSegmentAdded.Surfaces = outlinedSegmentAdded.Surfaces.Take(1).ToArray();
        outlinedSegmentAdded.Surfaces[0].AttributeId = addOutlinedSegment.Surfaces[0].AttributeId;
        outlinedSegmentAdded.Surfaces[0].FromPosition = addOutlinedSegment.Surfaces[0].FromPosition;
        outlinedSegmentAdded.Surfaces[0].ToPosition = addOutlinedSegment.Surfaces[0].ToPosition;
        outlinedSegmentAdded.Widths = outlinedSegmentAdded.Widths.Take(1).ToArray();
        outlinedSegmentAdded.Widths[0].AttributeId = addOutlinedSegment.Widths[0].AttributeId;
        outlinedSegmentAdded.Widths[0].FromPosition = addOutlinedSegment.Widths[0].FromPosition;
        outlinedSegmentAdded.Widths[0].ToPosition = addOutlinedSegment.Widths[0].ToPosition;
        outlinedSegmentAdded.Widths[0].Width = addOutlinedSegment.Widths[0].Width;

        var moveEndPointX = 10;
        geometry = new LineString([
            new(geometry.GetSingleLineString().StartPoint.X, geometry.GetSingleLineString().StartPoint.Y),
            new(geometry.GetSingleLineString().EndPoint.X + moveEndPointX, geometry.GetSingleLineString().EndPoint.Y)
        ]).ToMultiLineString();
        TestData.AddEndNode1.Geometry.Point.X += moveEndPointX;

        var outlinedSegmentModified = JsonConvert.DeserializeObject<RoadSegmentModified>(JsonConvert.SerializeObject(TestData.Segment1Modified));
        outlinedSegmentModified.ConvertedFromOutlined = false;
        outlinedSegmentModified.Geometry = GeometryTranslator.Translate(geometry);
        outlinedSegmentModified.Version = 2;
        outlinedSegmentModified.GeometryVersion = 2;
        outlinedSegmentModified.Lanes =
        [
            new RoadSegmentLaneAttributes
            {
                AttributeId = TestData.ModifySegment1.Lanes![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Count = TestData.ModifySegment1.Lanes[0].Count,
                Direction = TestData.ModifySegment1.Lanes[0].Direction,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];
        outlinedSegmentModified.Surfaces =
        [
            new RoadSegmentSurfaceAttributes
            {
                AttributeId = TestData.ModifySegment1.Surfaces![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Type = TestData.ModifySegment1.Surfaces[0].Type,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];
        outlinedSegmentModified.Widths =
        [
            new RoadSegmentWidthAttributes
            {
                AttributeId = TestData.ModifySegment1.Widths![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Width = TestData.ModifySegment1.Widths[0].Width,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];

        var modifySegmentToMeasured = TestData.ModifySegment1;
        modifySegmentToMeasured.ConvertedFromOutlined = true;
        modifySegmentToMeasured.Geometry = GeometryTranslator.Translate(geometry);
        modifySegmentToMeasured.Lanes =
        [
            new RequestedRoadSegmentLaneAttribute
            {
                AttributeId = 2,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Count = outlinedSegmentAdded.Lanes[0].Count,
                Direction = outlinedSegmentAdded.Lanes[0].Direction
            }
        ];
        modifySegmentToMeasured.Surfaces =
        [
            new RequestedRoadSegmentSurfaceAttribute
            {
                AttributeId = 2,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Type = outlinedSegmentAdded.Surfaces[0].Type
            }
        ];
        modifySegmentToMeasured.Widths =
        [
            new RequestedRoadSegmentWidthAttribute
            {
                AttributeId = 2,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Width = outlinedSegmentAdded.Widths[0].Width
            }
        ];

        var segmentToMeasuredModified = JsonConvert.DeserializeObject<RoadSegmentModified>(JsonConvert.SerializeObject(TestData.Segment1Modified));
        segmentToMeasuredModified.ConvertedFromOutlined = true;
        segmentToMeasuredModified.Geometry = GeometryTranslator.Translate(geometry);
        segmentToMeasuredModified.Version = 3;
        segmentToMeasuredModified.GeometryVersion = 2;
        segmentToMeasuredModified.Lanes =
        [
            new RoadSegmentLaneAttributes
            {
                AttributeId = modifySegmentToMeasured.Lanes![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Count = modifySegmentToMeasured.Lanes![0].Count,
                Direction = modifySegmentToMeasured.Lanes![0].Direction,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];
        segmentToMeasuredModified.Surfaces =
        [
            new RoadSegmentSurfaceAttributes
            {
                AttributeId = modifySegmentToMeasured.Surfaces![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Type = modifySegmentToMeasured.Surfaces![0].Type,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];
        segmentToMeasuredModified.Widths =
        [
            new RoadSegmentWidthAttributes
            {
                AttributeId = modifySegmentToMeasured.Widths![0].AttributeId,
                FromPosition = 0,
                ToPosition = (decimal)geometry.Length,
                Width = modifySegmentToMeasured.Widths![0].Width,
                AsOfGeometryVersion = GeometryVersion.Initial
            }
        ];

        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
            .Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(outlinedSegmentAdded.Id)),
                new RoadNetworkChangesAccepted
                {
                    RequestId = TestData.RequestId,
                    Reason = TestData.ReasonForChange,
                    Operator = TestData.ChangedByOperator,
                    OrganizationId = TestData.ChangedByOrganization,
                    Organization = TestData.ChangedByOrganizationName,
                    Changes =
                    [
                        new AcceptedChange
                        {
                            RoadSegmentAdded = outlinedSegmentAdded
                        },
                        new AcceptedChange
                        {
                            RoadSegmentModified = outlinedSegmentModified
                        }
                    ],
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .When(new ChangeRoadNetworkBuilder(TestData)
                .WithAddRoadNode(TestData.AddStartNode1)
                .WithAddRoadNode(TestData.AddEndNode1)
                .WithModifyRoadSegment(modifySegmentToMeasured)
                .WithRemoveOutlinedRoadSegment(new RemoveOutlinedRoadSegment
                {
                    Id = outlinedSegmentAdded.Id
                })
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
                        RoadSegmentModified = segmentToMeasuredModified
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .Then(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(segmentToMeasuredModified.Id)), new RoadNetworkChangesAccepted
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
                        OutlinedRoadSegmentRemoved = new OutlinedRoadSegmentRemoved
                        {
                            Id = segmentToMeasuredModified.Id
                        }
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

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
        [
            new LineString(
                new CoordinateArraySequence([pointA.Coordinate, pointB.Coordinate]),
                GeometryConfiguration.GeometryFactory
            )
        ]) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var line2Before = new MultiLineString(
        [
            new LineString(
                new CoordinateArraySequence([pointC.Coordinate, pointD.Coordinate]),
                GeometryConfiguration.GeometryFactory
            )
        ]) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var line2After = new MultiLineString(
        [
            new LineString(
                new CoordinateArraySequence([pointCModified.Coordinate, pointDModified.Coordinate]),
                GeometryConfiguration.GeometryFactory
            )
        ]) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

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
            MaintenanceAuthority = TestData.ChangedByOrganization,
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
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = TestData.RequestId, Reason = TestData.ReasonForChange, Operator = TestData.ChangedByOperator,
                    OrganizationId = TestData.ChangedByOrganization, Organization = TestData.ChangedByOrganizationName,
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
                            }
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
                            }
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
                                LeftSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
                                {
                                    StreetNameId = ObjectProvider.Create<StreetNameLocalId?>()
                                },
                                RightSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
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
                            }
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
                            }
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
                            }
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
                                LeftSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
                                {
                                    StreetNameId = ObjectProvider.Create<StreetNameLocalId?>()
                                },
                                RightSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
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
                            }
                        }
                    ]
                })
                .When(new ChangeRoadNetworkBuilder(TestData)
                        .WithModifyRoadNode(new ModifyRoadNode
                        {
                            Id = nodeC,
                            Geometry = GeometryTranslator.Translate(pointCModified),
                            Type = RoadNodeType.EndNode
                        })
                        .WithModifyRoadNode(new ModifyRoadNode
                        {
                            Id = nodeD,
                            Geometry = GeometryTranslator.Translate(pointDModified),
                            Type = RoadNodeType.EndNode
                        })
                        .WithModifyRoadSegment(modifyRoadSegment)
                        .Build()
                )
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = TestData.RequestId, Reason = TestData.ReasonForChange, Operator = TestData.ChangedByOperator, OrganizationId = TestData.ChangedByOrganization, Organization = TestData.ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes =
                    [
                        new RejectedChange
                        {
                            ModifyRoadSegment = modifyRoadSegment,
                            Problems =
                            [
                                new Problem
                                {
                                    Reason = nameof(IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction),
                                    Parameters =
                                    [
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
                                    ]
                                }
                            ]
                        }
                    ],
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
        [
            new LineString(
                new CoordinateArraySequence([pointA.Coordinate, pointB.Coordinate]),
                GeometryConfiguration.GeometryFactory
            )
        ]) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        var lineAfter = new MultiLineString(
        [
            new LineString(
                new CoordinateArraySequence([pointC.Coordinate, pointD.Coordinate]),
                GeometryConfiguration.GeometryFactory
            )
        ]) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

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
            MaintenanceAuthority = TestData.ChangedByOrganization,
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
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = TestData.RequestId, Reason = TestData.ReasonForChange, Operator = TestData.ChangedByOperator,
                    OrganizationId = TestData.ChangedByOrganization, Organization = TestData.ChangedByOrganizationName,
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
                            }
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
                            }
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
                                LeftSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
                                {
                                    StreetNameId = ObjectProvider.Create<StreetNameLocalId?>()
                                },
                                RightSide = new RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes
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
                            }
                        }
                    ]
                })
                .When(new ChangeRoadNetworkBuilder(TestData)
                    .WithAddRoadNode(new AddRoadNode
                    {
                        TemporaryId = nodeC,
                        Geometry = GeometryTranslator.Translate(pointC),
                        Type = RoadNodeType.EndNode
                    })
                    .WithAddRoadNode(new AddRoadNode
                    {
                        TemporaryId = nodeD,
                        Geometry = GeometryTranslator.Translate(pointD),
                        Type = RoadNodeType.EndNode
                    })
                    .WithModifyRoadSegment(modifyRoadSegment)
                    .Build()
                )
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = TestData.RequestId, Reason = TestData.ReasonForChange, Operator = TestData.ChangedByOperator, OrganizationId = TestData.ChangedByOrganization, Organization = TestData.ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes =
                    [
                        new RejectedChange
                        {
                            ModifyRoadSegment = modifyRoadSegment,
                            Problems =
                            [
                                new Problem
                                {
                                    Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                    Parameters =
                                    [
                                        new ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = nodeA.ToInt32().ToString()
                                        }
                                    ]
                                },
                                new Problem
                                {
                                    Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                    Parameters =
                                    [
                                        new ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = nodeB.ToInt32().ToString()
                                        }
                                    ]
                                }
                            ]
                        }
                    ],
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
        );
    }

    // obsolete
    // [Fact]
    // public Task GivenUpgradedCategory_WhenCategoryNotModified_ThenIgnoreNewValueAndGiveWarning()
    // {
    //     var upgradedCategory = RoadSegmentCategory.EuropeanMainRoad;
    //     var notUpgradedCategory = RoadSegmentCategory.Unknown;
    //
    //     var initial = new RoadNetworkChangesAcceptedBuilder(TestData)
    //         .WithRoadNodeAdded(TestData.StartNode1Added)
    //         .WithRoadNodeAdded(TestData.EndNode1Added)
    //         .WithRoadSegmentAdded(TestData.Segment1Added, segment => { segment.Category = upgradedCategory; })
    //         .Build();
    //
    //     var command = new ChangeRoadNetworkBuilder(TestData)
    //         .WithModifyRoadSegment(TestData.ModifySegment1, segment =>
    //         {
    //             segment.Category = notUpgradedCategory;
    //             //segment.CategoryModified = false;
    //         })
    //         .Build();
    //
    //     var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
    //         .WithClock(Clock)
    //         .WithTransactionId(2)
    //         .WithRoadSegmentModified(TestData.Segment1Modified,
    //             segment => { segment.Category = upgradedCategory; },
    //             [
    //                 new Problem
    //                 {
    //                     Severity = ProblemSeverity.Warning,
    //                     Reason = "RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion",
    //                     Parameters =
    //                     [
    //                         new ProblemParameter
    //                         {
    //                             Name = "Identifier",
    //                             Value = "1"
    //                         }
    //                     ]
    //                 }
    //             ])
    //         .Build();
    //
    //     return Run(scenario =>
    //         scenario
    //             .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
    //             .Given(RoadNetworks.Stream, initial)
    //             .When(command)
    //             .Then(RoadNetworks.Stream, expected)
    //     );
    // }

    [Fact]
    public Task GivenUpgradedCategory_WhenCategoryNotModifiedAndSegmentIsOutlined_ThenKeepNewValue()
    {
        var testData = new RoadNetworkTestData(fixture => { fixture.CustomizeRoadSegmentOutline(); });

        var upgradedCategory = RoadSegmentCategory.EuropeanMainRoad;
        var notUpgradedCategory = RoadSegmentCategory.Unknown;

        var initial = new RoadNetworkChangesAcceptedBuilder(testData)
            .WithOutlinedRoadSegment(testData.Segment1Added, segment => { segment.Category = upgradedCategory; })
            .Build();

        var command = new ChangeRoadNetworkBuilder(testData)
            .WithModifyOutlinedRoadSegment(testData.ModifySegment1, segment =>
            {
                segment.Category = notUpgradedCategory;
                //segment.CategoryModified = false;
            })
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(testData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithOutlinedRoadSegmentModified(testData.Segment1Modified,
                segment => { segment.Category = notUpgradedCategory; })
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
            .WithRoadSegmentAdded(TestData.Segment1Added, segment => { segment.Category = upgradedCategory; })
            .Build();

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithModifyRoadSegment(TestData.ModifySegment1, segment =>
            {
                segment.Category = category;
                //segment.CategoryModified = true;
            })
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentModified(TestData.Segment1Modified,
                segment => { segment.Category = category; })
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
