namespace RoadRegistry.Projections
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class RoadSegmentCategoryDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentCategoryDbaseSchema Schema = new RoadSegmentCategoryDbaseSchema();

        public static readonly RoadSegmentCategoryDbaseRecord[] All =
            Array.ConvertAll(
                RoadSegmentCategory.All,
                candidate => new RoadSegmentCategoryDbaseRecord(candidate)
            );

        public RoadSegmentCategoryDbaseRecord(RoadSegmentCategory value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseString(Schema.WEGCAT, value.Translation.Identifier),
                new DbaseString(Schema.LBLWEGCAT, value.Translation.Name),
                new DbaseString(Schema.DEFWEGCAT, value.Translation.Description)
            };
        }
    }
}
