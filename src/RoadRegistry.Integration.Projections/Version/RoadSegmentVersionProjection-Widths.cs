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
    private static void ImportWidths(
        Envelope<ImportedRoadSegment> envelope,
        RoadSegmentVersion roadSegment,
        ImportedRoadSegmentWidthAttribute[] widths)
    {
        roadSegment.Widths = widths
            .Select(widthAttribute =>
            {
                var width = new RoadSegmentWidth(widthAttribute.Width);

                return new RoadSegmentWidthAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = widthAttribute.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                    Width = width,
                    WidthLabel = width.ToDutchString(),
                    FromPosition = (double)widthAttribute.FromPosition,
                    ToPosition = (double)widthAttribute.ToPosition,
                    OrganizationId = widthAttribute.Origin.OrganizationId,
                    OrganizationName = widthAttribute.Origin.Organization,
                    CreatedOnTimestamp = new DateTimeOffset(widthAttribute.Origin.Since),
                    VersionTimestamp = new DateTimeOffset(widthAttribute.Origin.Since)
                };
            })
            .ToList();
    }

    private static void UpdateWidths(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment,
        RoadSegmentWidthAttributes[] widths)
    {
        if (widths is null)
        {
            return;
        }

        var currentSet = roadSegment.Widths.ToDictionary(a => a.Id);

        var nextSet = widths
            .Select(widthAttribute =>
            {
                var width = new RoadSegmentWidth(widthAttribute.Width);

                return new RoadSegmentWidthAttributeVersion
                {
                    Position = roadSegment.Position,
                    Id = widthAttribute.AttributeId,
                    RoadSegmentId = roadSegment.Id,
                    AsOfGeometryVersion = widthAttribute.AsOfGeometryVersion,
                    Width = width,
                    WidthLabel = width.ToDutchString(),
                    FromPosition = (double)widthAttribute.FromPosition,
                    ToPosition = (double)widthAttribute.ToPosition,
                    OrganizationId = envelope.Message.OrganizationId,
                    OrganizationName = envelope.Message.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };
            })
            .ToDictionary(a => a.Id);

        roadSegment.Widths = Synchronize(
            currentSet,
            nextSet,
            (current, next) =>
            {
                current.AsOfGeometryVersion = next.AsOfGeometryVersion;
                current.Width = next.Width;
                current.WidthLabel = next.WidthLabel;
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

    private static void RemoveWidths(
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentVersion roadSegment)
    {
        foreach (var version in roadSegment.Widths)
        {
            version.OrganizationId = envelope.Message.OrganizationId;
            version.OrganizationName = envelope.Message.Organization;
            version.IsRemoved = true;
            version.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }
    }
}
