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
    private static void ImportPartOfNumberedRoads(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentNumberedRoadAttribute[] partOfNumberedRoads)
    {
        roadSegment.PartOfNumberedRoads = partOfNumberedRoads
            .Select(numberedRoadAttribute =>
            {
                var numberedRoadDirectionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoadAttribute.Direction).Translation;

                return new RoadSegmentNumberedRoadAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = numberedRoadAttribute.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    Number = numberedRoadAttribute.Number,
                    DirectionId = numberedRoadDirectionTranslation.Identifier,
                    DirectionLabel = numberedRoadDirectionTranslation.Name,
                    SequenceNumber = numberedRoadAttribute.Ordinal,
                    OrganizationId = numberedRoadAttribute.Origin.OrganizationId,
                    OrganizationName = numberedRoadAttribute.Origin.Organization,
                    CreatedOnTimestamp = new DateTimeOffset(numberedRoadAttribute.Origin.Since),
                    VersionTimestamp = new DateTimeOffset(numberedRoadAttribute.Origin.Since)
                };
            })
            .ToList();
    }

    private static void AddPartOfNumberedRoad(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNumberedRoad numberedRoad)
    {
        var numberedRoadDirectionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;

        roadSegment.PartOfNumberedRoads.Add(new RoadSegmentNumberedRoadAttributeVersion
        {
            Position = roadSegment.Position,
            Id = numberedRoad.AttributeId,
            RoadSegmentId = numberedRoad.SegmentId,
            Number = numberedRoad.Number,
            DirectionId = numberedRoadDirectionTranslation.Identifier,
            DirectionLabel = numberedRoadDirectionTranslation.Name,
            SequenceNumber = numberedRoad.Ordinal,
            OrganizationId = envelope.Message.OrganizationId,
            OrganizationName = envelope.Message.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        });
    }

    private static void RemovePartOfNumberedRoads(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        int? id = null)
    {
        var removePartOfNumberedRoads = id is not null
            ? roadSegment.PartOfNumberedRoads.Where(x => x.Id == id.Value)
            : roadSegment.PartOfNumberedRoads;

        foreach (var version in removePartOfNumberedRoads)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
