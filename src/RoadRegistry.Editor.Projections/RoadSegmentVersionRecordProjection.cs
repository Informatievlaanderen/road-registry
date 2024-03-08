namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Schema;
using Schema.Extensions;
using Schema.RoadSegments;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

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
    }

    private static async Task AddRoadSegment(
        EditorContext context,
        RoadSegmentAdded roadSegmentAdded,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Identifier;

        var dbRecord = await context.RoadSegmentVersions
            .FindAsync(x => x.Id == roadSegmentAdded.Id && x.StreamId == envelope.StreamId && x.Method == method, token)
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

        dbRecord.Version = roadSegmentAdded.Version;
        dbRecord.GeometryVersion = roadSegmentAdded.GeometryVersion;
        dbRecord.IsRemoved = false;
    }

    private static async Task ModifyRoadSegment(
        EditorContext context,
        RoadSegmentModified roadSegmentModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var method = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation.Identifier;

        var dbRecord = await context.RoadSegmentVersions
            .FindAsync(x => x.Id == roadSegmentModified.Id && x.StreamId == envelope.StreamId && x.Method == method, token)
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

    private static async Task ModifyRoadSegmentAttributes(
        EditorContext context,
        RoadSegmentAttributesModified roadSegmentAttributesModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentVersions
            .Where(x => x.Id == roadSegmentAttributesModified.Id)
            .OrderByDescending(x => x.RecordingDate)
            .FirstAsync(token)
            .ConfigureAwait(false);

        dbRecord.Version = roadSegmentAttributesModified.Version;
    }

    private static async Task ModifyRoadSegmentGeometry(
        EditorContext context,
        RoadSegmentGeometryModified roadSegmentGeometryModified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var dbRecord = await context.RoadSegmentVersions
            .Where(x => x.Id == roadSegmentGeometryModified.Id)
            .OrderByDescending(x => x.RecordingDate)
            .FirstAsync(token)
            .ConfigureAwait(false);

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

            dbRecord = await context.RoadSegmentVersions
                .Where(x => x.Id == roadSegmentRemoved.Id && x.Method == method && !x.IsRemoved)
                .OrderBy(x => x.RecordingDate)
                .FirstOrDefaultAsync(token)
                .ConfigureAwait(false);
        }
        else
        {
            dbRecord = await context.RoadSegmentVersions
                .Where(x => x.Id == roadSegmentRemoved.Id && !x.IsRemoved)
                .OrderBy(x => x.RecordingDate)
                .FirstOrDefaultAsync(token)
                .ConfigureAwait(false);
        }

        if (dbRecord is not null)
        {
            dbRecord.IsRemoved = true;
        }
    }
}
