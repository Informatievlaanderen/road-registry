namespace RoadRegistry.Syndication.ProjectionHost.Tests.Projections;

using Mapping;
using Syndication.Projections.MunicipalityEvents;

public class EventSerializerMappingTests
{
    [Fact]
    public void Has_serializers_for_municipality_events()
    {
        var eventSerializerMapping = EventSerializerMapping.CreateForNamespaceOf(typeof(MunicipalityWasRegistered));

        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasRegistered)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasNamed)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNameWasCorrectedToCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNisCodeWasDefined)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityNisCodeWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityBecameCurrent)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasCorrectedToCurrent)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasRetired)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(MunicipalityWasCorrectedToRetired)));
    }
}
