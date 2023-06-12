namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.IO;
using Schema;

public class RoadSegmentEuropeanRoadAttributeRecordProjection : ConnectedProjection<EditorContext>
{
    public RoadSegmentEuropeanRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));

        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfEuropeanRoads.Length == 0)
                return Task.CompletedTask;

            var europeanRoadAttributes = envelope.Message
                .PartOfEuropeanRoads
                .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                {
                    Id = europeanRoad.AttributeId,
                    RoadSegmentId = envelope.Message.Id,
                    DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                    {
                        EU_OIDN = { Value = europeanRoad.AttributeId },
                        WS_OIDN = { Value = envelope.Message.Id },
                        EUNUMMER = { Value = europeanRoad.Number },
                        BEGINTIJD = { Value = europeanRoad.Origin.Since },
                        BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
                        LBLBGNORG = { Value = europeanRoad.Origin.Organization }
                    }.ToBytes(manager, encoding)
                });

            return context.AddRangeAsync(europeanRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToEuropeanRoad europeanRoad:
                        await context.RoadSegmentEuropeanRoadAttributes.AddAsync(new RoadSegmentEuropeanRoadAttributeRecord
                        {
                            Id = europeanRoad.AttributeId,
                            RoadSegmentId = europeanRoad.SegmentId,
                            DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                            {
                                EU_OIDN = { Value = europeanRoad.AttributeId },
                                WS_OIDN = { Value = europeanRoad.SegmentId },
                                EUNUMMER = { Value = europeanRoad.Number },
                                BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                BEGINORG = { Value = envelope.Message.OrganizationId },
                                LBLBGNORG = { Value = envelope.Message.Organization }
                            }.ToBytes(manager, encoding)
                        });
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad europeanRoad:
                        var roadSegmentEuropeanRoadAttributeRecord =
                            await context.RoadSegmentEuropeanRoadAttributes.FindAsync(europeanRoad.AttributeId, cancellationToken: token).ConfigureAwait(false);

                        context.RoadSegmentEuropeanRoadAttributes.Remove(roadSegmentEuropeanRoadAttributeRecord);
                        break;

                    case RoadSegmentRemoved roadSegmentRemoved:
                        var roadSegmentEuropeanRoadAttributeRecords =
                            context.RoadSegmentEuropeanRoadAttributes
                                .Local
                                .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id)
                                .Concat(context.RoadSegmentEuropeanRoadAttributes
                                    .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id));

                        context.RoadSegmentEuropeanRoadAttributes.RemoveRange(roadSegmentEuropeanRoadAttributeRecords);
                        break;
                }
        });
    }
}
