namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentStatusDbaseSchema : DbaseSchema
    {
        public RoadSegmentStatusDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(STATUS)),
                    new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLSTATUS)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFSTATUS)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField STATUS => Fields[0];
        public DbaseField LBLSTATUS => Fields[1];
        public DbaseField DEFSTATUS => Fields[2];
    }
}
