namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class SurfaceTypeDbaseRecord : DbaseRecord
    {
        private static readonly SurfaceTypeDbaseSchema Schema = new SurfaceTypeDbaseSchema();

        public static readonly SurfaceTypeDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentSurfaceType.All,
                candidate => new SurfaceTypeDbaseRecord(candidate)
            );

        public SurfaceTypeDbaseRecord(RoadSegmentSurfaceType value)
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
