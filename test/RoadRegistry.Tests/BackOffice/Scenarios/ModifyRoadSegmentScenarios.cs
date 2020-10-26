namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using NetTopologySuite.Geometries;
    using NodaTime.Text;
    using Core;
    using RoadRegistry.Framework.Testing;
    using Xunit;

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
        public Task move_segment()
        {
            var pointA = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            var nodeA = Fixture.Create<RoadNodeId>();
            var pointB = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            var nodeB = Fixture.Create<RoadNodeId>();
            var pointC = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 10.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            var nodeC = Fixture.Create<RoadNodeId>();
            var pointD = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            var nodeD = Fixture.Create<RoadNodeId>();
            var segment = Fixture.Create<RoadSegmentId>();
            var lineBefore = new NetTopologySuite.Geometries.MultiLineString(
                new []
                {
                    new NetTopologySuite.Geometries.LineString(
                        new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(new [] { pointA.Coordinate, pointB.Coordinate }),
                        Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            var lineAfter = new NetTopologySuite.Geometries.MultiLineString(
                new []
                {
                    new NetTopologySuite.Geometries.LineString(
                        new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(new [] { pointC.Coordinate, pointD.Coordinate }),
                        Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            var count = 3;

            var modifyRoadSegment = new Messages.ModifyRoadSegment
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
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttribute>(count)
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

                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttribute>(3)
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

                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttribute>(3)
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

                        return part;
                    })
                    .ToArray()
            };

            return Run(scenario =>
                scenario
                    .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                        new Messages.ImportedOrganization
                        {
                            Code = ChangedByOrganization,
                            Name = ChangedByOrganizationName,
                            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                        }
                    )
                    .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                        {
                            RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                            OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                            TransactionId = new TransactionId(1),
                            Changes = new[]
                            {
                                new Messages.AcceptedChange
                                {
                                    RoadNodeAdded = new Messages.RoadNodeAdded
                                    {
                                        Id = nodeA,
                                        TemporaryId = Fixture.Create<RoadNodeId>(),
                                        Geometry = GeometryTranslator.Translate(pointA),
                                        Type = RoadNodeType.EndNode
                                    }, Problems = new Messages.Problem[0]
                                },
                                new Messages.AcceptedChange
                                {
                                    RoadNodeAdded = new Messages.RoadNodeAdded
                                    {
                                        Id = nodeB,
                                        TemporaryId = Fixture.Create<RoadNodeId>(),
                                        Geometry = GeometryTranslator.Translate(pointB),
                                        Type = RoadNodeType.EndNode
                                    }, Problems = new Messages.Problem[0]
                                },
                                new Messages.AcceptedChange
                                {
                                    RoadSegmentAdded = new Messages.RoadSegmentAdded
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
                                        MaintenanceAuthority = new Messages.MaintenanceAuthority
                                        {
                                            Code = Fixture.Create<OrganizationId>(),
                                            Name = Fixture.Create<OrganizationName>()
                                        },
                                        LeftSide = new Messages.RoadSegmentSideAttributes
                                        {
                                            StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                        },
                                        RightSide = new Messages.RoadSegmentSideAttributes
                                        {
                                            StreetNameId = Fixture.Create<CrabStreetnameId?>()
                                        },
                                        Lanes = Fixture
                                            .CreateMany<Messages.RoadSegmentLaneAttributes>(count)
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

                                                return part;
                                            })
                                            .ToArray(),
                                        Widths = Fixture
                                            .CreateMany<Messages.RoadSegmentWidthAttributes>(3)
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

                                                return part;
                                            })
                                            .ToArray(),
                                        Surfaces = Fixture
                                            .CreateMany<Messages.RoadSegmentSurfaceAttributes>(3)
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

                                                return part;
                                            })
                                            .ToArray()
                                    }, Problems = new Messages.Problem[0]
                                }
                            }
                        })
                    .When(TheOperator.ChangesTheRoadNetwork(
                        RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                        new Messages.RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = nodeC,
                                Geometry = GeometryTranslator.Translate(pointC),
                                Type = RoadNodeType.EndNode
                            }
                        },
                        new Messages.RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = nodeD,
                                Geometry = GeometryTranslator.Translate(pointD),
                                Type = RoadNodeType.EndNode
                            }
                        },
                        new Messages.RequestedChange
                        {
                            ModifyRoadSegment = modifyRoadSegment
                        }))
                    .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(2),
                        Changes = new[]
                        {
                            new Messages.RejectedChange
                            {
                                ModifyRoadSegment = modifyRoadSegment,
                                Problems = new []
                                {
                                    new Messages.Problem
                                    {
                                        Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                        Parameters = new []
                                        {
                                            new Messages.ProblemParameter
                                            {
                                               Name = "RoadNodeId",
                                               Value = nodeA.ToInt32().ToString()
                                            }
                                        }
                                    },
                                    new Messages.Problem
                                    {
                                        Reason = nameof(RoadNodeNotConnectedToAnySegment),
                                        Parameters = new []
                                        {
                                            new Messages.ProblemParameter
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
}
