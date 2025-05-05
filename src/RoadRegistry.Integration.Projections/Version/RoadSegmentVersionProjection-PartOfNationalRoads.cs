namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Linq;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public partial class RoadSegmentVersionProjection
{
    private static void ImportPartOfNationalRoads(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentNationalRoadAttribute[] partOfNationalRoads)
    {
        roadSegment.PartOfNationalRoads = partOfNationalRoads
            .Select(nationalRoadAttribute => new RoadSegmentNationalRoadAttributeVersion
            {
                Position = roadSegment.Position,
                Id = nationalRoadAttribute.AttributeId,
                RoadSegmentId = envelope.Message.Id,
                Number = nationalRoadAttribute.Number,
                OrganizationId = nationalRoadAttribute.Origin.OrganizationId,
                OrganizationName = nationalRoadAttribute.Origin.Organization,
                CreatedOnTimestamp = nationalRoadAttribute.Origin.Since.ToBelgianInstant(),
                VersionTimestamp = nationalRoadAttribute.Origin.Since.ToBelgianInstant()
            })
            .ToList();
    }

    private static void AddPartOfNationalRoad(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToNationalRoad nationalRoad)
    {
        var item = roadSegment.PartOfNationalRoads.SingleOrDefault(x => x.Id == nationalRoad.AttributeId);

        if (item is null)
        {
            item = new RoadSegmentNationalRoadAttributeVersion
            {
                Position = roadSegment.Position,
                Id = nationalRoad.AttributeId,
                RoadSegmentId = nationalRoad.SegmentId
            };
            roadSegment.PartOfNationalRoads.Add(item);
        }
        else
        {
            item.IsRemoved = false;
        }

        item.RoadSegmentId = nationalRoad.SegmentId;
        item.Number = nationalRoad.Number;
        item.OrganizationId = envelope.Message.OrganizationId;
        item.OrganizationName = envelope.Message.Organization;
        item.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        item.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static void RemovePartOfNationalRoads(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        int? id = null)
    {
        var removePartOfNationalRoads = id is not null
            ? roadSegment.PartOfNationalRoads.Where(x => x.Id == id.Value)
            : roadSegment.PartOfNationalRoads;

        foreach (var version in removePartOfNationalRoads)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
