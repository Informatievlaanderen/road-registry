namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadSegmentStatusDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentStatusDbaseSchema Schema = new RoadSegmentStatusDbaseSchema();

        public static readonly RoadSegmentStatusDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentStatus.All,
                candidate => new RoadSegmentStatusDbaseRecord(candidate)
            );

        public RoadSegmentStatusDbaseRecord(RoadSegmentStatus value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.STATUS, value.Translation.Identifier),
                new DbaseString(Schema.LBLSTATUS, value.Translation.Name),
                new DbaseString(Schema.DEFSTATUS, value.Translation.Description)
            };
        }
    }
}
