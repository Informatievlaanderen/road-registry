namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using BackOffice;
    using Messages;
    using Model;
    using NetTopologySuite.Geometries;

    internal static class Customizations
    {
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

        public static void CustomizeImportedGradeSeparatedJunction(this IFixture fixture)
        {
            fixture.Customize<ImportedGradeSeparatedJunction>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedGradeSeparatedJunction
                        {
                            Id = fixture.Create<GradeSeparatedJunctionId>(),
                            Type = fixture.Create<GradeSeparatedJunctionType>(),
                            LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                            UpperRoadSegmentId = fixture.Create<RoadSegmentId>(),
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
                            Number = fixture.Create<EuropeanRoadNumber>(),
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
