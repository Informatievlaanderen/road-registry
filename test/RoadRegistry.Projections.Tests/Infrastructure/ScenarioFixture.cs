namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
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

//            Customize<ILineString>(customizations =>
//                customizations.FromFactory(generator =>
//                {
//                    return new LineString(
//                        CreateMany(
//                            generator.Next(1, 50),
//                            () => new Point(this.Create<double>(), this.Create<double>(), this.Create<double>(), this.Create<double>())
//                        ));
//                }));

            Customize<Events.Geometry>(customization =>
                customization.FromFactory<int>(value =>
                    new Events.Geometry
                    {
                        SpatialReferenceSystemIdentifier = value,
                        WellKnownBinary = this.Create<Point>().ToBinary()
                    }));

            LimitFieldLength<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length);
            LimitFieldLength<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length);

            LimitFieldLength<ImportedRoadSegment>(segment => segment.MaintainerId, RoadSegmentDbaseRecord.Schema.BEHEER.Length);

            LimitFieldLength<ImportedReferencePoint>(point => point.Ident8, RoadReferencePointDbaseRecord.Schema.IDENT8.Length);

            LimitFieldLength<Organization>(p => p.Id, RoadSegmentDbaseRecord.Schema.BEHEER.Length);
            LimitFieldLength<Organization>(p => p.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length);
        }

        private void LimitFieldLength<T>(Expression<Func<T, string>> field, int length)
        {
            Customizations.Add(new StringPropertyTruncateSpecimenBuilder<T>(field, length));
        }

        public IEnumerable<T> CreateMany<T>(int amount, Func<T> create)
        {
            if(amount < 0)
                throw new ArgumentException($"Cannot generate {amount} number of {typeof(T)}");

            for (var i = 0; i < amount; i++)
            {
                yield return create();
            }
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
