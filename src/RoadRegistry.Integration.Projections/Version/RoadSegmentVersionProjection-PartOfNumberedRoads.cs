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
                    CreatedOnTimestamp = numberedRoadAttribute.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = numberedRoadAttribute.Origin.Since.ToBelgianInstant()
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

        var item = roadSegment.PartOfNumberedRoads.SingleOrDefault(x => x.Id == numberedRoad.AttributeId);

        if (item is null)
        {
            item = new RoadSegmentNumberedRoadAttributeVersion
            {
                Position = roadSegment.Position,
                Id = numberedRoad.AttributeId,
                RoadSegmentId = numberedRoad.SegmentId
            };
            roadSegment.PartOfNumberedRoads.Add(item);
        }
        else
        {
            item.IsRemoved = false;
        }

        item.RoadSegmentId = numberedRoad.SegmentId;
        item.Number = numberedRoad.Number;
        item.DirectionId = numberedRoadDirectionTranslation.Identifier;
        item.DirectionLabel = numberedRoadDirectionTranslation.Name;
        item.SequenceNumber = numberedRoad.Ordinal;
        item.OrganizationId = envelope.Message.OrganizationId;
        item.OrganizationName = envelope.Message.Organization;
        item.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        item.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
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
