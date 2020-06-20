namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using NodaTime.Text;
    using RoadSegmentLaneAttributes = BackOffice.Messages.RoadSegmentLaneAttributes;
    using RoadSegmentSurfaceAttributes = BackOffice.Messages.RoadSegmentSurfaceAttributes;
    using RoadSegmentWidthAttributes = BackOffice.Messages.RoadSegmentWidthAttributes;

    internal static class Customizations
    {
        public static void CustomizeImportedRoadNode(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadNode>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadNode
                        {
                            Id = fixture.Create<RoadNodeId>(),
                            Type = fixture.Create<RoadNodeType>(),
                            Geometry = GeometryTranslator.Translate(fixture.Create<NetTopologySuite.Geometries.Point>()),
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
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
                                Code = fixture.Create<OrganizationId>(),
                                Name = fixture.Create<OrganizationName>()
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
                            Origin = fixture.Create<ImportedOriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeGradeSeparatedJunctionAdded(this IFixture fixture)
        {
            fixture.Customize<BackOffice.Messages.AcceptedChange>(customization =>
                customization
                    .FromFactory(generator =>
                        new BackOffice.Messages.AcceptedChange
                        {
                            GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
                            {
                                Id = fixture.Create<GradeSeparatedJunctionId>(),
                                TemporaryId = fixture.Create<GradeSeparatedJunctionId>(),
                                Type = fixture.Create<GradeSeparatedJunctionType>(),
                                LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                                UpperRoadSegmentId = fixture.Create<RoadSegmentId>()
                            }
                        }
                    )
                    .OmitAutoProperties()
                );
            fixture.Customize<RoadNetworkChangesAccepted>(customization =>
                customization
                    .FromFactory(generator =>
                        new RoadNetworkChangesAccepted
                        {
                            RequestId = fixture.Create<ArchiveId>(),
                            Reason = fixture.Create<Reason>(),
                            Operator = fixture.Create<OperatorName>(),
                            OrganizationId = fixture.Create<OrganizationId>(),
                            Organization = fixture.Create<OrganizationName>(),
                            Changes = fixture.CreateMany<BackOffice.Messages.AcceptedChange>(generator.Next(1,5)).ToArray(),
                            When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeRoadSegmentAdded(this IFixture fixture)
        {
            fixture.Customize<BackOffice.Messages.AcceptedChange>(customization =>
                customization
                    .FromFactory(generator =>
                        new BackOffice.Messages.AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = fixture.Create<RoadSegmentId>(),
                                TemporaryId = fixture.Create<RoadSegmentId>(),
                                Category = fixture.Create<RoadSegmentCategory>(),
                                Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                                Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(generator.Next(1,5)).ToArray(),
                                Morphology = fixture.Create<RoadSegmentMorphology>(),
                                Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(generator.Next(1,5)).ToArray(),
                                Version = fixture.Create<int>(),
                                Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(generator.Next(1,5)).ToArray(),
                                LeftSide = fixture.Create<RoadSegmentSideAttributes>(),
                                RightSide = fixture.Create<RoadSegmentSideAttributes>(),
                                MaintenanceAuthority = new MaintenanceAuthority
                                {
                                    Code = fixture.Create<OrganizationId>(),
                                    Name = fixture.Create<OrganizationName>()
                                },
                                GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                                GeometryVersion = fixture.Create<GeometryVersion>(),
                                Status = fixture.Create<RoadSegmentStatus>(),
                                AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                                StartNodeId = fixture.Create<RoadNodeId>(),
                                EndNodeId = fixture.Create<RoadNodeId>()
                            }
                        }
                    )
                    .OmitAutoProperties()
                );
            fixture.Customize<RoadNetworkChangesAccepted>(customization =>
                customization
                    .FromFactory(generator =>
                        new RoadNetworkChangesAccepted
                        {
                            RequestId = fixture.Create<ArchiveId>(),
                            Reason = fixture.Create<Reason>(),
                            Operator = fixture.Create<OperatorName>(),
                            OrganizationId = fixture.Create<OrganizationId>(),
                            Organization = fixture.Create<OrganizationName>(),
                            Changes = fixture.CreateMany<BackOffice.Messages.AcceptedChange>(generator.Next(1,5)).ToArray(),
                            When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                        }
                    )
                    .OmitAutoProperties()
            );
        }
    }
}
