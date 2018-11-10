namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentMorphologyDbaseSchema Schema = new RoadSegmentMorphologyDbaseSchema();

        public static readonly RoadSegmentMorphologyDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentMorphology.All,
                candidate => new RoadSegmentMorphologyDbaseRecord(candidate)
            );

        public RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.MORF, value.Translation.Identifier),
                new DbaseString(Schema.LBLMORF, value.Translation.Name),
                new DbaseString(Schema.DEFMORF, value.Translation.Description)
            };
        }
    }
}
