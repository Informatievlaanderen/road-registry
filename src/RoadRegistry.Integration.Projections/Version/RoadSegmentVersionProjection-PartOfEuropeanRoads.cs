namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Linq;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public partial class RoadSegmentVersionProjection
{
    private static void ImportPartOfEuropeanRoads(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentEuropeanRoadAttribute[] partOfEuropeanRoads)
    {
        roadSegment.PartOfEuropeanRoads = partOfEuropeanRoads
            .Select(europeanRoadAttribute => new RoadSegmentEuropeanRoadAttributeVersion
            {
                Position = roadSegment.Position,
                Id = europeanRoadAttribute.AttributeId,
                RoadSegmentId = envelope.Message.Id,
                Number = europeanRoadAttribute.Number,
                OrganizationId = europeanRoadAttribute.Origin.OrganizationId,
                OrganizationName = europeanRoadAttribute.Origin.Organization,
                CreatedOnTimestamp = europeanRoadAttribute.Origin.Since.ToBelgianInstant(),
                VersionTimestamp = europeanRoadAttribute.Origin.Since.ToBelgianInstant()
            })
            .ToList();
    }

    private static void AddPartOfEuropeanRoad(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentAddedToEuropeanRoad europeanRoad)
    {
        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
        {
            Position = roadSegment.Position,
            Id = europeanRoad.AttributeId,
            RoadSegmentId = europeanRoad.SegmentId,
            Number = europeanRoad.Number,
            OrganizationId = envelope.Message.OrganizationId,
            OrganizationName = envelope.Message.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        });
    }

    private static void RemovePartOfEuropeanRoads(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        int? id = null)
    {
        var removePartOfEuropeanRoads = id is not null
            ? roadSegment.PartOfEuropeanRoads.Where(x => x.Id == id.Value)
            : roadSegment.PartOfEuropeanRoads;

        foreach (var version in removePartOfEuropeanRoads)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
