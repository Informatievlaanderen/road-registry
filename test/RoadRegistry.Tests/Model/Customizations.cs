namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;

    internal static class Customizations
    {
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
                    new LineString(
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

        public static void CustomizeRoadSegmentHardeningType(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentHardeningType>(customization =>
                customization.FromFactory(generator =>
                    RoadSegmentHardeningType.All[generator.Next() % RoadSegmentHardeningType.All.Length]
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
                    value => new RoadSegmentLaneCount(Math.Abs(value))
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
                customization.FromFactory<double>(
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
                    value => new RoadSegmentWidth(Math.Abs(value))
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
                            fixture.Create<RoadSegmentWidth>(),
                            from,
                            to,
                            fixture.Create<GeometryVersion>()
                        );
                    }
                )
            );
        }

        public static void CustomizeRoadSegmentHardeningAttribute(this IFixture fixture)
        {
            fixture.Customize<RoadSegmentHardeningAttribute>(customization =>
                customization.FromFactory<int>(
                    value => 
                    {
                        var generator = new Generator<RoadSegmentPosition>(fixture);
                        var from = generator.First();
                        var to = generator.First(candidate => candidate > from);
                        return new RoadSegmentHardeningAttribute(
                            fixture.Create<RoadSegmentHardeningType>(),
                            from,
                            to,
                            fixture.Create<GeometryVersion>()
                        );
                    }
                )
            );
        }
    }
}
