namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoFixture;
    using AutoFixture.Dsl;
    using Events;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            Customize<Point>(customization =>
                customization.FromFactory(generator =>
                    new Point(generator.NextDouble(), generator.NextDouble())
                ).OmitAutoProperties());

            Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(this
                        .CreateMany<ILineString>(generator.Next(1,10))
                        .ToArray())
                ).OmitAutoProperties());

            Customize<ILineString>(customizations =>
                customizations.FromFactory(generator =>
                        new LineString(
                            this.CreateMany<Coordinate>(generator.Next(2, 50))
                                .ToArray()
                        )
                    ).OmitAutoProperties());

            Customize<Coordinate>(composer => composer.OmitAutoProperties());

            Customize<Events.Geometry>(customization =>
                customization.FromFactory<int>(value =>
                    new Events.Geometry
                    {
                        SpatialReferenceSystemIdentifier = value,
                        WellKnownBinary = this.Create<Point>().ToBinary()
                    }));

            LimitFieldLength<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length);
            LimitFieldLength<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length);

            LimitFieldLength<Maintainer>(segment => segment.Code, RoadSegmentDbaseRecord.Schema.BEHEER.Length);
            LimitFieldLength<Maintainer>(segment => segment.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length);

            LimitFieldLength<ImportedReferencePoint>(point => point.Ident8, RoadReferencePointDbaseRecord.Schema.IDENT8.Length);
        }

        private void LimitFieldLength<T>(Expression<Func<T, string>> field, int length)
        {
            Customizations.Add(new StringPropertyTruncateSpecimenBuilder<T>(field, length));
        }
    }

    public static class FactoryComposerExtensions
    {
        public static IPostprocessComposer<T> FromFactory<T>(this IFactoryComposer<T> composer, Func<Random, T> factory)
        {
            return composer.FromFactory<int>(value => factory(new Random(value)));
        }
    }
}
