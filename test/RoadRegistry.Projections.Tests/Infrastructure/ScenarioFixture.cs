namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;
    using Events;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            Customizations.Add(new IdentifierBuilder());
            Customizations.Add(new LimitedLengthStringBuilder<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length));
            Customizations.Add(new LimitedLengthStringBuilder<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length));
            Customizations.Add(new LimitedLengthStringBuilder<Maintainer>(segment => segment.Code, RoadSegmentDbaseRecord.Schema.BEHEER.Length));
            Customizations.Add(new LimitedLengthStringBuilder<Maintainer>(segment => segment.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length));
            Customizations.Add(new LimitedLengthStringBuilder<ImportedReferencePoint>(point => point.Ident8, RoadReferencePointDbaseRecord.Schema.IDENT8.Length));
            Customizations.Add(new LimitedLengthIntegerBuilder<RoadSegmentLaneProperties>(lane => lane.Count, RoadSegmentDynamicLaneAttributeDbaseRecord.Schema.AANTAL.Length));
            Customizations.Add(new LimitedLengthIntegerBuilder<RoadSegmentWidthProperties>(width => width.Width, RoadSegmentDynamicWidthAttributeDbaseRecord.Schema.BREEDTE.Length));
            Customizations.Add(new LimitedLengthStringBuilder<RoadSegmentEuropeanRoadProperties>(road => road.RoadNumber, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema.EUNUMMER.Length));

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

            Customize<Coordinate>(customizations =>
                customizations.OmitAutoProperties()
            );
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
