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
            fixture.Customize<ImportedRoadSegmentLaneAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentLaneAttribute
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
            fixture.Customize<ImportedRoadSegmentWidthAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentWidthAttribute
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
            fixture.Customize<ImportedRoadSegmentSurfaceAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentSurfaceAttribute
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
            fixture.Customize<ImportedRoadSegmentEuropeanRoadAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentEuropeanRoadAttribute
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
            fixture.Customize<ImportedRoadSegmentNationalRoadAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentNationalRoadAttribute
                        {
                            AttributeId = fixture.Create<int>(),
                            Number = fixture.Create<NationalRoadNumber>(),
                            Origin = fixture.Create<ImportedOriginProperties>()
                        }
                    )
                    .OmitAutoProperties()
            );
        }

        public static void CustomizeImportedRoadSegmentNumberedRoadAttributes(this IFixture fixture)
        {
            fixture.Customize<ImportedRoadSegmentNumberedRoadAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentNumberedRoadAttribute
                        {
                            AttributeId = fixture.Create<int>(),
                            Number = fixture.Create<NumberedRoadNumber>(),
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
            fixture.Customize<ImportedRoadSegmentSideAttribute>(customization =>
                customization
                    .FromFactory(generator =>
                        new ImportedRoadSegmentSideAttribute
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
                            LeftSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                            RightSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                            Lanes = fixture.CreateMany<ImportedRoadSegmentLaneAttribute>(generator.Next(0, 10)).ToArray(),
                            Widths = fixture.CreateMany<ImportedRoadSegmentWidthAttribute>(generator.Next(0, 10)).ToArray(),
                            Surfaces = fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>(generator.Next(0, 10)).ToArray(),
                            PartOfEuropeanRoads = fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(generator.Next(0, 10)).ToArray(),
                            PartOfNationalRoads = fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>(generator.Next(0, 10)).ToArray(),
                            PartOfNumberedRoads = fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(generator.Next(0, 10)).ToArray(),
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
                            TransactionId = fixture.Create<TransactionId>(),
                            Changes = fixture.CreateMany<BackOffice.Messages.AcceptedChange>(generator.Next(1,5)).ToArray(),
                            When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                        }
                    )
                    .OmitAutoProperties()
            );
        }
    }
}
