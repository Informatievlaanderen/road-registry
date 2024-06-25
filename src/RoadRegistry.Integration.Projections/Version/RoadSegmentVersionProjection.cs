namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Schema.RoadSegments;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;
//TODO-rik versioned:
/*
 * - copy of Version entity + add Position prop
 * - add Position to PK + add indices
 * - RoadSegmentVersionProjection handles all related data (lanes,surfaces,...)
 * - create new Version for each roadsegment change
 */
public class RoadSegmentVersionProjection : ConnectedProjection<IntegrationContext>
{
    public RoadSegmentVersionProjection()
    {
        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            var geometry =
                GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
            var statusTranslation = RoadSegmentStatus.Parse(envelope.Message.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(envelope.Message.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(envelope.Message.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction).Translation;

            await context.RoadSegmentVersions.AddAsync(
                new RoadSegmentVersion
                {
                    Position = envelope.Position,
                    Id = envelope.Message.Id,

                    StartNodeId = envelope.Message.StartNodeId,
                    EndNodeId = envelope.Message.EndNodeId,
                    Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),

                    Version = envelope.Message.Version,
                    GeometryVersion = envelope.Message.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    StatusLabel = statusTranslation.Name,
                    MorphologyId = morphologyTranslation.Identifier,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryId = categoryTranslation.Identifier,
                    CategoryLabel = categoryTranslation.Name,
                    LeftSideStreetNameId = envelope.Message.LeftSide.StreetNameId,
                    RightSideStreetNameId = envelope.Message.RightSide.StreetNameId,
                    MaintainerId = envelope.Message.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    CreatedOnTimestamp = new DateTimeOffset(envelope.Message.RecordingDate),
                    VersionTimestamp = new DateTimeOffset(envelope.Message.Origin.Since),
                    OrganizationId = envelope.Message.Origin.OrganizationId,
                    OrganizationName = envelope.Message.Origin.Organization
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape)),
                token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            var changesGroupedByRoadSegments = envelope.Message.Changes.Flatten()
                .Select(change =>
                {
                    var roadSegmentId = GetRoadSegmentIdFromChange(change);
                    return roadSegmentId is not null ? new RoadSegmentChange(roadSegmentId.Value, change) : null;
                })
                .Where(x => x is not null)
                .GroupBy(x => x.Id)
                .ToList();

            foreach (var changesGroupedByRoadSegment in changesGroupedByRoadSegments)
            {
                var roadSegmentId = changesGroupedByRoadSegment.Key;
                var roadSegmentChanges = changesGroupedByRoadSegment.Select(x => x.Change).ToList();

                RoadSegmentVersion roadSegment;
                if (roadSegmentChanges.OfType<RoadSegmentAdded>().Any())
                {
                    var roadSegmentAdded = roadSegmentChanges.OfType<RoadSegmentAdded>().Single();
                    roadSegment = await CreateFirstRoadSegmentVersion(roadSegmentAdded, envelope);
                    await context.RoadSegmentVersions.AddAsync(roadSegment, token);
                    roadSegmentChanges.Remove(roadSegmentAdded);
                }
                else
                {
                    roadSegment = await context.CreateNewRoadSegmentVersion(roadSegmentId,
                        envelope,
                        token);
                }

                foreach (var message in roadSegmentChanges)
                    switch (message)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            throw new InvalidOperationException($"Invalid RoadSegmentAdded change for ID {roadSegmentAdded.Id} for position {envelope.Position}");

                        case RoadSegmentModified roadSegmentModified:
                            await ModifyRoadSegment(roadSegment, roadSegmentModified, envelope);
                            break;

                        case RoadSegmentAddedToEuropeanRoad change:
                            await AddRoadSegmentToEuropeanRoad(roadSegment, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromEuropeanRoad change:
                            await RemoveRoadSegmentFromEuropeanRoad(roadSegment, change, envelope, token);
                            break;

                        case RoadSegmentAddedToNationalRoad change:
                            await AddRoadSegmentToNationalRoad(roadSegment, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromNationalRoad change:
                            await RemoveRoadSegmentFromNationalRoad(roadSegment, change, envelope, token);
                            break;

                        case RoadSegmentAddedToNumberedRoad change:
                            await AddRoadSegmentToNumberedRoad(roadSegment, change, envelope, token);
                            break;
                        case RoadSegmentRemovedFromNumberedRoad change:
                            await RemoveRoadSegmentFromNumberedRoad(roadSegment, change, envelope, token);
                            break;

                        case RoadSegmentAttributesModified roadSegmentAttributesModified:
                            await ModifyRoadSegmentAttributes(roadSegment, roadSegmentAttributesModified, envelope, token);
                            break;

                        case RoadSegmentGeometryModified roadSegmentGeometryModified:
                            await ModifyRoadSegmentGeometry(roadSegment, roadSegmentGeometryModified, envelope, token);
                            break;

                        case RoadSegmentRemoved roadSegmentRemoved:
                            await RemoveRoadSegment(roadSegment, roadSegmentRemoved, envelope, token);
                            break;
                    }
            }
        });
    }

    private static int? GetRoadSegmentIdFromChange(object change)
    {
        switch (change)
        {
            case RoadSegmentAdded roadSegmentAdded:
                return roadSegmentAdded.Id;

                //case RoadSegmentModified roadSegmentModified:
                //    await ModifyRoadSegment(eventContext, roadSegmentModified, envelope, token);
                //    break;

                //case RoadSegmentAddedToEuropeanRoad change:
                //    await AddRoadSegmentToEuropeanRoad(eventContext, change, envelope, token);
                //    break;
                //case RoadSegmentRemovedFromEuropeanRoad change:
                //    await RemoveRoadSegmentFromEuropeanRoad(eventContext, change, envelope, token);
                //    break;

                //case RoadSegmentAddedToNationalRoad change:
                //    await AddRoadSegmentToNationalRoad(eventContext, change, envelope, token);
                //    break;
                //case RoadSegmentRemovedFromNationalRoad change:
                //    await RemoveRoadSegmentFromNationalRoad(eventContext, change, envelope, token);
                //    break;

                //case RoadSegmentAddedToNumberedRoad change:
                //    await AddRoadSegmentToNumberedRoad(eventContext, change, envelope, token);
                //    break;
                //case RoadSegmentRemovedFromNumberedRoad change:
                //    await RemoveRoadSegmentFromNumberedRoad(eventContext, change, envelope, token);
                //    break;

                //case RoadSegmentAttributesModified roadSegmentAttributesModified:
                //    await ModifyRoadSegmentAttributes(eventContext, roadSegmentAttributesModified, envelope, token);
                //    break;

                //case RoadSegmentGeometryModified roadSegmentGeometryModified:
                //    await ModifyRoadSegmentGeometry(eventContext, roadSegmentGeometryModified, envelope, token);
                //    break;

                //case RoadSegmentRemoved roadSegmentRemoved:
                //    await RemoveRoadSegment(eventContext, roadSegmentRemoved, envelope, token);
                //    break;
        }

        throw new NotImplementedException($"{change.GetType().Name}");
    }

    private sealed record RoadSegmentChange(int Id, object Change);

    private static async Task<RoadSegmentVersion> CreateFirstRoadSegmentVersion(
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        var versionItem = new RoadSegmentVersion
        {
            Position = envelope.Position,
            Id = roadSegmentAdded.Id
        };
        
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentAdded.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentAdded.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction);

        versionItem.StartNodeId = roadSegmentAdded.StartNodeId;
        versionItem.EndNodeId = roadSegmentAdded.EndNodeId;
        versionItem.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        versionItem.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        versionItem.Version = roadSegmentAdded.Version;
        versionItem.GeometryVersion = roadSegmentAdded.GeometryVersion;
        versionItem.SetStatus(status);
        versionItem.SetMorphology(morphology);
        versionItem.SetCategory(category);
        versionItem.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        versionItem.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        versionItem.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        versionItem.SetMethod(geometryDrawMethod);
        versionItem.SetAccessRestriction(accessRestriction);

        versionItem.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        versionItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        versionItem.OrganizationId = envelope.Message.OrganizationId;
        versionItem.OrganizationName = envelope.Message.Organization;

        return versionItem;
    }

    private static async Task ModifyRoadSegment(
        RoadSegmentVersion roadSegment,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
        var status = RoadSegmentStatus.Parse(roadSegmentModified.Status);
        var morphology = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology);
        var category = RoadSegmentCategory.Parse(roadSegmentModified.Category);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction);

        roadSegment.StartNodeId = roadSegmentModified.StartNodeId;
        roadSegment.EndNodeId = roadSegmentModified.EndNodeId;
        roadSegment.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentModified.Geometry);
        roadSegment.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        roadSegment.Version = roadSegmentModified.Version;
        roadSegment.GeometryVersion = roadSegmentModified.GeometryVersion;
        roadSegment.SetStatus(status);
        roadSegment.SetMorphology(morphology);
        roadSegment.SetCategory(category);
        roadSegment.LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId;
        roadSegment.RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId;
        roadSegment.MaintainerId = roadSegmentModified.MaintenanceAuthority.Code;
        roadSegment.SetMethod(geometryDrawMethod);
        roadSegment.SetAccessRestriction(accessRestriction);

        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task AddRoadSegmentToEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task ModifyRoadSegmentAttributes(
        RoadSegmentVersion roadSegment,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.Version = roadSegmentAttributesModified.Version;
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        if (roadSegmentAttributesModified.Status is not null)
        {
            var status = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status);
            roadSegment.SetStatus(status);
        }

        if (roadSegmentAttributesModified.Morphology is not null)
        {
            var morphology = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology);
            roadSegment.SetMorphology(morphology);
        }

        if (roadSegmentAttributesModified.Category is not null)
        {
            var category = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category);
            roadSegment.SetCategory(category);
        }

        if (roadSegmentAttributesModified.AccessRestriction is not null)
        {
            var accessRestriction = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction);
            roadSegment.SetAccessRestriction(accessRestriction);
        }

        if (roadSegmentAttributesModified.MaintenanceAuthority is not null)
        {
            roadSegment.MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code;
        }
    }

    private static async Task ModifyRoadSegmentGeometry(
        RoadSegmentVersion roadSegment,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        var geometry = GeometryTranslator.FromGeometryMultiLineString(BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry));
        var polyLineMShapeContent = new PolyLineMShapeContent(geometry);

        roadSegment.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
        roadSegment.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        roadSegment.Version = roadSegmentGeometryModified.Version;
        roadSegment.GeometryVersion = roadSegmentGeometryModified.GeometryVersion;

        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RemoveRoadSegment(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemoved roadSegmentRemoved,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await context.DbContext.CreateNewRoadSegmentVersion(roadSegmentRemoved.Id,
            envelope,
            roadSegment =>
            {
                if (roadSegment.IsRemoved)
                {
                    return;
                }

                roadSegment.OrganizationId = envelope.Message.OrganizationId;
                roadSegment.OrganizationName = envelope.Message.Organization;
                roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                roadSegment.IsRemoved = true;
            },
            token);
    }

    private static async Task UpdateRoadSegmentVersion(
        RoadSegmentVersion roadSegment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int segmentId,
        int? segmentVersion,
        CancellationToken token)
    {
        if (segmentVersion is null
            || context.ProcessedRoadSegmentVersions.TryGetValue(segmentId, out var processedRoadSegmentVersion) && processedRoadSegmentVersion == segmentVersion.Value)
        {
            return;
        }

        await context.DbContext.CreateNewRoadSegmentVersion(segmentId,
            envelope,
            roadSegment =>
            {
                roadSegment.Version = segmentVersion.Value;

                roadSegment.OrganizationId = envelope.Message.OrganizationId;
                roadSegment.OrganizationName = envelope.Message.Organization;
                roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

                context.ProcessedRoadSegmentVersions[segmentId] = segmentVersion.Value;
            },
            token);
    }

    private sealed class RoadNetworkChangesAcceptedContext(IntegrationContext dbContext)
    {
        public IntegrationContext DbContext { get; } = dbContext;
        public readonly Dictionary<int, int> ProcessedRoadSegmentVersions = [];
    }
}
