namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
    using System.Linq.Expressions;
    using AutoFixture;
    using Events;
    using Wkx;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            Customize<Point>(customization =>
                customization.FromFactory<int>(value =>
                    new Point(new Random(value).NextDouble(), new Random(value).NextDouble())));

            Customize<MultiLineString>(customization =>
                customization.FromFactory<int>(value =>
                    new MultiLineString(this.CreateMany<LineString>(new Random(value).Next(10)))));

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

            LimitFieldLength<Organisation>(p => p.Id, RoadSegmentDbaseRecord.Schema.BEHEER.Length);
            LimitFieldLength<Organisation>(p => p.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length);
        }

        private void LimitFieldLength<T>(Expression<Func<T, string>> field, int length)
        {
            Customizations.Add(new StringPropertyTruncateSpecimenBuilder<T>(field, length));
        }
    }
}
