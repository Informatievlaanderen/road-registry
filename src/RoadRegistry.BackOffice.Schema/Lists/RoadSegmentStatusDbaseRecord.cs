namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentStatusDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentStatusDbaseSchema Schema = new RoadSegmentStatusDbaseSchema();

        public RoadSegmentStatusDbaseRecord()
        {
            STATUS = new DbaseNumber(Schema.STATUS);
            LBLSTATUS = new DbaseCharacter(Schema.LBLSTATUS);
            DEFSTATUS = new DbaseCharacter(Schema.DEFSTATUS);

            Values = new DbaseFieldValue[]
            {
                STATUS, LBLSTATUS, DEFSTATUS
            };
        }

        public DbaseNumber STATUS { get; }
        public DbaseCharacter LBLSTATUS { get; }
        public DbaseCharacter DEFSTATUS { get; }
    }
}
