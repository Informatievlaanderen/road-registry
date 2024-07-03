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
    private static void ImportLanes(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentLaneAttribute[] lanes)
    {
        roadSegment.Lanes = lanes
            .Select(laneAttribute =>
            {
                var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation;

                return new RoadSegmentLaneAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = laneAttribute.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                    Count = laneAttribute.Count,
                    DirectionId = laneDirectionTranslation.Identifier,
                    DirectionLabel = laneDirectionTranslation.Name,
                    FromPosition = (double)laneAttribute.FromPosition,
                    ToPosition = (double)laneAttribute.ToPosition,
                    OrganizationId = laneAttribute.Origin.OrganizationId,
                    OrganizationName = laneAttribute.Origin.Organization,
                    CreatedOnTimestamp = laneAttribute.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = laneAttribute.Origin.Since.ToBelgianInstant()
                };
            })
            .ToList();
    }

    private static void UpdateLanes(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentLaneAttributes[] lanes)
    {
        if (lanes is null)
        {
            return;
        }

        var currentSet = roadSegment.Lanes.ToDictionary(a => a.Id);

        var nextSet = lanes
            .Select(laneAttribute =>
            {
                var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(laneAttribute.Direction).Translation;

                return new RoadSegmentLaneAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = laneAttribute.AttributeId,
                    RoadSegmentId = roadSegment.Id,
                    AsOfGeometryVersion = laneAttribute.AsOfGeometryVersion,
                    Count = laneAttribute.Count,
                    DirectionId = laneDirectionTranslation.Identifier,
                    DirectionLabel = laneDirectionTranslation.Name,
                    FromPosition = (double)laneAttribute.FromPosition,
                    ToPosition = (double)laneAttribute.ToPosition,
                    OrganizationId = envelope.Message.OrganizationId,
                    OrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        roadSegment.Lanes = Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.Count = next.Count;
                current.DirectionId = next.DirectionId;
                current.DirectionLabel = next.DirectionLabel;
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

    private static void RemoveLanes(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment)
    {
        foreach (var version in roadSegment.Lanes)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
