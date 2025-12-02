namespace RoadRegistry.MartenMigration.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extensions;
using GradeSeparatedJunction;
using Infrastructure.MartenDb;
using Marten;
using NodaTime;
using NodaTime.Text;
using RoadNetwork;
using RoadNetwork.Events;
using RoadNode;
using RoadSegment;
using RoadSegment.ValueObjects;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;

public class MartenMigrationProjection : ConnectedProjection<MartenMigrationContext>
{
    private readonly MigrationRoadNetworkRepository _repo;

    public MartenMigrationProjection(MigrationRoadNetworkRepository repo)
    {
        _repo = repo.ThrowIfNull();

        When<Envelope<ImportedRoadNode>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                var roadNodeId = new RoadNodeId(envelope.Message.Id);
                var type = RoadNodeType.Parse(envelope.Message.Type);
                var point = GeometryTranslator.Translate(envelope.Message.Geometry);
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var roadNode = RoadNode.Create(new RoadRegistry.RoadNode.Events.RoadNodeAdded
                {
                    RoadNodeId = roadNodeId,
                    OriginalId = null,
                    Type = type,
                    Geometry = point.ToGeometryObject(),
                    Provenance = new ProvenanceData(provenance)
                });

                SaveAggregateAndLegacyEvent(session, roadNode, envelope, envelope.Message, eventIdentifier, provenance);
            }, token);
        });

        When<Envelope<ImportedRoadSegment>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                var roadSegmentId = new RoadSegmentId(envelope.Message.Id);
                var geometry = GeometryTranslator.Translate(envelope.Message.Geometry);
                var status = RoadSegmentStatus.Parse(envelope.Message.Status);
                var morphology = RoadSegmentMorphology.Parse(envelope.Message.Morphology);
                var category = RoadSegmentCategory.Parse(envelope.Message.Category);
                var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod);
                var accessRestriction = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction);
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.RecordingDate),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var roadSegment = RoadSegment.Create(new RoadRegistry.RoadSegment.Events.RoadSegmentAdded
                {
                    RoadSegmentId = roadSegmentId,
                    OriginalId = null,
                    Geometry = geometry.ToGeometryObject(),
                    StartNodeId = new RoadNodeId(envelope.Message.StartNodeId),
                    EndNodeId = new RoadNodeId(envelope.Message.EndNodeId),
                    GeometryDrawMethod = geometryDrawMethod,
                    AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(accessRestriction),
                    Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(category),
                    Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(morphology),
                    Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(status),
                    StreetNameId = BuildStreetNameIdAttributes(envelope.Message.LeftSide.StreetNameId, envelope.Message.RightSide.StreetNameId),
                    MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(envelope.Message.MaintenanceAuthority.Code)),
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(envelope.Message.Surfaces
                        .Select(x => (
                            new RoadSegmentPosition(x.FromPosition),
                            new RoadSegmentPosition(x.ToPosition),
                            RoadSegmentAttributeSide.Both,
                            RoadSegmentSurfaceType.Parse(x.Type)))
                    ),
                    EuropeanRoadNumbers = envelope.Message.PartOfEuropeanRoads
                        .Select(x => EuropeanRoadNumber.Parse(x.Number))
                        .ToArray(),
                    NationalRoadNumbers = envelope.Message.PartOfNationalRoads
                        .Select(x => NationalRoadNumber.Parse(x.Number))
                        .ToArray(),
                    Provenance = new ProvenanceData(provenance)
                });

                SaveAggregateAndLegacyEvent(session, roadSegment, envelope, envelope.Message, eventIdentifier, provenance);
            }, token);
        });

        When<Envelope<ImportedGradeSeparatedJunction>>((_, envelope, token) =>
        {
            var eventIdentifier = BuildEventIdentifier(envelope);

            return _repo.InIdempotentSession(eventIdentifier, session =>
            {
                var junctionId = new GradeSeparatedJunctionId(envelope.Message.Id);
                var type = GradeSeparatedJunctionType.Parse(envelope.Message.Type);
                var provenance = new Provenance(
                    Instant.FromDateTimeUtc(envelope.Message.Origin.Since),
                    Application.RoadRegistry,
                    new Reason("Import"),
                    new Operator(envelope.Message.Origin.OrganizationId),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen);

                var junction = GradeSeparatedJunction.Create(new RoadRegistry.GradeSeparatedJunction.Events.GradeSeparatedJunctionAdded
                {
                    GradeSeparatedJunctionId = junctionId,
                    OriginalId = null,
                    Type = type,
                    LowerRoadSegmentId = new RoadSegmentId(envelope.Message.LowerRoadSegmentId),
                    UpperRoadSegmentId = new RoadSegmentId(envelope.Message.UpperRoadSegmentId),
                    Provenance = new ProvenanceData(provenance)
                });
                SaveAggregateAndLegacyEvent(session, junction, envelope, envelope.Message, eventIdentifier, provenance);
            }, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (_, envelope, token) =>
        {
            foreach (var (message, changeIndex) in envelope.Message.Changes.Select((x, i) => (x.Flatten(), i)))
                switch (message)
                {
                    case RoadNodeAdded change:
                        await AddRoadNode(change, changeIndex, envelope, token);
                        break;
                    case RoadNodeModified change:
                        await ModifyRoadNode(change, changeIndex, envelope, token);
                        break;
                    case RoadNodeRemoved change:
                        await RemoveRoadNode(change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAdded change:
                        await AddRoadSegment(change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentModified change:
                        await ModifyRoadSegment(change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToEuropeanRoad change:
                        await AddRoadSegmentToEuropeanRoad(change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad change:
                        await RemoveRoadSegmentFromEuropeanRoad(change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToNationalRoad change:
                        await AddRoadSegmentToNationalRoad(change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad change:
                        await RemoveRoadSegmentFromNationalRoad(change, changeIndex, envelope, token);
                        break;

                    case RoadSegmentAddedToNumberedRoad change:
                        // only store legacy event
                        await ModifyRoadSegment(new RoadSegmentId(change.SegmentId), (_, _) => { }, envelope, change, changeIndex, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad change:
                        // only store legacy event
                        await ModifyRoadSegment(new RoadSegmentId(change.SegmentId), (_, _) => { }, envelope, change, changeIndex, token);
                        break;

                    case RoadSegmentAttributesModified change:
                        await ModifyRoadSegmentAttributes(change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentGeometryModified change:
                        await ModifyRoadSegmentGeometry(change, changeIndex, envelope, token);
                        break;
                    case RoadSegmentRemoved change:
                        await RemoveRoadSegment(change, changeIndex, envelope, token);
                        break;
                    case OutlinedRoadSegmentRemoved change:
                        // only store legacy event, used for changing segment from outlined to measured
                        await ModifyRoadSegment(new RoadSegmentId(change.Id), (_, _) => { }, envelope, change, changeIndex, token);
                        break;

                    case GradeSeparatedJunctionAdded change:
                        await AddGradeSeparatedJunction(change, changeIndex, envelope, token);
                        break;
                    case GradeSeparatedJunctionModified:
                        throw new InvalidOperationException("Change GradeSeparatedJunctionModified should not be in use");
                    case GradeSeparatedJunctionRemoved change:
                        await RemoveGradeSeparatedJunction(change, changeIndex, envelope, token);
                        break;

                    default:
                        throw new NotImplementedException($"Unknown change type {message.GetType()}");
                }
        });

        When<Envelope<RoadSegmentsStreetNamesChanged>>(async (_, envelope, token) =>
        {
            await RoadSegmentsStreetNamesChanged(envelope, token);
        });
    }

    private Task AddRoadNode(
        RoadNodeAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            var roadNodeId = new RoadNodeId(change.Id);
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadNode = RoadNode.Create(new RoadRegistry.RoadNode.Events.RoadNodeAdded
            {
                RoadNodeId = roadNodeId,
                OriginalId = new RoadNodeId(change.OriginalId ?? change.TemporaryId),
                Type = RoadNodeType.Parse(change.Type),
                Geometry = GeometryTranslator.Translate(change.Geometry).ToGeometryObject(),
                Provenance = new ProvenanceData(provenance)
            });
            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, roadNode, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private Task ModifyRoadNode(
        RoadNodeModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
            {
                var roadNodeId = new RoadNodeId(change.Id);

                var roadNode = await session.LoadAsync(roadNodeId);
                if (roadNode is null)
                {
                    throw new InvalidOperationException($"RoadNode with id {roadNodeId} is not found");
                }

                var provenance = BuildProvenance(envelope, Modification.Update);

                roadNode.Apply(new RoadRegistry.RoadNode.Events.RoadNodeModified
                {
                    RoadNodeId = roadNodeId,
                    Type = RoadNodeType.Parse(change.Type),
                    Geometry = GeometryTranslator.Translate(change.Geometry).ToGeometryObject(),
                    Provenance = new ProvenanceData(provenance)
                });

                var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
                SaveAggregateAndLegacyEvent(session, roadNode, envelope, change, eventIdentifier, provenance, downloadId);
            },
            token);
    }

    private Task RemoveRoadNode(
        RoadNodeRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            var roadNodeId = new RoadNodeId(change.Id);

            var roadNode = await session.LoadAsync(roadNodeId);
            if (roadNode is null)
            {
                throw new InvalidOperationException($"RoadNode with id {roadNodeId} is not found");
            }

            var provenance = BuildProvenance(envelope, Modification.Delete);

            roadNode.Apply(new RoadRegistry.RoadNode.Events.RoadNodeRemoved
            {
                RoadNodeId = roadNodeId,
                Provenance = new  ProvenanceData(provenance)
            });

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, roadNode, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private Task AddRoadSegment(
        RoadSegmentAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            var roadSegmentId = new RoadSegmentId(change.Id);

            var roadSegment = await session.LoadAsync(roadSegmentId);

            var geometry = GeometryTranslator.Translate(change.Geometry);
            var status = RoadSegmentStatus.Parse(change.Status);
            var morphology = RoadSegmentMorphology.Parse(change.Morphology);
            var category = RoadSegmentCategory.Parse(change.Category);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(change.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(change.AccessRestriction);
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var roadSegmentEvent = new RoadRegistry.RoadSegment.Events.RoadSegmentAdded
            {
                RoadSegmentId = roadSegmentId,
                OriginalId = new RoadSegmentId(change.OriginalId ?? change.TemporaryId),
                Geometry = geometry.ToGeometryObject(),
                StartNodeId = new RoadNodeId(change.StartNodeId),
                EndNodeId = new RoadNodeId(change.EndNodeId),
                GeometryDrawMethod = geometryDrawMethod,
                AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(accessRestriction),
                Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(category),
                Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(morphology),
                Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(status),
                StreetNameId = BuildStreetNameIdAttributes(change.LeftSide.StreetNameId, change.RightSide.StreetNameId),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(change.MaintenanceAuthority.Code)),
                SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(change.Surfaces
                    .Select(x => (
                        new RoadSegmentPosition(x.FromPosition),
                        new RoadSegmentPosition(x.ToPosition),
                        RoadSegmentAttributeSide.Both,
                        RoadSegmentSurfaceType.Parse(x.Type)))
                ),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = [],
                Provenance = new ProvenanceData(provenance)
            };

            if (roadSegment is null)
            {
                roadSegment = RoadSegment.Create(roadSegmentEvent);
            }
            else
            {
                roadSegment.Apply(roadSegmentEvent);
            }

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, roadSegment, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private async Task ModifyRoadSegment(
        RoadSegmentModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentModified
                {
                    RoadSegmentId = roadSegmentId,
                    OriginalId = change.OriginalId is not null
                        ? new RoadSegmentId(change.OriginalId.Value)
                        : null,
                    Geometry = GeometryTranslator.Translate(change.Geometry).ToGeometryObject(),
                    StartNodeId = new RoadNodeId(change.StartNodeId),
                    EndNodeId = new RoadNodeId(change.EndNodeId),
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(change.GeometryDrawMethod),
                    AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.Parse(change.AccessRestriction)),
                    Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.Parse(change.Category)),
                    Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.Parse(change.Morphology)),
                    Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.Parse(change.Status)),
                    StreetNameId = BuildStreetNameIdAttributes(change.LeftSide.StreetNameId, change.RightSide.StreetNameId),
                    MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(change.MaintenanceAuthority.Code)),
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(change.Surfaces
                        .Select(x => (
                            new RoadSegmentPosition(x.FromPosition),
                            new RoadSegmentPosition(x.ToPosition),
                            RoadSegmentAttributeSide.Both,
                            RoadSegmentSurfaceType.Parse(x.Type)))
                    ),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task AddRoadSegmentToEuropeanRoad(
        RoadSegmentAddedToEuropeanRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentAddedToEuropeanRoad
                {
                    RoadSegmentId = roadSegmentId,
                    Number = EuropeanRoadNumber.Parse(change.Number),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task RemoveRoadSegmentFromEuropeanRoad(
        RoadSegmentRemovedFromEuropeanRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentRemovedFromEuropeanRoad
                {
                    RoadSegmentId = roadSegmentId,
                    Number = EuropeanRoadNumber.Parse(change.Number),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task AddRoadSegmentToNationalRoad(
        RoadSegmentAddedToNationalRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentAddedToNationalRoad
                {
                    RoadSegmentId = roadSegmentId,
                    Number = NationalRoadNumber.Parse(change.Number),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task RemoveRoadSegmentFromNationalRoad(
        RoadSegmentRemovedFromNationalRoad change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.SegmentId);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentRemovedFromNationalRoad
                {
                    RoadSegmentId = roadSegmentId,
                    Number = NationalRoadNumber.Parse(change.Number),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task ModifyRoadSegmentAttributes(
        RoadSegmentAttributesModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentModified
                {
                    RoadSegmentId = roadSegmentId,
                    AccessRestriction = change.AccessRestriction is not null
                        ? new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.Parse(change.AccessRestriction))
                        : null,
                    Category = change.Category is not null
                        ? new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.Parse(change.Category))
                        : null,
                    Morphology = change.Morphology is not null
                        ? new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.Parse(change.Morphology))
                        : null,
                    Status = change.Status is not null
                        ? new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.Parse(change.Status))
                        : null,
                    StreetNameId = change.LeftSide is not null || change.RightSide is not null
                        ? BuildStreetNameIdAttributes(
                            change.LeftSide?.StreetNameId ?? GetValue(segment.Attributes.StreetNameId, RoadSegmentAttributeSide.Left),
                            change.RightSide?.StreetNameId ?? GetValue(segment.Attributes.StreetNameId, RoadSegmentAttributeSide.Right))
                        : null,
                    MaintenanceAuthorityId = change.MaintenanceAuthority is not null
                        ? new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(change.MaintenanceAuthority.Code))
                        : null,
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private async Task ModifyRoadSegmentGeometry(
        RoadSegmentGeometryModified change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);

        await ModifyRoadSegment(roadSegmentId,
            (segment, provenance) =>
            {
                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentModified
                {
                    RoadSegmentId = roadSegmentId,
                    Geometry = GeometryTranslator.Translate(change.Geometry).ToGeometryObject(),
                    Provenance = new ProvenanceData(provenance)
                });
            },
            envelope, change, changeIndex, token);
    }

    private Task RemoveRoadSegment(
        RoadSegmentRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var roadSegmentId = new RoadSegmentId(change.Id);
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            var roadSegment = await session.LoadAsync(roadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"RoadSegment with id {roadSegmentId} is not found");
            }

            var provenance = BuildProvenance(envelope, Modification.Delete);

            roadSegment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentRemoved
            {
                RoadSegmentId = roadSegmentId,
                Provenance = new ProvenanceData(provenance)
            });

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, roadSegment, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private Task AddGradeSeparatedJunction(
        GradeSeparatedJunctionAdded change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, session =>
        {
            var gradeSeparatedJunctionId = new GradeSeparatedJunctionId(change.Id);
            var provenance = BuildProvenance(envelope, Modification.Insert);

            var junction = GradeSeparatedJunction.Create(new RoadRegistry.GradeSeparatedJunction.Events.GradeSeparatedJunctionAdded
            {
                GradeSeparatedJunctionId = gradeSeparatedJunctionId,
                OriginalId = new GradeSeparatedJunctionId(change.TemporaryId),
                Type = GradeSeparatedJunctionType.Parse(change.Type),
                UpperRoadSegmentId = new RoadSegmentId(change.UpperRoadSegmentId),
                LowerRoadSegmentId = new RoadSegmentId(change.LowerRoadSegmentId),
                Provenance = new ProvenanceData(provenance)
            });

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, junction, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private Task RemoveGradeSeparatedJunction(
        GradeSeparatedJunctionRemoved change,
        int changeIndex,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            var gradeSeparatedJunctionId = new GradeSeparatedJunctionId(change.Id);

            var junction = await session.LoadAsync(gradeSeparatedJunctionId);
            if (junction is null)
            {
                throw new InvalidOperationException($"GradeSeparatedJunction with id {gradeSeparatedJunctionId} is not found");
            }

            var provenance = BuildProvenance(envelope, Modification.Delete);

            junction.Apply(new RoadRegistry.GradeSeparatedJunction.Events.GradeSeparatedJunctionRemoved
            {
                GradeSeparatedJunctionId = gradeSeparatedJunctionId,
                Provenance = new  ProvenanceData(provenance)
            });

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, junction, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private async Task RoadSegmentsStreetNamesChanged(
        Envelope<RoadSegmentsStreetNamesChanged> envelope,
        CancellationToken token)
    {
        foreach (var (change, index) in envelope.Message.RoadSegments.Select((x, i) => (x, i)))
        {
            var eventIdentifier = BuildEventIdentifier(envelope, index);

            await _repo.InIdempotentSession(eventIdentifier, async session =>
            {
                var roadSegmentId = new RoadSegmentId(change.Id);

                var segment = await session.LoadAsync(roadSegmentId);
                if (segment is null)
                {
                    return;
                }

                var provenance = new Provenance(
                    LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    Application.RoadRegistry,
                    new Reason(envelope.Message.Reason),
                    new Operator(string.Empty),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen);

                segment.Apply(new RoadRegistry.RoadSegment.Events.RoadSegmentModified
                {
                    RoadSegmentId = roadSegmentId,
                    StreetNameId = BuildStreetNameIdAttributes(
                        change.LeftSideStreetNameId ?? GetValue(segment.Attributes.StreetNameId, RoadSegmentAttributeSide.Left),
                        change.RightSideStreetNameId ?? GetValue(segment.Attributes.StreetNameId, RoadSegmentAttributeSide.Right)),
                    Provenance = new ProvenanceData(provenance)
                });

                SaveAggregateAndLegacyEvent(session, segment, envelope, change, eventIdentifier, provenance);
            }, token);
        }
    }

    private Task ModifyRoadSegment(RoadSegmentId roadSegmentId, Action<RoadSegment, Provenance> modify, Envelope<RoadNetworkChangesAccepted> envelope, object change, int changeIndex, CancellationToken token)
    {
        var eventIdentifier = BuildEventIdentifier(envelope, changeIndex);

        return _repo.InIdempotentSession(eventIdentifier, async session =>
        {
            var roadSegment = await session.LoadAsync(roadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"RoadSegment with id {roadSegmentId} is not found");
            }

            var provenance = BuildProvenance(envelope, Modification.Update);

            modify(roadSegment, provenance);

            var downloadId = DownloadId.FromValue(envelope.Message.DownloadId);
            SaveAggregateAndLegacyEvent(session, roadSegment, envelope, change, eventIdentifier, provenance, downloadId);
        }, token);
    }

    private static Provenance BuildProvenance(Envelope<RoadNetworkChangesAccepted> envelope, Modification modification)
    {
        return new Provenance(
            LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            Application.RoadRegistry,
            new Reason(envelope.Message.Reason),
            new Operator(envelope.Message.OrganizationId),
            modification,
            Organisation.DigitaalVlaanderen);
    }

    private static string BuildEventIdentifier<TMessage>(Envelope<TMessage> envelope, int? changeIndex = null)
        where TMessage : IMessage
    {
        return changeIndex is not null
            ? $"{envelope.Position}-{changeIndex}"
            : envelope.Position.ToString();
    }

    private void SaveAggregateAndLegacyEvent<TMessage>(IDocumentSession session, IMartenAggregateRootEntity aggregate, Envelope<TMessage> envelope, object legacyEvent, string legacyEventIdentifier, Provenance provenance, DownloadId? downloadId = null)
        where TMessage : IMessage
    {
        var roadNetwork = aggregate switch
        {
            RoadNode roadNode => new RoadNetwork([roadNode], [], []),
            RoadSegment roadSegment => new RoadNetwork([], [roadSegment], []),
            GradeSeparatedJunction junction => new RoadNetwork([], [], [junction]),
            _ => throw new NotImplementedException($"Unknown aggregate type {aggregate.GetType()}"),
        };
        roadNetwork.Apply(new RoadNetworkChanged
        {
            DownloadId = downloadId,
            Provenance = new ProvenanceData(provenance)
        });

        session.SetHeader("LegacyEvent", legacyEvent);

        var causationId = $"migration-{envelope.EventName}-{legacyEventIdentifier}";
        _repo.AddChangesToSession(session, roadNetwork, causationId);
    }

    private static RoadSegmentDynamicAttributeValues<StreetNameLocalId> BuildStreetNameIdAttributes(int? leftSideStreetNameId, int? rightSideStreetNameId)
    {
        if (leftSideStreetNameId is null && rightSideStreetNameId is null)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(StreetNameLocalId.NotApplicable);
        }

        if (leftSideStreetNameId == rightSideStreetNameId)
        {
            return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(new StreetNameLocalId(leftSideStreetNameId!.Value));
        }

        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(null, null, RoadSegmentAttributeSide.Left, StreetNameLocalId.FromValue(leftSideStreetNameId) ?? StreetNameLocalId.NotApplicable)
            .Add(null, null, RoadSegmentAttributeSide.Right, StreetNameLocalId.FromValue(rightSideStreetNameId) ?? StreetNameLocalId.NotApplicable);
    }

    private static StreetNameLocalId GetValue(RoadSegmentDynamicAttributeValues<StreetNameLocalId> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Right).Value,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }
}

public static class LocalDateTimeTranslator
{
    private static readonly DateTimeZone LocalTimeZone =
        DateTimeZoneProviders.Tzdb["Europe/Brussels"];

    public static Instant TranslateFromWhen(string value)
    {
        return Instant.FromDateTimeOffset(new ZonedDateTime(InstantPattern.ExtendedIso.Parse(value).Value, LocalTimeZone)
            .ToDateTimeOffset());
    }
}
