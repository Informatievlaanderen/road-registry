namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentWidthChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentWidthChangeDbaseSchema Schema = new RoadSegmentWidthChangeDbaseSchema();

        public RoadSegmentWidthChangeDbaseRecord()
        {
            WB_OIDN = new DbaseInt32(Schema.WB_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            VAN_POSITIE = new DbaseDouble(Schema.VAN_POSITIE);
            TOT_POSITIE = new DbaseDouble(Schema.TOT_POSITIE);
            BREEDTE = new DbaseInt16(Schema.BREEDTE);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);

            Values = new DbaseFieldValue[]
            {
                WB_OIDN,
                WS_OIDN,
                VAN_POSITIE,
                TOT_POSITIE,
                BREEDTE,
                TransactID,
                RecordType
            };
        }

        public DbaseInt32 WB_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseDouble VAN_POSITIE { get; }
        
        public DbaseDouble TOT_POSITIE { get; }
        
        public DbaseInt16 BREEDTE { get; }
        
        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }
    }
}