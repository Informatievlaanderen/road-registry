namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new RoadSegmentGeometryDrawMethodDbaseSchema();

        public static readonly RoadSegmentGeometryDrawMethodDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentGeometryDrawMethod.All,
                candidate => new RoadSegmentGeometryDrawMethodDbaseRecord(candidate)
            );

        public RoadSegmentGeometryDrawMethodDbaseRecord(RoadSegmentGeometryDrawMethod value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.METHODE, value.Translation.Identifier),
                new DbaseString(Schema.LBLMETHOD, value.Translation.Name),
                new DbaseString(Schema.DEFMETHOD, value.Translation.Description)
            };
        }
    }
}
