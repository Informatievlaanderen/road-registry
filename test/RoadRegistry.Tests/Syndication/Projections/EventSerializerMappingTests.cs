namespace RoadRegistry.Syndication.Projections
{
    using MunicipalityEvents;
    using ProjectionHost.Mapping;
    using Xunit;

    public class EventSerializerMappingTests
    {
        [Fact]
        public void Has_serializers()
        {
            var eventSerializerMapping = new EventSerializerMapping();

            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasRegistered)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasNamed)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCleared)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCorrected)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCorrectedToCleared)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNisCodeWasDefined)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNisCodeWasCorrected)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityBecameCurrent)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasCorrectedToCurrent)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityBecameRetired)));
            Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasCorrectedToRetired)));
        }
    }
}
