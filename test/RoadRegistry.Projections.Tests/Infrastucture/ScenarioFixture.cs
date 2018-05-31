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

            Customizations
                .Add(new StringPropertyTruncateSpecimenBuilder<OriginProperties>(p => p.OrganizationId, RoadNodeDbaseRecord.Schema.BEGINORG.Length));
            Customizations
                .Add(new StringPropertyTruncateSpecimenBuilder<OriginProperties>(p => p.Organization, RoadNodeDbaseRecord.Schema.LBLBGINORG.Length));
        }
    }
}
