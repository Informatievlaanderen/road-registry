namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentLaneChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentLaneChangeDbaseSchema Schema = new RoadSegmentLaneChangeDbaseSchema();

        public RoadSegmentLaneChangeDbaseRecord()
        {
            RS_OIDN = new DbaseInt32(Schema.RS_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            VAN_POSITIE = new DbaseDouble(Schema.VAN_POSITIE);
            TOT_POSITIE = new DbaseDouble(Schema.TOT_POSITIE);
            AANTAL = new DbaseInt16(Schema.AANTAL);
            RICHTING = new DbaseInt16(Schema.RICHTING);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                RS_OIDN,
                WS_OIDN,
                VAN_POSITIE,
                TOT_POSITIE,
                AANTAL,
                RICHTING,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 RS_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseDouble VAN_POSITIE { get; }
        
        public DbaseDouble TOT_POSITIE { get; }
        
        public DbaseInt16 AANTAL { get; }
        
        public DbaseInt16 RICHTING { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}