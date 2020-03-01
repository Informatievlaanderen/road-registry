namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadSegmentDirectionDbaseRecord : DbaseRecord
    {
        public static readonly NumberedRoadSegmentDirectionDbaseSchema Schema = new NumberedRoadSegmentDirectionDbaseSchema();

        public NumberedRoadSegmentDirectionDbaseRecord()
        {
            RICHTING = new DbaseNumber(Schema.RICHTING);
            LBLRICHT = new DbaseCharacter(Schema.LBLRICHT);
            DEFRICHT = new DbaseCharacter(Schema.DEFRICHT);

            Values = new DbaseFieldValue[]
            {
                RICHTING, LBLRICHT, DEFRICHT
            };
        }

        public DbaseNumber RICHTING { get; }
        public DbaseCharacter LBLRICHT { get; }
        public DbaseCharacter DEFRICHT { get; }
    }
}
