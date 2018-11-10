namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new RoadSegmentAccessRestrictionDbaseSchema();

        public static readonly RoadSegmentAccessRestrictionDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentAccessRestriction.All,
                candidate => new RoadSegmentAccessRestrictionDbaseRecord(candidate)
            );

        public RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.TYPE, value.Translation.Identifier),
                new DbaseString(Schema.LBLTYPE, value.Translation.Name),
                new DbaseString(Schema.DEFTYPE, value.Translation.Description)
            };
        }
    }
}
