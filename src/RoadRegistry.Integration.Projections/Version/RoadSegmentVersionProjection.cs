namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Collections.Generic;
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

public partial class RoadSegmentVersionProjection : ConnectedProjection<IntegrationContext>
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

            var roadSegment = new RoadSegmentVersion
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

                OrganizationId = envelope.Message.Origin.OrganizationId,
                OrganizationName = envelope.Message.Origin.Organization,
                CreatedOnTimestamp = envelope.Message.RecordingDate.ToBelgianInstant(),
                VersionTimestamp = envelope.Message.Origin.Since.ToBelgianInstant()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

            ImportLanes(envelope, roadSegment, envelope.Message.Lanes);
            ImportSurfaces(envelope, roadSegment, envelope.Message.Surfaces);
            ImportWidths(envelope, roadSegment, envelope.Message.Widths);
            ImportPartOfEuropeanRoads(envelope, roadSegment, envelope.Message.PartOfEuropeanRoads);
            ImportPartOfNationalRoads(envelope, roadSegment, envelope.Message.PartOfNationalRoads);
            ImportPartOfNumberedRoads(envelope, roadSegment, envelope.Message.PartOfNumberedRoads);

            await context.RoadSegmentVersions.AddAsync(roadSegment, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            var changesGroupedByRoadSegments = envelope.Message.Changes
                .Select(RoadSegmentChange.From)
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
                        case RoadSegmentAdded change:
                            throw new InvalidOperationException($"Invalid RoadSegmentAdded change for ID {change.Id} for position {envelope.Position}");

                        case RoadSegmentModified change:
                            ModifyRoadSegment(roadSegment, change, envelope);
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

                        case RoadSegmentAttributesModified change:
                            ModifyRoadSegmentAttributes(roadSegment, change, envelope);
                            break;

                        case RoadSegmentGeometryModified change:
                            ModifyRoadSegmentGeometry(roadSegment, change, envelope);
                            break;

                        case RoadSegmentRemoved:
                            RemoveRoadSegment(roadSegment, envelope);
                            break;

                        default:
                            throw new NotImplementedException($"Change {message.GetType().Name} is not implemented when handling {nameof(RoadNetworkChangesAccepted)}");
                    }
            }
        });
    }

    private static RoadSegmentVersion CreateFirstRoadSegmentVersion(
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        var roadSegment = new RoadSegmentVersion
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

        roadSegment.StartNodeId = roadSegmentAdded.StartNodeId;
        roadSegment.EndNodeId = roadSegmentAdded.EndNodeId;
        roadSegment.Geometry = BackOffice.GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        roadSegment.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));

        roadSegment.Version = roadSegmentAdded.Version;
        roadSegment.GeometryVersion = roadSegmentAdded.GeometryVersion;
        roadSegment.SetStatus(status);
        roadSegment.SetMorphology(morphology);
        roadSegment.SetCategory(category);
        roadSegment.LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId;
        roadSegment.RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId;
        roadSegment.MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code;
        roadSegment.SetMethod(geometryDrawMethod);
        roadSegment.SetAccessRestriction(accessRestriction);

        roadSegment.OrganizationId = envelope.Message.OrganizationId;
        roadSegment.OrganizationName = envelope.Message.Organization;
        roadSegment.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);

        UpdateLanes(envelope, roadSegment, roadSegmentAdded.Lanes);
        UpdateSurfaces(envelope, roadSegment, roadSegmentAdded.Surfaces);
        UpdateWidths(envelope, roadSegment, roadSegmentAdded.Widths);

        return roadSegment;
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

        UpdateLanes(envelope, roadSegment, roadSegmentModified.Lanes);
        UpdateSurfaces(envelope, roadSegment, roadSegmentModified.Surfaces);
        UpdateWidths(envelope, roadSegment, roadSegmentModified.Widths);
    }

    private static void AddRoadSegmentToEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        AddPartOfEuropeanRoad(envelope, roadSegment, change);
    }

    private static void RemoveRoadSegmentFromEuropeanRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        RemovePartOfEuropeanRoads(envelope, roadSegment, change.AttributeId);
    }

    private static void AddRoadSegmentToNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        AddPartOfNationalRoad(envelope, roadSegment, change);
    }

    private static void RemoveRoadSegmentFromNationalRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        RemovePartOfNationalRoads(envelope, roadSegment, change.AttributeId);
    }

    private static void AddRoadSegmentToNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        AddPartOfNumberedRoad(envelope, roadSegment, change);
    }

    private static void RemoveRoadSegmentFromNumberedRoad(
        RoadSegmentVersion roadSegment,
        RoadSegmentRemovedFromNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope)
    {
        UpdateRoadSegmentVersion(roadSegment, envelope, change.SegmentVersion);
        RemovePartOfNumberedRoads(envelope, roadSegment, change.AttributeId);
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

        UpdateLanes(envelope, roadSegment, roadSegmentAttributesModified.Lanes);
        UpdateSurfaces(envelope, roadSegment, roadSegmentAttributesModified.Surfaces);
        UpdateWidths(envelope, roadSegment, roadSegmentAttributesModified.Widths);
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

        UpdateLanes(envelope, roadSegment, roadSegmentGeometryModified.Lanes);
        UpdateSurfaces(envelope, roadSegment, roadSegmentGeometryModified.Surfaces);
        UpdateWidths(envelope, roadSegment, roadSegmentGeometryModified.Widths);
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

        RemoveLanes(envelope, roadSegment);
        RemoveSurfaces(envelope, roadSegment);
        RemoveWidths(envelope, roadSegment);
        RemovePartOfEuropeanRoads(envelope, roadSegment);
        RemovePartOfNationalRoads(envelope, roadSegment);
        RemovePartOfNumberedRoads(envelope, roadSegment);
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

    private static ICollection<T> Synchronize<T>(
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier,
        Action<T> remove) where T : class
    {
        var source = currentSet.Values.ToList();

        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
            }
            else if (gotCurrent)
            {
                remove(current);
            }
            else if (gotNext)
            {
                source.Add(next);
            }
        }

        return source;
    }

    public sealed record RoadSegmentChange(int Id, object Change)
    {
        public static RoadSegmentChange From(AcceptedChange acceptedChange)
        {
            var change = acceptedChange.Flatten();
            var roadSegmentId = GetHandledRoadSegmentIdFromChange(change);
            return roadSegmentId is not null ? new RoadSegmentChange(roadSegmentId.Value, change) : null;
        }

        private static int? GetHandledRoadSegmentIdFromChange(object changeMessage)
        {
            // All changes which aren't handled in RoadNetworkChangesAccepted should return null

            return changeMessage switch
            {
                RoadSegmentAdded change => change.Id,
                RoadSegmentModified change => change.Id,
                RoadSegmentRemoved change => change.Id,
                RoadSegmentAddedToEuropeanRoad change => change.SegmentId,
                RoadSegmentRemovedFromEuropeanRoad change => change.SegmentId,
                RoadSegmentAddedToNationalRoad change => change.SegmentId,
                RoadSegmentRemovedFromNationalRoad change => change.SegmentId,
                RoadSegmentAddedToNumberedRoad change => change.SegmentId,
                RoadSegmentRemovedFromNumberedRoad change => change.SegmentId,
                RoadSegmentAttributesModified change => change.Id,
                RoadSegmentGeometryModified change => change.Id,
                GradeSeparatedJunctionAdded => null,
                GradeSeparatedJunctionModified => null,
                GradeSeparatedJunctionRemoved => null,
                RoadNodeAdded => null,
                RoadNodeModified => null,
                RoadNodeRemoved => null,
                OutlinedRoadSegmentRemoved => null,
                _ => throw new NotImplementedException($"Missing implementation for change {changeMessage.GetType().Name}")
            };
        }
    }
}
