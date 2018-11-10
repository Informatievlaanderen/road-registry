namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class NumberedRoadSegmentDirectionDbaseRecord : DbaseRecord
    {
        private static readonly NumberedRoadSegmentDirectionDbaseSchema Schema = new NumberedRoadSegmentDirectionDbaseSchema();

        public static readonly NumberedRoadSegmentDirectionDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentNumberedRoadDirection.All,
                candidate => new NumberedRoadSegmentDirectionDbaseRecord(candidate)
            );

        public NumberedRoadSegmentDirectionDbaseRecord(RoadSegmentNumberedRoadDirection value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.RICHTING, value.Translation.Identifier),
                new DbaseString(Schema.LBLRICHT, value.Translation.Name),
                new DbaseString(Schema.DEFRICHT, value.Translation.Description)
            };
        }
    }
}
