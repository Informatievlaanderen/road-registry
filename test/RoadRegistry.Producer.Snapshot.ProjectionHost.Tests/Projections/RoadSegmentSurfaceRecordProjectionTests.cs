namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Extensions;
using Moq;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;
using RoadSegmentSurface;
using Shared;

public class RoadSegmentSurfaceRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentSurfaceRecordProjectionTests(ProjectionTestServices services)
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
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeRoadSegmentSurfaceAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    private static Mock<IKafkaProducer> BuildKafkaProducer()
    {
        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadSegmentSurfaceSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadSegmentSurfaceSnapshot>.Success(It.IsAny<RoadSegmentSurfaceSnapshot>()));
        return kafkaProducer;
    }

    private static ICollection<object> ConvertToRoadSegmentSurfaceRecords(RoadNetworkChangesAccepted message, DateTimeOffset created, Action<RoadSegmentSurfaceRecord> modifier = null)
    {
        return Array.ConvertAll(message.Changes, change =>
            {
                var surfaces = change.RoadSegmentModified?.Surfaces
                               ?? change.RoadSegmentAttributesModified?.Surfaces
                               ?? change.RoadSegmentGeometryModified?.Surfaces
                               ?? change.RoadSegmentAdded.Surfaces;
                var segmentId = change.RoadSegmentModified?.Id
                                ?? change.RoadSegmentAttributesModified?.Id
                                ?? change.RoadSegmentGeometryModified?.Id
                                ?? change.RoadSegmentAdded.Id;

                return surfaces.Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;

                    var roadSegmentSurfaceRecord = new RoadSegmentSurfaceRecord(
                        surface.AttributeId,
                        segmentId,
                        surface.AsOfGeometryVersion,
                        typeTranslation.Identifier,
                        typeTranslation.Name,
                        (double)surface.FromPosition,
                        (double)surface.ToPosition,
                        message.ToOrigin(),
                        created
                    );
                    modifier?.Invoke(roadSegmentSurfaceRecord);
                    return roadSegmentSurfaceRecord;
                });
            })
            .SelectMany(x => x)
            .ToArray();
    }

    private void KafkaVerify(Mock<IKafkaProducer> kafkaProducer, IEnumerable<object> expectedRecords, Times? times = null)
    {
        foreach (var expectedRecord in expectedRecords.Cast<RoadSegmentSurfaceRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadSegmentSurfaceSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }
    }

    [Fact]
    public async Task When_modifying_road_nodes_with_modified_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select(attributes =>
            {
                var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentSurfaceAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_nodes_with_removed_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created, record =>
        {
            record.Origin = acceptedRoadSegmentModified.ToOrigin();
            record.IsRemoved = true;
        });

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_nodes_with_some_added_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Append(_fixture.Create<RoadSegmentSurfaceAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_nodes_with_some_modified_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select((attributes, i) =>
            {
                if (i % 2 == 0)
                {
                    var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                    roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                    return roadSegmentSurfaceAttributes;
                }

                return attributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_nodes_with_some_removed_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Take(roadSegmentAdded.Surfaces.Length - 1)
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var modifiedSurfaceIds = roadSegmentModified.Surfaces.Select(x => x.AttributeId).ToArray();
        var removedSurfaceIds = roadSegmentAdded.Surfaces
            .Where(addedSurface => !modifiedSurfaceIds.Contains(addedSurface.AttributeId))
            .Select(x => x.AttributeId)
            .ToArray();

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created)
            .UnionBy(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created), x => ((RoadSegmentSurfaceRecord)x).Id)
            .Cast<RoadSegmentSurfaceRecord>()
            .Select(record =>
            {
                if (removedSurfaceIds.Contains(record.Id))
                {
                    record.Origin = acceptedRoadSegmentModified.ToOrigin();
                    record.IsRemoved = true;
                }

                return (object)record;
            })
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_segment_geometry_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentGeometryModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created, record =>
            {
                record.Origin = acceptedRoadSegmentGeometryModified.ToOrigin();
                record.IsRemoved = true;
            })
            .Concat(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentGeometryModified, created))
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_segments_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created, record =>
            {
                record.Origin = acceptedRoadSegmentModified.ToOrigin();
                record.IsRemoved = true;
            })
            .Concat(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created))
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_road_segment_attributes_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created, record =>
            {
                record.Origin = acceptedRoadSegmentModified.ToOrigin();
                record.IsRemoved = true;
            })
            .Concat(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified, created))
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_removing_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());

        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created, record =>
        {
            record.Origin = acceptedRoadSegmentRemoved.ToOrigin();
            record.IsRemoved = true;
        });

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_road_segment_surfaces_were_imported()
    {
        var created = DateTimeOffset.UtcNow;
        var random = new Random();

        var data = _fixture
            .CreateMany<ImportedRoadSegment>(new Random().Next(1, 100))
            .Select(@event =>
            {
                @event.Surfaces = _fixture
                    .CreateMany<ImportedRoadSegmentSurfaceAttribute>(random.Next(1, 10))
                    .ToArray();

                var expectedRecords = @event
                    .Surfaces
                    .Select(surface =>
                    {
                        var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;

                        return new RoadSegmentSurfaceRecord(
                            surface.AttributeId,
                            @event.Id,
                            surface.AsOfGeometryVersion,
                            typeTranslation.Identifier,
                            typeTranslation.Name,
                            (double)surface.FromPosition,
                            (double)surface.ToPosition,
                            surface.Origin.ToOrigin(),
                            created
                        );
                    });

                return new
                {
                    ImportedRoadSegment = @event,
                    ExpectedRecords = expectedRecords
                };
            })
            .ToList();
        var expectedRecords = data.SelectMany(x => x.ExpectedRecords).ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(x => x.ImportedRoadSegment))
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_adding_surfaces_which_were_previously_removed()
    {
        _fixture.Freeze<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;
        var surfaces = _fixture.CreateMany<RoadSegmentSurfaceAttributes>(2).ToArray();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
        acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.Surfaces = surfaces;

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());
        
        var expectedRecords = ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentAdded, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved, acceptedRoadSegmentAdded)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords, Times.Exactly(2));
    }

    [Fact]
    public async Task When_modifying_surfaces_which_were_previously_removed_from_another_roadsegment()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentId = _fixture.Create<RoadSegmentId>();

        var created = DateTimeOffset.UtcNow;
        var surfaces = _fixture.CreateMany<RoadSegmentSurfaceAttributes>(3).ToArray();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
        acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.Surfaces = surfaces;

        var acceptedRoadSegmentModified1 = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());
        acceptedRoadSegmentModified1.Changes.Single().RoadSegmentModified.Surfaces = surfaces.Take(surfaces.Length - 1).ToArray();
        
        var acceptedRoadSegmentModified2 = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());
        acceptedRoadSegmentModified2.Changes.Single().RoadSegmentModified.Id = roadSegmentId.Next();
        acceptedRoadSegmentModified2.Changes.Single().RoadSegmentModified.Surfaces.First().AttributeId = surfaces.Last().AttributeId;

        var expectedRecords = Array.Empty<object>()
            .Concat(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified1, created))
            .Concat(ConvertToRoadSegmentSurfaceRecords(acceptedRoadSegmentModified2, created))
            .ToArray();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified1, acceptedRoadSegmentModified2)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    //[Fact]
    //public async Task When_adding_road_segments_with_road_segment_surfaces()
    //{
    //    var message = _fixture
    //        .Create<RoadNetworkChangesAccepted>()
    //        .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToRoadSegmentSurface>());

    //    var created = DateTimeOffset.UtcNow;

    //    var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //    {
    //        var roadSegmentSurfaceAdded = change.RoadSegmentAddedToRoadSegmentSurface;

    //        return (object)new RoadSegmentSurfaceRecord(
    //            roadSegmentSurfaceAdded.AttributeId,
    //            roadSegmentSurfaceAdded.SegmentId,
    //            roadSegmentSurfaceAdded.Number,
    //            message.ToOrigin(),
    //            created);
    //    });

    //    var kafkaProducer = new Mock<IKafkaProducer>();
    //    kafkaProducer
    //        .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadSegmentSurfaceSnapshot>(), CancellationToken.None))
    //        .ReturnsAsync(Result<RoadSegmentSurfaceSnapshot>.Success(It.IsAny<RoadSegmentSurfaceSnapshot>()));

    //    await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
    //         .Scenario()
    //         .Given(message)
    //         .Expect(created.UtcDateTime, expectedRecords);

    //    foreach (var expectedRecord in expectedRecords.Cast<RoadSegmentSurfaceRecord>())
    //    {
    //        kafkaProducer.Verify(
    //            x => x.Produce(
    //                expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
    //                It.Is(expectedRecord.ToContract(), new RoadSegmentSurfaceSnapshotEqualityComparer()),
    //                It.IsAny<CancellationToken>()),
    //            Times.Once);
    //    }
    //}

    //[Fact]
    //public async Task When_removing_road_segments_from_road_segment_surfaces()
    //{
    //    var acceptedRoadSegmentSurfaceAdded = _fixture
    //        .Create<RoadNetworkChangesAccepted>()
    //        .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToRoadSegmentSurface>());

    //    var acceptedRoadSegmentSurfaceRemoved = _fixture
    //        .Create<RoadNetworkChangesAccepted>()
    //        .WithAcceptedChanges(
    //            acceptedRoadSegmentSurfaceAdded.Changes.Select(change =>
    //                new RoadSegmentRemovedFromRoadSegmentSurface
    //                {
    //                    AttributeId = change.RoadSegmentAddedToRoadSegmentSurface.AttributeId,
    //                    SegmentId = change.RoadSegmentAddedToRoadSegmentSurface.SegmentId,
    //                    Number = change.RoadSegmentAddedToRoadSegmentSurface.Number
    //                }));

    //    var created = DateTimeOffset.UtcNow;

    //    var expectedRecords = Array.ConvertAll(acceptedRoadSegmentSurfaceAdded.Changes, change =>
    //    {
    //        var roadSegmentSurfaceAdded = change.RoadSegmentAddedToRoadSegmentSurface;

    //        return (object)new RoadSegmentSurfaceRecord(
    //                roadSegmentSurfaceAdded.AttributeId,
    //                roadSegmentSurfaceAdded.SegmentId,
    //                roadSegmentSurfaceAdded.Number,
    //                acceptedRoadSegmentSurfaceAdded.ToOrigin(),
    //                created.AddDays(-1))
    //        { IsRemoved = true };
    //    });

    //    expectedRecords = Array.ConvertAll(acceptedRoadSegmentSurfaceRemoved.Changes, change =>
    //    {
    //        var roadSegmentSurfaceRemoved = change.RoadSegmentRemovedFromRoadSegmentSurface;

    //        var record = expectedRecords.Cast<RoadSegmentSurfaceRecord>().Single(x => x.Id == roadSegmentSurfaceRemoved.AttributeId);
    //        record.Origin = record.Origin
    //            .WithOrganization(acceptedRoadSegmentSurfaceRemoved.OrganizationId, acceptedRoadSegmentSurfaceRemoved.Organization);
    //        record.IsRemoved = true;
    //        record.LastChangedTimestamp = created;

    //        return (object)record;
    //    });

    //    var kafkaProducer = new Mock<IKafkaProducer>();
    //    kafkaProducer
    //        .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadSegmentSurfaceSnapshot>(), CancellationToken.None))
    //        .ReturnsAsync(Result<RoadSegmentSurfaceSnapshot>.Success(It.IsAny<RoadSegmentSurfaceSnapshot>()));

    //    await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
    //        .Scenario()
    //        .Given(acceptedRoadSegmentSurfaceAdded, acceptedRoadSegmentSurfaceRemoved)
    //        .Expect(created.UtcDateTime, expectedRecords);

    //    foreach (var expectedRecord in expectedRecords.Cast<RoadSegmentSurfaceRecord>())
    //    {
    //        kafkaProducer.Verify(
    //            x => x.Produce(
    //                expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
    //                It.Is(expectedRecord.ToContract(), new RoadSegmentSurfaceSnapshotEqualityComparer()),
    //                It.IsAny<CancellationToken>()),
    //            Times.Once);
    //    }
    //}

    //[Fact]
    //public async Task When_removing_road_segments_with_road_segment_surfaces()
    //{
    //    var acceptedRoadSegmentSurfaceAdded = _fixture
    //        .Create<RoadNetworkChangesAccepted>()
    //        .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToRoadSegmentSurface>());

    //    var acceptedRoadSegmentRemoved = _fixture
    //        .Create<RoadNetworkChangesAccepted>()
    //        .WithAcceptedChanges(
    //            acceptedRoadSegmentSurfaceAdded.Changes
    //                .Select(change => change.RoadSegmentAddedToRoadSegmentSurface.SegmentId)
    //                .Distinct()
    //                .Select(roadSegmentId =>
    //                new RoadSegmentRemoved
    //                {
    //                    Id = roadSegmentId
    //                }));

    //    var created = DateTimeOffset.UtcNow;

    //    var expectedRecords = Array.ConvertAll(acceptedRoadSegmentSurfaceAdded.Changes, change =>
    //    {
    //        var roadSegmentSurfaceAdded = change.RoadSegmentAddedToRoadSegmentSurface;

    //        return (object)new RoadSegmentSurfaceRecord(
    //                roadSegmentSurfaceAdded.AttributeId,
    //                roadSegmentSurfaceAdded.SegmentId,
    //                roadSegmentSurfaceAdded.Number,
    //                acceptedRoadSegmentSurfaceAdded.ToOrigin(),
    //                created.AddDays(-1))
    //        { IsRemoved = true };
    //    });

    //    expectedRecords = Array.ConvertAll(acceptedRoadSegmentRemoved.Changes, change =>
    //    {
    //        var roadSegmentRemoved = change.RoadSegmentRemoved;

    //        var record = expectedRecords.Cast<RoadSegmentSurfaceRecord>().Single(x => x.RoadSegmentId == roadSegmentRemoved.Id);
    //        record.Origin = record.Origin
    //            .WithOrganization(acceptedRoadSegmentRemoved.OrganizationId, acceptedRoadSegmentRemoved.Organization);
    //        record.IsRemoved = true;
    //        record.LastChangedTimestamp = created;

    //        return (object)record;
    //    });

    //    var kafkaProducer = new Mock<IKafkaProducer>();
    //    kafkaProducer
    //        .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadSegmentSurfaceSnapshot>(), CancellationToken.None))
    //        .ReturnsAsync(Result<RoadSegmentSurfaceSnapshot>.Success(It.IsAny<RoadSegmentSurfaceSnapshot>()));

    //    await new RoadSegmentSurfaceRecordProjection(kafkaProducer.Object)
    //        .Scenario()
    //        .Given(acceptedRoadSegmentSurfaceAdded, acceptedRoadSegmentRemoved)
    //        .Expect(created.UtcDateTime, expectedRecords);

    //    foreach (var expectedRecord in expectedRecords.Cast<RoadSegmentSurfaceRecord>())
    //    {
    //        kafkaProducer.Verify(
    //            x => x.Produce(
    //                expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
    //                It.Is(expectedRecord.ToContract(), new RoadSegmentSurfaceSnapshotEqualityComparer()),
    //                It.IsAny<CancellationToken>()),
    //            Times.Once);
    //    }
    //}
}
