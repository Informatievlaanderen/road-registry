namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class LaneDirectionDbaseRecord : DbaseRecord
    {
        private static readonly LaneDirectionDbaseSchema Schema = new LaneDirectionDbaseSchema();

        public static readonly LaneDirectionDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentLaneDirection.All,
                candidate => new LaneDirectionDbaseRecord(candidate)
            );

        public LaneDirectionDbaseRecord(RoadSegmentLaneDirection value)
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
