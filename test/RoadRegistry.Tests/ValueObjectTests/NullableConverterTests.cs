namespace RoadRegistry.Tests.ValueObjectTests;

using System.IO;
using System.Text;
using FluentAssertions;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ValueObjects;
using Xunit;

// An attribute-only edit emits RoadSegmentWasChanged with every untouched property left null. The scalar value
// object converters called Parse on whatever the reader held, so a null status threw ArgumentNullException while
// deserializing - and because Marten skipped serialization errors, the event was dead-lettered, the progression
// advanced, and all four read models silently missed the change. Observed on road segment 818746.
public class NullableConverterTests
{
    private static ISerializer Serializer()
    {
        var options = new StoreOptions();
        options.ConfigureSerializer();
        return options.Serializer();
    }

    private static T FromJson<T>(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return Serializer().FromJson<T>(stream);
    }

    // Verbatim from the event that was dead-lettered on test (sequence 4464936): one attribute set, every scalar null.
    // Back then the event was RoadSegmentWasModified and still carried a status and a geometry draw method; both are
    // kept in the JSON, so this also pins down that an event stored before they were dropped still reads. The node
    // identifiers below are the scalars that remain.
    private const string AttributeOnlyEdit = """
        {
          "status": null,
          "nodeIds": {"start": null, "end": null},
          "category": [{"value": "RegionaleWeg", "coverage": {"to": 76.88, "from": 0.0}}],
          "geometry": null,
          "endNodeId": null,
          "morphology": null,
          "provenance": {"reason": "", "operator": "OVO002949", "timestamp": "2026-08-03T15:30:03.3924599Z", "application": "RoadRegistry", "modification": "Unknown", "organisation": "Agiv"},
          "startNodeId": null,
          "surfaceType": null,
          "streetNameId": null,
          "roadSegmentId": 818746,
          "accessRestriction": null,
          "geometryDrawMethod": null,
          "carTrafficDirection": null,
          "bikeTrafficDirection": null,
          "maintenanceAuthorityId": null,
          "pedestrianTrafficDirection": null,
          "originalRoadSegmentIdReference": {"tempIds": null, "roadSegmentId": 818746}
        }
        """;

    [Fact]
    public void RoadSegmentWasChanged_WithOnlyOneAttributeSet_CanBeDeserialized()
    {
        var @event = FromJson<RoadSegmentWasChanged>(AttributeOnlyEdit);

        @event.NodeIds!.Start.Should().BeNull();
        @event.NodeIds.End.Should().BeNull();
        @event.Morphology.Should().BeNull();
        @event.SurfaceType.Should().BeNull();
        @event.AccessRestriction.Should().BeNull();
        @event.CarTrafficDirection.Should().BeNull();

        @event.RoadSegmentId.ToInt32().Should().Be(818746);
        @event.Category!.Values.Should().ContainSingle()
            .Which.Value.Should().Be(RoadSegmentCategoryV2.RegionaleWeg);
    }

    [Fact]
    public void TheNullScalarsSurviveAFullRoundTrip()
    {
        // The same converters are shared with the V1 events, so pin the write side too: a null must serialize back to
        // null and read again without throwing.
        var json = Serializer().ToJson(FromJson<RoadSegmentWasChanged>(AttributeOnlyEdit));

        json.Should().Contain("\"start\":null");

        var roundTripped = FromJson<RoadSegmentWasChanged>(json);

        roundTripped.NodeIds!.Start.Should().BeNull();
        roundTripped.Category!.Values.Should().ContainSingle()
            .Which.Value.Should().Be(RoadSegmentCategoryV2.RegionaleWeg);
    }
}
