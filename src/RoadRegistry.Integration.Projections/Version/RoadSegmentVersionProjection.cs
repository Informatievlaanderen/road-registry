namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Linq;
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
                    roadSegment = CreateFirstRoadSegmentVersion(roadSegmentAdded, envelope);
                    roadSegmentChanges.Remove(roadSegmentAdded);
                }
                else
                {
                    roadSegment = await context.CreateNewRoadSegmentVersion(roadSegmentId, envelope, token);
                }

                await context.RoadSegmentVersions.AddAsync(roadSegment, token);

                foreach (var message in roadSegmentChanges)
                    switch (message)
                    {
                        case RoadSegmentAdded roadSegmentAdded:
                            throw new InvalidOperationException($"Invalid RoadSegmentAdded change for ID {roadSegmentAdded.Id} for position {envelope.Position}");

                        case RoadSegmentModified roadSegmentModified:
                            ModifyRoadSegment(roadSegment, roadSegmentModified, envelope);
                            break;

                        case RoadSegmentAddedToEuropeanRoad change:
                            AddRoadSegmentToEuropeanRoad(roadSegment, change, envelope);
                            break;
                        case RoadSegmentRemovedFromEuropeanRoad change:
                            RemoveRoadSegmentFromEuropeanRoad(roadSegment, change, envelope);
                            break;

                        case RoadSegmentAddedToNationalRoad change:
                            AddRoadSegmentToNationalRoad(roadSegment, change, envelope);
                            break;
                        case RoadSegmentRemovedFromNationalRoad change:
                            RemoveRoadSegmentFromNationalRoad(roadSegment, change, envelope);
                            break;

                        case RoadSegmentAddedToNumberedRoad change:
                            AddRoadSegmentToNumberedRoad(roadSegment, change, envelope);
                            break;
                        case RoadSegmentRemovedFromNumberedRoad change:
                            RemoveRoadSegmentFromNumberedRoad(roadSegment, change, envelope);
                            break;

                        case RoadSegmentAttributesModified roadSegmentAttributesModified:
                            ModifyRoadSegmentAttributes(roadSegment, roadSegmentAttributesModified, envelope);
                            break;

                        case RoadSegmentGeometryModified roadSegmentGeometryModified:
                            ModifyRoadSegmentGeometry(roadSegment, roadSegmentGeometryModified, envelope);
                            break;

                        case RoadSegmentRemoved:
                            RemoveRoadSegment(roadSegment, envelope);
                            break;
                    }
            }
        });
    }

    private static int? GetRoadSegmentIdFromChange(object changeMessage)
    {
        return changeMessage switch
        {
            RoadSegmentAdded change => change.Id,
            RoadSegmentModified change => change.Id,
            RoadSegmentAddedToEuropeanRoad change => change.SegmentId,
            RoadSegmentRemovedFromEuropeanRoad change => change.SegmentId,
            RoadSegmentAddedToNationalRoad change => change.SegmentId,
            RoadSegmentRemovedFromNationalRoad change => change.SegmentId,
            RoadSegmentAddedToNumberedRoad change => change.SegmentId,
            RoadSegmentRemovedFromNumberedRoad change => change.SegmentId,
            RoadSegmentAttributesModified change => change.Id,
            RoadSegmentGeometryModified change => change.Id,
            RoadSegmentRemoved change => change.Id,
            _ => throw new NotImplementedException($"{changeMessage.GetType().Name}")
        };
    }

    private static RoadSegmentVersion CreateFirstRoadSegmentVersion(
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

    private static void ModifyRoadSegment(
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
    
    private static void AddRoadSegmentToEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void RemoveRoadSegmentFromEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void AddRoadSegmentToNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void RemoveRoadSegmentFromNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void AddRoadSegmentToNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void RemoveRoadSegmentFromNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
    }

    private static void ModifyRoadSegmentAttributes(
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

    private static void ModifyRoadSegmentGeometry(
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

    private static void RemoveRoadSegment(
        RoadSegmentVersion roadSegment,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        if (roadSegment.IsRemoved)
        {
            return;
        }

        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        roadSegment.IsRemoved = true;
    }

    private static void UpdateRoadSegmentVersion(
        RoadSegmentVersion roadSegment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int? segmentVersion)
    {
        if (segmentVersion is null)
        {
            return;
        }

        roadSegment.Version = segmentVersion.Value;

        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private sealed record RoadSegmentChange(int Id, object Change);
}
