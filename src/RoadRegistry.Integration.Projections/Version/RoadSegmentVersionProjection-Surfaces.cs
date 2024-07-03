namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Linq;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public partial class RoadSegmentVersionProjection
{
    private static void ImportSurfaces(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentSurfaceAttribute[] surfaces)
    {
        roadSegment.Surfaces = surfaces
            .Select(surfaceAttribute =>
            {
                var surfaceTypeTranslation = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation;

                return new RoadSegmentSurfaceAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = surfaceAttribute.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                    TypeId = surfaceTypeTranslation.Identifier,
                    TypeLabel = surfaceTypeTranslation.Name,
                    FromPosition = (double)surfaceAttribute.FromPosition,
                    ToPosition = (double)surfaceAttribute.ToPosition,
                    OrganizationId = surfaceAttribute.Origin.OrganizationId,
                    OrganizationName = surfaceAttribute.Origin.Organization,
                    CreatedOnTimestamp = surfaceAttribute.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = surfaceAttribute.Origin.Since.ToBelgianInstant()
                };
            })
            .ToList();
    }

    private static void UpdateSurfaces(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentSurfaceAttributes[] surfaces)
    {
        if (surfaces is null)
        {
            return;
        }

        var currentSet = roadSegment.Surfaces.ToDictionary(a => a.Id);

        var nextSet = surfaces
            .Select(surfaceAttribute =>
            {
                var surfaceTypeTranslation = RoadSegmentSurfaceType.Parse(surfaceAttribute.Type).Translation;

                return new RoadSegmentSurfaceAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = surfaceAttribute.AttributeId,
                    RoadSegmentId = roadSegment.Id,
                    AsOfGeometryVersion = surfaceAttribute.AsOfGeometryVersion,
                    TypeId = surfaceTypeTranslation.Identifier,
                    TypeLabel = surfaceTypeTranslation.Name,
                    FromPosition = (double)surfaceAttribute.FromPosition,
                    ToPosition = (double)surfaceAttribute.ToPosition,
                    OrganizationId = envelope.Message.OrganizationId,
                    OrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        roadSegment.Surfaces = Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.TypeId = next.TypeId;
                current.TypeLabel = next.TypeLabel;
                current.FromPosition = next.FromPosition;
                current.ToPosition = next.ToPosition;
                current.OrganizationId = next.OrganizationId;
                current.OrganizationName = next.OrganizationName;
                current.VersionTimestamp = next.VersionTimestamp;
            },
            current =>
            {
                if (current.IsRemoved) return;

                current.OrganizationId = envelope.Message.OrganizationId;
                current.OrganizationName = envelope.Message.Organization;
                current.IsRemoved = true;
                current.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            });
    }

    private static void RemoveSurfaces(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment)
    {
        foreach (var version in roadSegment.Surfaces)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
