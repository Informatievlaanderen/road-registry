namespace RoadRegistry.Projections.Tests.Infrastructure
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Dsl;
    using Events;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Shaperon;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            Customizations.Add(new IdentifierBuilder());
            Customizations.Add(new LimitedLengthStringBuilder<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length));
            Customizations.Add(new LimitedLengthStringBuilder<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGNORG.Length));
            Customizations.Add(new LimitedLengthStringBuilder<Maintainer>(segment => segment.Code, RoadSegmentDbaseRecord.Schema.BEHEER.Length));
            Customizations.Add(new LimitedLengthStringBuilder<Maintainer>(segment => segment.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length));
            Customizations.Add(new LimitedLengthStringBuilder<ImportedReferencePoint>(point => point.Ident8, RoadReferencePointDbaseRecord.Schema.IDENT8.Length));
            Customizations.Add(new LimitedLengthIntegerBuilder<RoadSegmentLaneProperties>(lane => lane.Count, RoadSegmentDynamicLaneAttributeDbaseRecord.Schema.AANTAL.Length));
            Customizations.Add(new LimitedLengthIntegerBuilder<RoadSegmentWidthProperties>(width => width.Width, RoadSegmentDynamicWidthAttributeDbaseRecord.Schema.BREEDTE.Length));
            Customizations.Add(new LimitedLengthStringBuilder<RoadSegmentEuropeanRoadProperties>(road => road.RoadNumber, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema.EUNUMMER.Length));
            Customizations.Add(new LimitedLengthStringBuilder<RoadSegmentNationalRoadProperties>(road => road.Ident2, RoadSegmentNationalRoadAttributeDbaseRecord.Schema.IDENT2.Length));
            Customizations.Add(new LimitedLengthStringBuilder<RoadSegmentNumberedRoadProperties>(road => road.Ident8, RoadSegmentNumberedRoadAttributeDbaseRecord.Schema.IDENT8.Length));
            Customizations.Add(new LimitedLengthStringBuilder<ImportedOrganization>(organization => organization.Code, OrganizationDbaseRecord.Schema.ORG.Length));
            Customizations.Add(new LimitedLengthStringBuilder<ImportedOrganization>(organization => organization.Name, OrganizationDbaseRecord.Schema.LBLORG.Length));

            Customize<MeasuredPoint>(customization => 
			    customization.FromFactory(
                    generator => new MeasuredPoint(
                        this.Create<double>(),
                        this.Create<double>(),
                        this.Create<double>(),
                        this.Create<double>()
                    )
                ).OmitAutoProperties()
            );

            Customize<ILineString>(customization => 
			    customization.FromFactory(
                    generator => new LineString(
                        new PointSequence(this.CreateMany<MeasuredPoint>(generator.Next(2,10))),
                        GeometryConfiguration.GeometryFactory
                    )
                ).OmitAutoProperties()
            );

            Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(this
                        .CreateMany<ILineString>(generator.Next(1,10))
                        .ToArray())
                ).OmitAutoProperties()
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
