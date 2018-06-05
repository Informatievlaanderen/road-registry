namespace RoadRegistry.Projections.Tests.Infrastucture
{
    using System;
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

            Customize<Events.Geometry>(customization =>
                customization.FromFactory<int>(value =>
                    new Events.Geometry
                    {
                        SpatialReferenceSystemIdentifier = value,
                        WellKnownBinary = this.Create<Point>().SerializeByteArray<WkbSerializer>()
                    }));

            LimitField<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length);
            LimitField<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length);

            LimitField<Organisation>(p => p.Id, RoadSegmentDbaseRecord.Schema.BEHEER.Length);
            LimitField<Organisation>(p => p.Name, RoadSegmentDbaseRecord.Schema.LBLBEHEER.Length);

        }

        private void LimitField<T>(Expression<Func<T, string>> field, int length)
        {
            Customizations.Add(new StringPropertyTruncateSpecimenBuilder<T>(field, length));
        }
    }
}
