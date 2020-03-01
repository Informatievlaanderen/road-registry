namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class LaneDirectionDbaseRecord : DbaseRecord
    {
        public static readonly LaneDirectionDbaseSchema Schema = new LaneDirectionDbaseSchema();

        public LaneDirectionDbaseRecord()
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
