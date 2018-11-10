namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadNodeTypeDbaseRecord : DbaseRecord
    {
        private static readonly RoadNodeTypeDbaseSchema Schema = new RoadNodeTypeDbaseSchema();

        public static readonly RoadNodeTypeDbaseRecord[] All =
            Array.ConvertAll(
                RoadNodeType.All,
                candidate => new RoadNodeTypeDbaseRecord(candidate)
            );

        public RoadNodeTypeDbaseRecord(RoadNodeType value)
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
