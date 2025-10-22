namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.RoadSegments;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using RoadSegment.ValueObjects;

public class RoadSegmentVersionRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentVersionRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeEuropeanRoadNumber();
        _fixture.CustomizeNationalRoadNumber();
        _fixture.CustomizeNumberedRoadNumber();
        _fixture.CustomizeRoadSegmentNumberedRoadDirection();
        _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeRoadSegmentGeometryVersion();

        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentLaneAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeRoadNetworkChangesAccepted();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
        _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
        _fixture.CustomizeRoadSegmentAddedToNationalRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNationalRoad();
        _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
        _fixture.CustomizeRoadSegmentsStreetNamesChanged();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            return (object)BuildRoadSegmentRecord(message, change.RoadSegmentAdded, _ => { });
        });

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_european_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_national_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToNationalRoad = change.RoadSegmentAddedToNationalRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_numbered_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToNumberedRoad = change.RoadSegmentAddedToNumberedRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToNumberedRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_european_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromEuropeanRoad = change.RoadSegmentRemovedFromEuropeanRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromEuropeanRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_national_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromNationalRoad = change.RoadSegmentRemovedFromNationalRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromNationalRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_numbered_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromNumberedRoad = change.RoadSegmentRemovedFromNumberedRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromNumberedRoad.SegmentVersion!.Value;
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentAttributesModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAttributesModified.Changes, change =>
        {
            var roadSegmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var roadSegmentAttributesModified = change.RoadSegmentAttributesModified;

            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;

            return (object)new RoadSegmentVersionRecord
            {
                StreamId = "roadnetwork",
                Id = roadSegmentAttributesModified.Id,
                Method = geometryDrawMethodTranslation.Identifier,
                Version = roadSegmentAttributesModified.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
            };
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentGeometryModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, change =>
        {
            var roadSegmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var roadSegmentGeometryModified = change.RoadSegmentGeometryModified;

            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;

            return (object)new RoadSegmentVersionRecord
            {
                StreamId = "roadnetwork",
                Id = roadSegmentGeometryModified.Id,
                Method = geometryDrawMethodTranslation.Identifier,
                Version = roadSegmentGeometryModified.Version,
                GeometryVersion = roadSegmentGeometryModified.GeometryVersion,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
            };
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var roadSegmentModified = change.RoadSegmentModified;

            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation;

            return (object)new RoadSegmentVersionRecord
            {
                StreamId = "roadnetwork",
                Id = roadSegmentModified.Id,
                Method = geometryDrawMethodTranslation.Identifier,
                Version = roadSegmentModified.Version,
                GeometryVersion = roadSegmentModified.GeometryVersion,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
            };
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            acceptedRoadSegmentRemoved
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var roadSegmentAdded = change.RoadSegmentAdded;

            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;

            return (object)new RoadSegmentVersionRecord
            {
                StreamId = "roadnetwork",
                Id = roadSegmentAdded.Id,
                Version = roadSegmentAdded.Version,
                Method = geometryDrawMethodTranslation.Identifier,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                IsRemoved = true
            };
        });

        return BuildProjection()
            .Scenario()
            .Given(messages)
            .ExpectWhileIgnoringQueryFilters(expectedRecords);
    }

    [Fact]
    public Task WhenRoadSegmentsStreetNamesChanged()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var roadSegmentsStreetNamesChanged = _fixture.Create<RoadSegmentsStreetNamesChanged>();

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            roadSegmentsStreetNamesChanged
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var roadSegmentAdded = change.RoadSegmentAdded;

            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;

            return (object)new RoadSegmentVersionRecord
            {
                StreamId = "roadnetwork",
                Id = roadSegmentAdded.Id,
                Version = roadSegmentsStreetNamesChanged.RoadSegments.Single().Version,
                Method = geometryDrawMethodTranslation.Identifier,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                IsRemoved = false
            };
        });

        return BuildProjection()
            .Scenario()
            .Given(messages)
            .ExpectWhileIgnoringQueryFilters(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_are_imported()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(importedRoadSegment =>
            {
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation;

                var expected = new RoadSegmentVersionRecord
                {
                    StreamId = "roadnetwork",
                    Id = importedRoadSegment.Id,
                    Method = geometryDrawMethodTranslation.Identifier,
                    Version = importedRoadSegment.Version,
                    GeometryVersion = importedRoadSegment.GeometryVersion,
                    RecordingDate = importedRoadSegment.RecordingDate
                };
                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    private RoadSegmentVersionRecord BuildRoadSegmentRecord(
        RoadNetworkChangesAccepted acceptedRoadSegmentAdded,
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentVersionRecord> changeRecord)
    {
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;

        var record = new RoadSegmentVersionRecord
        {
            StreamId = "roadnetwork",
            Id = roadSegmentAdded.Id,
            Method = geometryDrawMethodTranslation.Identifier,
            Version = roadSegmentAdded.Version,
            GeometryVersion = roadSegmentAdded.GeometryVersion,
            RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
        };
        changeRecord(record);

        return record;
    }

    private RoadSegmentVersionRecordProjection BuildProjection()
    {
        return new RoadSegmentVersionRecordProjection(new NullLogger<RoadSegmentVersionRecordProjection>());
    }
}
