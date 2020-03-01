namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadNodeChangeDbaseSchema Schema = new RoadNodeChangeDbaseSchema();

        public RoadNodeChangeDbaseRecord()
        {
            WEGKNOOPID = new DbaseNumber(Schema.WEGKNOOPID);
            TYPE = new DbaseNumber(Schema.TYPE);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                WEGKNOOPID,
                TYPE,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber WEGKNOOPID { get; }

        public DbaseNumber TYPE { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
