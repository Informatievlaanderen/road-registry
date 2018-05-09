using System.IO;
using Aiv.Vbr.ProjectionHandling.Connector;
using RoadRegistry.Events;
using Shaperon;
using Shaperon.IO;
using Wkx;

namespace RoadRegistry.Projections
{

    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private static readonly RoadNodeTypeTranslator RoadNodeTypeTranslator = new RoadNodeTypeTranslator();

        public RoadNodeRecordProjection()
        {
            When<ImportedRoadNode>((context, @event, token) =>
            {
                //TODO:
                //- Where does record number come from?
                //- Use pooled memory streams

                return context.AddAsync(new RoadNodeRecord
                {
                    Id = @event.Id,
                    ShapeRecord = ShapeRecord
                        .Create(
                            new RecordNumber(1), 
                            new PointShapeContent(RoadNodePoint.FromWellKnownBinary(@event.Geometry.WellKnownBinary))
                        ).ToBytes(),
                    DbaseRecord = new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = @event.Id },
                        WK_UIDN = { Value = @event.Id + "_" + @event.Version },
                        TYPE = { Value = RoadNodeTypeTranslator.TranslateToIdentifier(@event.Type) },
                        LBLTYPE = { Value = RoadNodeTypeTranslator.TranslateToDutchName(@event.Type) },
                    }.ToBytes()
                    //...
                }, token);
            });
        }
    }
}