namespace RoadRegistry.BackOffice
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Core;
    using Messages;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using Uploads;
    using RoadSegmentLaneAttribute = Core.RoadSegmentLaneAttribute;
    using RoadSegmentSurfaceAttribute = Core.RoadSegmentSurfaceAttribute;
    using RoadSegmentWidthAttribute = Core.RoadSegmentWidthAttribute;

    internal static class SharedCustomizations
    {
        public static void CustomizeGradeSeparatedJunctionId(this IFixture fixture)
        {
            fixture.Customize<GradeSeparatedJunctionId>(composer =>
                composer.FromFactory<int>(value => new GradeSeparatedJunctionId(Math.Abs(value)))
            );
        }

        public static void CustomizeGradeSeparatedJunctionType(this IFixture fixture)
        {
            fixture.Customize<GradeSeparatedJunctionType>(composer =>
                composer.FromFactory<int>(value => GradeSeparatedJunctionType.All[value % GradeSeparatedJunctionType.All.Length]));
        }

        public static void CustomizeRoadNodeId(this IFixture fixture)
        {
            fixture.Customize<RoadNodeId>(composer =>
                composer.FromFactory<int>(value => new RoadNodeId(Math.Abs(value)))
            );
        }

        public static void CustomizeAttributeId(this IFixture fixture)
        {
            fixture.Customize<AttributeId>(composer =>
                composer.FromFactory<int>(value => new AttributeId(Math.Abs(value)))
            );
        }

        public static void CustomizeOrganizationId(this IFixture fixture)
        {
            fixture.Customize<OrganizationId>(composer =>
                composer.FromFactory(generator =>
                    new OrganizationId(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, OrganizationId.MaxLength + 1))))
            );
        }

        public static void CustomizeExternalExtractRequestId(this IFixture fixture)
        {
            fixture.Customize<ExternalExtractRequestId>(composer =>
                composer.FromFactory(generator =>
                    new ExternalExtractRequestId(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, ExternalExtractRequestId.MaxLength + 1))))
            );
        }

        public static void CustomizeCrabStreetnameId(this IFixture fixture)
        {
            fixture.Customize<CrabStreetnameId>(composer =>
                composer.FromFactory(generator =>
                {
                    switch (generator.Next() % 3)
                    {
                        case 0:
                            return new CrabStreetnameId(CrabStreetnameId.Unknown);
                        case 1:
                            return new CrabStreetnameId(CrabStreetnameId.NotApplicable);
                        default:
                            return new CrabStreetnameId(generator.Next(0, int.MaxValue));
                    }
                })
            );
        }

        public static void CustomizeArchiveId(this IFixture fixture)
        {
            fixture.Customize<ArchiveId>(composer =>
                composer.FromFactory(generator =>
                    new ArchiveId(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, ArchiveId.MaxLength + 1))))
            );
        }

        public static void CustomizeChangeRequestId(this IFixture fixture)
        {
            fixture.Customize<ChangeRequestId>(composer =>
                composer.FromFactory(generator =>
                    new ChangeRequestId(Enumerable.Range(0,ChangeRequestId.ExactLength).Select(index => (byte)generator.Next(0,256)).ToArray())
                )
            );
        }

        public static void CustomizeExtractRequestId(this IFixture fixture)
        {
            fixture.Customize<ExtractRequestId>(composer =>
                composer.FromFactory(generator =>
                    new ExtractRequestId(Enumerable.Range(0,ExtractRequestId.ExactLength).Select(index => (byte)generator.Next(0,256)).ToArray())
                )
            );
        }

        public static void CustomizeOrganizationName(this IFixture fixture)
        {
            fixture.Customize<OrganizationName>(composer =>
                composer.FromFactory(generator =>
                    new OrganizationName(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, OrganizationName.MaxLength + 1))))
            );
        }

        public static void CustomizeOperatorName(this IFixture fixture)
        {
            fixture.Customize<OperatorName>(composer =>
                composer.FromFactory(generator =>
                    new OperatorName(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, OperatorName.MaxLength + 1))))
            );
        }

        public static void CustomizeReason(this IFixture fixture)
        {
            fixture.Customize<Reason>(composer =>
                composer.FromFactory(generator =>
                    new Reason(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, Reason.MaxLength + 1))))
            );
        }

        public static void CustomizeTransactionId(this IFixture fixture)
        {
            fixture.Customize<TransactionId>(composer =>
                composer.FromFactory(generator => new TransactionId(generator.Next()))
            );
        }

        public static void CustomizeRoadSegmentId(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentId>(composer =>
                composer.FromFactory<int>(value => new RoadSegmentId(Math.Abs(value)))
            );
        }

        public static void CustomizeRoadSegmentGeometryVersion(this IFixture fixture)
        {
            fixture.Customize<GeometryVersion>(composer =>
                composer.FromFactory<int>(value => new GeometryVersion(Math.Abs(value)))
            );
        }

        public static void CustomizeRoadNodeType(this IFixture fixture)
        {
            fixture.Customize<RoadNodeType>(composer =>
                composer.FromFactory<int>(value => RoadNodeType.All[Math.Abs(value) % RoadNodeType.All.Length]));
        }

        public static void CustomizeRecordType(this IFixture fixture)
        {
            fixture.Customize<RecordType>(composer =>
                composer.FromFactory<int>(value => RecordType.All[Math.Abs(value) % RecordType.All.Length]));
        }

        public static void CustomizeEuropeanRoadNumber(this IFixture fixture)
        {
            fixture.Customize<EuropeanRoadNumber>(composer =>
                composer.FromFactory<int>(value => EuropeanRoadNumber.All[Math.Abs(value) % EuropeanRoadNumber.All.Length]));
        }

        public static void CustomizeNationalRoadNumber(this IFixture fixture)
        {
            fixture.Customize<NationalRoadNumber>(composer =>
                composer.FromFactory<int>(value => NationalRoadNumbers.All[Math.Abs(value) % NationalRoadNumbers.All.Length]));
        }

        public static void CustomizeNumberedRoadNumber(this IFixture fixture)
        {
            fixture.Customize<NumberedRoadNumber>(composer =>
                composer.FromFactory<int>(value => NumberedRoadNumbers.All[Math.Abs(value) % NumberedRoadNumbers.All.Length]));
        }

        public static void CustomizeAttributeHash(this IFixture fixture)
        {
            fixture.Customize<AttributeHash>(
                customization => customization.FromFactory(
                    generator =>
                    {
                        var result = new AttributeHash(
                            fixture.Create<RoadSegmentAccessRestriction>(),
                            fixture.Create<RoadSegmentCategory>(),
                            fixture.Create<RoadSegmentMorphology>(),
                            fixture.Create<RoadSegmentStatus>(),
                            fixture.Create<CrabStreetnameId?>(),
                            fixture.Create<CrabStreetnameId?>(),
                            fixture.Create<OrganizationId>());
                        var times = generator.Next(0, 10);
                        for (var index = 0; index < times; index++)
                        {
                            switch (generator.Next(0, 7))
                            {
                                case 0:
                                    result = result.With(fixture.Create<RoadSegmentCategory>());
                                    break;
                                case 1:
                                    result = result.With(fixture.Create<RoadSegmentMorphology>());
                                    break;
                                case 2:
                                    result = result.With(fixture.Create<RoadSegmentStatus>());
                                    break;
                                case 3:
                                    result = result.With(fixture.Create<RoadSegmentAccessRestriction>());
                                    break;
                                case 4:
                                    result = result.WithLeftSide(fixture.Create<CrabStreetnameId?>());
                                    break;
                                case 5:
                                    result = result.WithRightSide(fixture.Create<CrabStreetnameId?>());
                                    break;
                                case 6:
                                    result = result.With(fixture.Create<OrganizationId>());
                                    break;
                            }
                        }

                        return result;
                    }));
        }

        public static void CustomizePoint(this IFixture fixture)
        {
            fixture.Customize<NetTopologySuite.Geometries.Point>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.Point(
                        fixture.Create<double>(),
                        fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
        }

        public static void CustomizePolylineM(this IFixture fixture)
        {
            fixture.Customize<NetTopologySuite.Geometries.CoordinateM>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.CoordinateM(
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );

            fixture.Customize<NetTopologySuite.Geometries.LineString>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.LineString(
                        new CoordinateArraySequence(fixture.CreateMany<CoordinateM>(2).Cast<Coordinate>().ToArray()),
                        GeometryConfiguration.GeometryFactory)
                ).OmitAutoProperties()
            );

            fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(
                        fixture.CreateMany<NetTopologySuite.Geometries.LineString>(1).ToArray(),
                        GeometryConfiguration.GeometryFactory)
                ).OmitAutoProperties()
            );
        }

        public static void CustomizeMunicipalityGeometry(this IFixture fixture)
        {
            fixture.Customize<Ring>(customization =>
                customization.FromFactory(generator =>
                {
                    var ring = new Ring
                    {
                        Points = fixture.CreateMany<Messages.Point>().ToArray()
                    };

                    ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                    return ring;
                }).OmitAutoProperties());

            fixture.Customize<Messages.Polygon>(customization =>
                customization.FromFactory(generator => new Messages.Polygon
                {
                    Shell = fixture.Create<Ring>(),
                    Holes = new Ring[0]
                }).OmitAutoProperties());

            fixture.Customize<MunicipalityGeometry>(customization =>
                customization.FromFactory(generator =>
                {
                    var geometry = new MunicipalityGeometry
                    {
                        MultiPolygon = new []{fixture.Create<Messages.Polygon>()},
                        SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                    };
                    return geometry;
                }).OmitAutoProperties()
            );
        }

        public static void CustomizeRoadNetworkExtractGeometry(this IFixture fixture)
        {
            fixture.Customize<Ring>(customization =>
                customization.FromFactory(generator =>
                {
                    var ring = new Ring
                    {
                        Points = fixture.CreateMany<Messages.Point>().ToArray()
                    };

                    ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                    return ring;
                }).OmitAutoProperties());

            fixture.Customize<Messages.Polygon>(customization =>
                customization.FromFactory(generator => new Messages.Polygon
                {
                    Shell = fixture.Create<Ring>(),
                    Holes = new Ring[0]
                }).OmitAutoProperties());

            fixture.Customize<RoadNetworkExtractGeometry>(customization =>
                customization.FromFactory(generator =>
                {
                    var geometry = new RoadNetworkExtractGeometry
                    {
                        SpatialReferenceSystemIdentifier =
                            SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                    };

                    if (generator.Next() % 2 == 0)
                    {
                        geometry.MultiPolygon = new[] { fixture.Create<Messages.Polygon>() };
                        geometry.Polygon = null;
                    }
                    else
                    {
                        geometry.Polygon = fixture.Create<Messages.Polygon>();
                        geometry.MultiPolygon = null;
                    }

                    return geometry;
                }).OmitAutoProperties()
            );
        }

        public static void CustomizeMultiPolygon(this IFixture fixture)
        {
            fixture.Customize<Ring>(customization =>
                customization.FromFactory(generator =>
                {
                    var ring = new Ring
                    {
                        Points = fixture.CreateMany<Messages.Point>().ToArray()
                    };

                    ring.Points = ring.Points.Append(ring.Points[0]).ToArray();

                    return ring;
                }).OmitAutoProperties());

            fixture.Customize<Messages.Polygon>(customization =>
                customization.FromFactory(generator => new Messages.Polygon
                {
                    Shell = fixture.Create<Ring>(),
                    Holes = new Ring[0]
                }).OmitAutoProperties());

            fixture.Customize<RoadNetworkExtractGeometry>(customization =>
                customization.FromFactory(generator =>
                {
                    var geometry = new RoadNetworkExtractGeometry
                    {
                        SpatialReferenceSystemIdentifier =
                            SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                    };

                    if (generator.Next() % 2 == 0)
                    {
                        geometry.MultiPolygon = new[] { fixture.Create<Messages.Polygon>() };
                        geometry.Polygon = null;
                    }
                    else
                    {
                        geometry.Polygon = fixture.Create<Messages.Polygon>();
                        geometry.MultiPolygon = null;
                    }

                    return geometry;
                }).OmitAutoProperties()
            );
        }

        public static IPostprocessComposer<T> FromFactory<T>(this IFactoryComposer<T> composer, Func<Random, T> factory)
        {
            return composer.FromFactory<int>(value => factory(new Random(value)));
        }

        public static void CustomizeRoadSegmentAccessRestriction(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentAccessRestriction>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentAccessRestriction.All[generator.Next() % RoadSegmentAccessRestriction.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentCategory(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentCategory>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentCategory.All[generator.Next() % RoadSegmentCategory.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentGeometryDrawMethod(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentGeometryDrawMethod>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentGeometryDrawMethod.All[generator.Next() % RoadSegmentGeometryDrawMethod.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentSurfaceType(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentSurfaceType>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentSurfaceType.All[generator.Next() % RoadSegmentSurfaceType.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentLaneDirection(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentLaneDirection>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentLaneDirection.All[generator.Next() % RoadSegmentLaneDirection.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentLaneCount(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentLaneCount>(customization =>
                customization.FromFactory<int>(
                    value => value == RoadSegmentLaneCount.Unknown || value == RoadSegmentLaneCount.NotApplicable
                        ? new RoadSegmentLaneCount(value)
                        : new RoadSegmentLaneCount(Math.Abs(value) % RoadSegmentLaneCount.Maximum.ToInt32())
                )
            );
        }

        public static void CustomizeRoadSegmentMorphology(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentMorphology>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentMorphology.All[generator.Next() % RoadSegmentMorphology.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentNumberedRoadDirection(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentNumberedRoadDirection>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentNumberedRoadDirection.All[generator.Next() % RoadSegmentNumberedRoadDirection.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentNumberedRoadOrdinal(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentNumberedRoadOrdinal>(customization =>
                customization.FromFactory<int>(
                    value => new RoadSegmentNumberedRoadOrdinal(Math.Abs(value))
                )
            );
        }

        public static void CustomizeRoadSegmentPosition(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentPosition>(customization =>
                customization.FromFactory<decimal>(
                    value => new RoadSegmentPosition(Math.Abs(value))
                )
            );
        }

        public static void CustomizeRoadSegmentStatus(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentStatus>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentStatus.All[generator.Next() % RoadSegmentStatus.All.Length]
                )
            );
        }

        public static void CustomizeRoadSegmentWidth(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentWidth>(customization =>
                customization.FromFactory<int>(
                    value => value == RoadSegmentWidth.Unknown || value == RoadSegmentWidth.NotApplicable
                        ? new RoadSegmentWidth(value)
                        : new RoadSegmentWidth(Math.Abs(value) % RoadSegmentWidth.Maximum.ToInt32())
                )
            );
        }

        public static void CustomizeRoadSegmentLaneAttribute(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentLaneAttribute>(customization =>
                customization.FromFactory<int>(
                    value =>
                    {
                        var generator = new Generator<RoadSegmentPosition>(fixture);
                        var from = generator.First();
                        var to = generator.First(candidate => candidate > from);
                        return new RoadSegmentLaneAttribute(
                            fixture.Create<AttributeId>(),
                            fixture.Create<AttributeId>(),
                            fixture.Create<RoadSegmentLaneCount>(),
                            fixture.Create<RoadSegmentLaneDirection>(),
                            from,
                            to,
                            fixture.Create<GeometryVersion>()
                        );
                    }
                )
            );
        }

        public static void CustomizeRoadSegmentWidthAttribute(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentWidthAttribute>(customization =>
                customization.FromFactory<int>(
                    value =>
                    {
                        var generator = new Generator<RoadSegmentPosition>(fixture);
                        var from = generator.First();
                        var to = generator.First(candidate => candidate > from);
                        return new RoadSegmentWidthAttribute(
                            fixture.Create<AttributeId>(),
                            fixture.Create<AttributeId>(),
                            fixture.Create<RoadSegmentWidth>(),
                            from,
                            to,
                            fixture.Create<GeometryVersion>()
                        );
                    }
                )
            );
        }

        public static void CustomizeRoadSegmentSurfaceAttribute(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentSurfaceAttribute>(customization =>
                customization.FromFactory<int>(
                    value =>
                    {
                        var generator = new Generator<RoadSegmentPosition>(fixture);
                        var from = generator.First();
                        var to = generator.First(candidate => candidate > from);
                        return new RoadSegmentSurfaceAttribute(
                            fixture.Create<AttributeId>(),
                            fixture.Create<AttributeId>(),
                            fixture.Create<RoadSegmentSurfaceType>(),
                            from,
                            to,
                            fixture.Create<GeometryVersion>()
                        );
                    }
                )
            );
        }

        public static void CustomizeOriginProperties(this IFixture fixture)
        {
            fixture.Customize<ImportedOriginProperties>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedOriginProperties
                        {
                            Organization = fixture.Create<OrganizationName>(),
                            OrganizationId = fixture.Create<OrganizationId>(),
                            Operator = fixture.Create<OperatorName>(),
                            Application = fixture.Create<string>(),
                            Since = fixture.Create<DateTime>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }
    }
}
