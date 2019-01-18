namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Schema;
    using Schema.RoadSegmentNationalRoadAttributes;

    public class RoadSegmentNationalRoadAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;
        public RoadSegmentNationalRoadAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.PartOfNationalRoads.Length == 0)
                return Task.CompletedTask;


            var nationalRoadAttributes = @event
                    .PartOfNationalRoads
                    .Select(nationalRoad => new RoadSegmentNationalRoadAttributeRecord
                    {
                        Id = nationalRoad.AttributeId,
                        RoadSegmentId = @event.Id,
                        DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                        {
                            NW_OIDN = { Value = nationalRoad.AttributeId },
                            WS_OIDN = { Value = @event.Id },
                            IDENT2 = { Value = nationalRoad.Ident2 },
                            BEGINTIJD = { Value = nationalRoad.Origin.Since },
                            BEGINORG = { Value = nationalRoad.Origin.OrganizationId },
                            LBLBGNORG = { Value = nationalRoad.Origin.Organization },
                        }.ToBytes(_encoding)
                    });

            return context.AddRangeAsync(nationalRoadAttributes, token);
        }
    }
}
