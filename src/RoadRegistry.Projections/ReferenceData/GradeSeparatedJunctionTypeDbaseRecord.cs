namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class GradeSeparatedJunctionTypeDbaseRecord : DbaseRecord
    {
        private static readonly GradeSeparatedJunctionTypeDbaseSchema Schema = new GradeSeparatedJunctionTypeDbaseSchema();

        public static readonly GradeSeparatedJunctionTypeDbaseRecord[] All =
            Array.ConvertAll(
                GradeSeparatedJunctionType.All,
                candidate => new GradeSeparatedJunctionTypeDbaseRecord(candidate)
            );

        public GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType value)
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
