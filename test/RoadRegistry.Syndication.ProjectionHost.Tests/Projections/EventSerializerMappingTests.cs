namespace RoadRegistry.Syndication.ProjectionHost.Tests.Projections;

using Mapping;
using Syndication.Projections.MunicipalityEvents;
using Syndication.Projections.StreetNameEvents;

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

    [Fact]
    public void Has_serializers_for_street_name_events()
    {
        var eventSerializerMapping = EventSerializerMapping.CreateForNamespaceOf(typeof(StreetNameWasRegistered));

        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasRegistered)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNamePersistentLocalIdentifierWasAssigned)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasNamed)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameNameWasCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameNameWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameNameWasCorrectedToCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameBecameCurrent)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasCorrectedToCurrent)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasProposed)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasCorrectedToProposed)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasRetired)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasCorrectedToRetired)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameStatusWasRemoved)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameStatusWasCorrectedToRemoved)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameBecameComplete)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameBecameIncomplete)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameWasRemoved)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameHomonymAdditionWasCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameHomonymAdditionWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameHomonymAdditionWasCorrectedToCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameHomonymAdditionWasDefined)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNamePrimaryLanguageWasCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNamePrimaryLanguageWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNamePrimaryLanguageWasCorrectedToCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNamePrimaryLanguageWasDefined)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameSecondaryLanguageWasCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameSecondaryLanguageWasCorrected)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameSecondaryLanguageWasCorrectedToCleared)));
        Assert.True(eventSerializerMapping.HasSerializerFor(nameof(StreetNameSecondaryLanguageWasDefined)));
    }
}