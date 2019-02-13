namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadNodeChangeDbaseSchema Schema = new RoadNodeChangeDbaseSchema();

        public RoadNodeChangeDbaseRecord()
        {
            WEGKNOOPID = new DbaseInt32(Schema.WEGKNOOPID);
            TYPE = new DbaseInt16(Schema.TYPE);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                WEGKNOOPID,
                TYPE,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 WEGKNOOPID { get; }

        public DbaseInt16 TYPE { get; }

        public DbaseString EUNUMMER { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}
