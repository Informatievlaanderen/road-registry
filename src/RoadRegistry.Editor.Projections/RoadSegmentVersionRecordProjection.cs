namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extensions;
using Microsoft.Extensions.Logging;
using Schema;
using Schema.RoadSegments;

public class RoadSegmentVersionRecordProjection : ConnectedProjection<EditorContext>
{
    private readonly ILogger<RoadSegmentVersionRecordProjection> _logger;

    public RoadSegmentVersionRecordProjection(ILogger<RoadSegmentVersionRecordProjection> logger)
    {
        _logger = logger.ThrowIfNull();

        When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
        {
            await context.RoadSegmentVersions.AddAsync(
                new RoadSegmentVersionRecord
                {
                    StreamId = envelope.StreamId,
                    Id = envelope.Message.Id,
                    Method = RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation.Identifier,
                    Version = envelope.Message.Version,
                    GeometryVersion = envelope.Message.GeometryVersion,
                    IsRemoved = false,
                    RecordingDate = envelope.Message.RecordingDate
                }, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var message in envelope.Message.Changes.Flatten())
                switch (message)
                {
                    case RoadSegmentAdded roadSegmentAdded:
                        await AddRoadSegment(context, roadSegmentAdded, envelope, token);
                        break;

                    case RoadSegmentModified roadSegmentModified:
                        await ModifyRoadSegment(context, roadSegmentModified, envelope, token);
                        break;

                    case RoadSegmentAddedToEuropeanRoad change:
                        await AddRoadSegmentToEuropeanRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad change:
                        await RemoveRoadSegmentFromEuropeanRoad(context, change, envelope, token);
                        break;

                    case RoadSegmentAddedToNationalRoad change:
                        await AddRoadSegmentToNationalRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNationalRoad change:
                        await RemoveRoadSegmentFromNationalRoad(context, change, envelope, token);
                        break;

                    case RoadSegmentAddedToNumberedRoad change:
                        await AddRoadSegmentToNumberedRoad(context, change, envelope, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad change:
                        await RemoveRoadSegmentFromNumberedRoad(context, change, envelope, token);
                        break;

                    case RoadSegmentAttributesModified roadSegmentAttributesModified:
                        await ModifyRoadSegmentAttributes(context, roadSegmentAttributesModified, envelope, token);
                        break;

                    case RoadSegmentGeometryModified roadSegmentGeometryModified:
                        await ModifyRoadSegmentGeometry(context, roadSegmentGeometryModified, envelope, token);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RemoveRoadSegment(context, roadSegmentRemoved, envelope, token);
                        break;
                }
        });

        When<Envelope<RoadSegmentsStreetNamesChanged>>(async (context, envelope, token) =>
        {
            foreach (var roadSegment in envelope.Message.RoadSegments)
            {
                await RoadSegmentStreetNamesChanged(context, roadSegment, envelope, token);
            }
        });
    }

    private static async Task AddRoadSegment(
        EditorContext context,
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Identifier;

        var dbRecord = await context.RoadSegmentVersions
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentAdded.Id && x.StreamId == envelope.StreamId && x.Method == method, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentVersionRecord
            {
                StreamId = envelope.StreamId,
                Id = roadSegmentAdded.Id,
                Method = method,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };
            await context.RoadSegmentVersions.AddAsync(dbRecord, token);
        }
        else
        {
            dbRecord.IsRemoved = false;
        }

        dbRecord.Version = roadSegmentAdded.Version;
        dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;
    }

    private static async Task ModifyRoadSegment(
        EditorContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation.Identifier;

        var dbRecord = await context.RoadSegmentVersions
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentModified.Id && x.StreamId == envelope.StreamId && x.Method == method, token)
            .ConfigureAwait(false);
        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentVersionRecord
            {
                StreamId = envelope.StreamId,
                Id = roadSegmentModified.Id,
                Method = method,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };
            await context.RoadSegmentVersions.AddAsync(dbRecord, token);
        }

        dbRecord.Version = roadSegmentModified.Version;
        dbRecord.GeometryVersion = roadSegmentModified.GeometryVersion;
        dbRecord.IsRemoved = false;
    }

    private static async Task AddRoadSegmentToEuropeanRoad(
        EditorContext context,
        RoadSegmentAddedToEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromEuropeanRoad(
        EditorContext context,
        RoadSegmentRemovedFromEuropeanRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNationalRoad(
        EditorContext context,
        RoadSegmentAddedToNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNationalRoad(
        EditorContext context,
        RoadSegmentRemovedFromNationalRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task AddRoadSegmentToNumberedRoad(
        EditorContext context,
        RoadSegmentAddedToNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task RemoveRoadSegmentFromNumberedRoad(
        EditorContext context,
        RoadSegmentRemovedFromNumberedRoad change,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.SegmentId, change.SegmentVersion, token);
    }

    private static async Task ModifyRoadSegmentAttributes(
        EditorContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = (await context.RoadSegmentVersions.IncludeLocalToListAsync(roadSegmentVersions =>
                roadSegmentVersions
                    .Where(x => x.Id == roadSegmentAttributesModified.Id)
                    .OrderByDescending(x => x.RecordingDate)
                    .Take(1)
            , token).ConfigureAwait(false)).FirstOrDefault();

        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentVersionRecord
            {
                StreamId = envelope.StreamId,
                Id = roadSegmentAttributesModified.Id,
                Method = 0,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };
            await context.RoadSegmentVersions.AddAsync(dbRecord, token);
        }

        dbRecord.Version = roadSegmentAttributesModified.Version;
    }

    private static async Task RoadSegmentStreetNamesChanged(
        EditorContext context,
        RoadSegmentStreetNamesChanged change,
        Envelope<RoadSegmentsStreetNamesChanged> envelope,
        CancellationToken token)
    {
        await UpdateRoadSegmentVersion(context, envelope, change.Id, change.Version, token);
    }

    private static async Task ModifyRoadSegmentGeometry(
        EditorContext context,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = (await context.RoadSegmentVersions.IncludeLocalToListAsync(roadSegmentVersions =>
                roadSegmentVersions
                    .Where(x => x.Id == roadSegmentGeometryModified.Id)
                    .OrderByDescending(x => x.RecordingDate)
                    .Take(1)
            , token).ConfigureAwait(false)).FirstOrDefault();

        if (dbRecord is null)
        {
            dbRecord = new RoadSegmentVersionRecord
            {
                StreamId = envelope.StreamId,
                Id = roadSegmentGeometryModified.Id,
                Method = 0,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };
            await context.RoadSegmentVersions.AddAsync(dbRecord, token);
        }

        dbRecord.Version = roadSegmentGeometryModified.Version;
        dbRecord.GeometryVersion = roadSegmentGeometryModified.GeometryVersion;
    }

    private static async Task RemoveRoadSegment(
        EditorContext context,
        RoadSegmentRemoved roadSegmentRemoved,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        RoadSegmentVersionRecord dbRecord;

        if (!string.IsNullOrEmpty(roadSegmentRemoved.GeometryDrawMethod))
        {
            var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentRemoved.GeometryDrawMethod).Translation.Identifier;

            dbRecord = (await context.RoadSegmentVersions.IncludeLocalToListAsync(roadSegmentVersions =>
                    roadSegmentVersions
                        .Where(x => x.Id == roadSegmentRemoved.Id && x.Method == method && !x.IsRemoved)
                        .OrderBy(x => x.RecordingDate)
                        .Take(1)
                , token).ConfigureAwait(false)).FirstOrDefault();
        }
        else
        {
            dbRecord = (await context.RoadSegmentVersions.IncludeLocalToListAsync(roadSegmentVersions =>
                    roadSegmentVersions
                        .Where(x => x.Id == roadSegmentRemoved.Id && !x.IsRemoved)
                        .OrderBy(x => x.RecordingDate)
                        .Take(1)
                , token).ConfigureAwait(false)).FirstOrDefault();
        }

        if (dbRecord is not null)
        {
            dbRecord.IsRemoved = true;
        }
    }

    private static async Task UpdateRoadSegmentVersion<TMessage>(
        EditorContext context,
        Envelope<TMessage> envelope,
        int segmentId,
        int? segmentVersion,
        CancellationToken token)
        where TMessage : IMessage
    {
        if (segmentVersion is null)
        {
            return;
        }

        var dbRecord = (await context.RoadSegmentVersions.IncludeLocalToListAsync(roadSegmentVersions =>
                roadSegmentVersions
                    .Where(x => x.Id == segmentId && x.StreamId == envelope.StreamId)
                    .OrderByDescending(x => x.RecordingDate)
                    .Take(1)
            , token).ConfigureAwait(false)).FirstOrDefault();

        if (dbRecord is null)
        {
            throw new InvalidOperationException($"{nameof(RoadSegmentVersionRecord)} with id {segmentId} is not found");
        }

        dbRecord.Version = segmentVersion.Value;
    }
}
