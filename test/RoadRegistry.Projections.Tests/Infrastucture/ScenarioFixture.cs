namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AutoFixture;
    using AutoFixture.Dsl;
    using Events;
    using Wkx;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            Customize<Point>(customization =>
                customization.FromFactory(generator =>
                    new Point(generator.NextDouble(), generator.NextDouble())
                ));

            Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(this.CreateMany<LineString>(generator.Next(1,10)))
                ));

            Customize<Events.Geometry>(customization =>
                customization.FromFactory<int>(value =>
                    new Events.Geometry
                    {
                        SpatialReferenceSystemIdentifier = value,
                        WellKnownBinary = this.Create<Point>().SerializeByteArray<WkbSerializer>()
                    }));

            LimitFieldLength<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length);
            LimitFieldLength<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length);

            LimitFieldLength<ImportedRoadSegment>(segment => segment.MaintainerId, RoadSegmentDbaseRecord.Schema.BEHEER.Length);

            LimitFieldLength<ImportedReferencePoint>(point => point.Ident8, RoadReferencePointDbaseRecord.Schema.IDENT8.Length);

            LimitFieldLength<Organisation>(p => p.Id, RoadSegmentDbaseRecord.Schema.BEHEER.Length);
            LimitFieldLength<Organisation>(p => p.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length);
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
