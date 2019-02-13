namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentSurfaceChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentSurfaceChangeDbaseSchema Schema = new RoadSegmentSurfaceChangeDbaseSchema();

        public RoadSegmentSurfaceChangeDbaseRecord()
        {
            WV_OIDN = new DbaseInt32(Schema.WV_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            VAN_POSITIE = new DbaseDouble(Schema.VAN_POSITIE);
            TOT_POSITIE = new DbaseDouble(Schema.TOT_POSITIE);
            TYPE = new DbaseInt16(Schema.TYPE);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                WV_OIDN,
                WS_OIDN,
                VAN_POSITIE,
                TOT_POSITIE,
                TYPE,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 WV_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseDouble VAN_POSITIE { get; }
        
        public DbaseDouble TOT_POSITIE { get; }
        
        public DbaseInt16 TYPE { get; }
        
        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}