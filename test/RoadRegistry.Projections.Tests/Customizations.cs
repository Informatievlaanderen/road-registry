namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using Model;

    internal static class Customizations
    {
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

        public static void CustomizeMaintenanceAuthorityId(this IFixture fixture)
        {
            fixture.Customize<MaintenanceAuthorityId>(composer =>
                composer.FromFactory(generator =>
                    new MaintenanceAuthorityId(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, MaintenanceAuthorityId.MaxLength + 1))))
            );
        }

        public static void CustomizeMaintenanceAuthorityName(this IFixture fixture)
        {
            fixture.Customize<MaintenanceAuthorityName>(composer =>
                composer.FromFactory(generator =>
                    new MaintenanceAuthorityName(new string(
                        (char)generator.Next(97, 123), // a-z
                        generator.Next(1, MaintenanceAuthorityName.MaxLength + 1))))
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
                composer.FromFactory<int>(value => RoadNodeType.All[value % RoadNodeType.All.Length]));
        }

        public static void CustomizeEuropeanRoadNumber(this IFixture fixture)
        {
            fixture.Customize<EuropeanRoadNumber>(composer =>
                composer.FromFactory<int>(value => EuropeanRoadNumber.All[value % EuropeanRoadNumber.All.Length]));
        }

        public static void CustomizeNationalRoadNumber(this IFixture fixture)
        {
            fixture.Customize<NationalRoadNumber>(composer =>
                composer.FromFactory<int>(value => NationalRoadNumber.All[value % NationalRoadNumber.All.Length]));
        }

        public static void CustomizeNumberedRoadNumber(this IFixture fixture)
        {
            fixture.Customize<NumberedRoadNumber>(composer =>
                composer.FromFactory<int>(value => NumberedRoadNumber.All[value % NumberedRoadNumber.All.Length]));
        }

        public static void CustomizePointM(this IFixture fixture)
        {
            fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
        }

        public static void CustomizePolylineM(this IFixture fixture)
        {
            fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>(),
                        fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );

            fixture.Customize<ILineString>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.LineString(
                        new PointSequence(fixture.CreateMany<PointM>()),
                        GeometryConfiguration.GeometryFactory)
                ).OmitAutoProperties()
            );

            fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(fixture.CreateMany<ILineString>(generator.Next(1, 10)).ToArray())
                ).OmitAutoProperties()
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
                    value => value == -8 || value == -9
                        ? new RoadSegmentLaneCount(value)
                        : new RoadSegmentLaneCount(Math.Abs(value % RoadSegmentLaneCount.Maximum.ToInt32()))
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
                    value => value == -8 || value == -9
                        ? new RoadSegmentWidth(value)
                        : new RoadSegmentWidth(Math.Abs(value % RoadSegmentWidth.Maximum.ToInt32()))
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
            fixture.Customize<OriginProperties>(customization =>
                customization
                    .FromFactory(generator =>
                        new OriginProperties
                        {
                            Organization = fixture.Create<MaintenanceAuthorityName>(),
                            OrganizationId = fixture.Create<MaintenanceAuthorityId>(),
                            Since = fixture.Create<DateTime>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadNode(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadNode>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadNode
                        {
                            Id = fixture.Create<RoadNodeId>(),
                            Type = fixture.Create<RoadNodeType>(),
                            Geometry = GeometryTranslator.Translate(fixture.Create<PointM>()),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentLaneAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentLaneAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentLaneAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>(),
                            FromPosition = fixture.Create<RoadSegmentPosition>(),
                            ToPosition = fixture.Create<RoadSegmentPosition>(),
                            AsOfGeometryVersion = fixture.Create<int>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentWidthAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentWidthAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentWidthAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            Width = fixture.Create<RoadSegmentWidth>(),
                            FromPosition = fixture.Create<RoadSegmentPosition>(),
                            ToPosition = fixture.Create<RoadSegmentPosition>(),
                            AsOfGeometryVersion = fixture.Create<int>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentSurfaceAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentSurfaceAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentSurfaceAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            Type = fixture.Create<RoadSegmentSurfaceType>(),
                            FromPosition = fixture.Create<RoadSegmentPosition>(),
                            ToPosition = fixture.Create<RoadSegmentPosition>(),
                            AsOfGeometryVersion = fixture.Create<int>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentEuropeanRoadAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentEuropeanRoadAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentEuropeanRoadAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            RoadNumber = fixture.Create<EuropeanRoadNumber>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentNationalRoadAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentNationalRoadAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentNationalRoadAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            Ident2 = fixture.Create<NationalRoadNumber>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentNumberedRoadAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentNumberedRoadAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentNumberedRoadAttributes
                        {
                            AttributeId = fixture.Create<int>(),
                            Ident8 = fixture.Create<NumberedRoadNumber>(),
                            Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                            Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentSideAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentSideAttributes>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentSideAttributes
                        {
                            StreetNameId = fixture.Create<int?>(),
                            StreetName = fixture.Create<string>(),
                            MunicipalityNISCode = fixture.Create<string>(),
                            Municipality = fixture.Create<string>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegment(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegment>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegment
                        {
                            Id = fixture.Create<RoadSegmentId>(),
                            Version = fixture.Create<int>(),
                            StartNodeId = fixture.Create<RoadNodeId>(),
                            EndNodeId = fixture.Create<RoadNodeId>(),
                            Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                            GeometryVersion = fixture.Create<GeometryVersion>(),
                            MaintenanceAuthority = new MaintenanceAuthority
                            {
                                Code = fixture.Create<MaintenanceAuthorityId>(),
                                Name = fixture.Create<MaintenanceAuthorityName>()
                            },
                            AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                            Morphology = fixture.Create<RoadSegmentMorphology>(),
                            Status = fixture.Create<RoadSegmentStatus>(),
                            Category = fixture.Create<RoadSegmentCategory>(),
                            GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                            LeftSide = fixture.Create<ImportedRoadSegmentSideAttributes>(),
                            RightSide = fixture.Create<ImportedRoadSegmentSideAttributes>(),
                            Lanes = fixture.CreateMany<ImportedRoadSegmentLaneAttributes>(generator.Next(0, 10)).ToArray(),
                            Widths = fixture.CreateMany<ImportedRoadSegmentWidthAttributes>(generator.Next(0, 10)).ToArray(),
                            Surfaces = fixture.CreateMany<ImportedRoadSegmentSurfaceAttributes>(generator.Next(0, 10)).ToArray(),
                            PartOfEuropeanRoads = fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttributes>(generator.Next(0, 10)).ToArray(),
                            PartOfNationalRoads = fixture.CreateMany<ImportedRoadSegmentNationalRoadAttributes>(generator.Next(0, 10)).ToArray(),
                            PartOfNumberedRoads = fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttributes>(generator.Next(0, 10)).ToArray(),
                            RecordingDate = fixture.Create<DateTime>(),
                            Origin = fixture.Create<OriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }
    }
}
