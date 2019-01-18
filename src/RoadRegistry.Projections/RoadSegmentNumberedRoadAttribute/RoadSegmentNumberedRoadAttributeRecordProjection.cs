namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using BackOfficeSchema;
    using BackOfficeSchema.RoadSegmentNumberedRoadAttributes;
    using Messages;
    using Model;

    public class RoadSegmentNumberedRoadAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;

        public RoadSegmentNumberedRoadAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.PartOfNumberedRoads.Length == 0)
                return Task.CompletedTask;

            var numberedRoadAttributes = @event
                .PartOfNumberedRoads
                .Select(numberedRoad =>
                {
                    var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
                    return new RoadSegmentNumberedRoadAttributeRecord
                    {
                        Id = numberedRoad.AttributeId,
                        RoadSegmentId = @event.Id,
                        DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                        {
                            GW_OIDN = {Value = numberedRoad.AttributeId},
                            WS_OIDN = {Value = @event.Id},
                            IDENT8 = {Value = numberedRoad.Ident8},
                            RICHTING = {Value = directionTranslation.Identifier},
                            LBLRICHT = {Value = directionTranslation.Name},
                            VOLGNUMMER = {Value = numberedRoad.Ordinal},
                            BEGINTIJD = {Value = numberedRoad.Origin.Since},
                            BEGINORG = {Value = numberedRoad.Origin.OrganizationId},
                            LBLBGNORG = {Value = numberedRoad.Origin.Organization},
                        }.ToBytes(_encoding)
                    };
                });
            return context.AddRangeAsync(numberedRoadAttributes, token);
        }
    }
}
