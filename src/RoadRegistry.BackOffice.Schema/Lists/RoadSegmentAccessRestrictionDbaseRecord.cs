namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new RoadSegmentAccessRestrictionDbaseSchema();

        public RoadSegmentAccessRestrictionDbaseRecord()
        {
            TYPE = new DbaseNumber(Schema.TYPE);
            LBLTYPE = new DbaseCharacter(Schema.LBLTYPE);
            DEFTYPE = new DbaseCharacter(Schema.DEFTYPE);

            Values = new DbaseFieldValue[]
            {
                TYPE, LBLTYPE, DEFTYPE
            };
        }

        public DbaseNumber TYPE { get; }
        public DbaseCharacter LBLTYPE { get; }
        public DbaseCharacter DEFTYPE { get; }
    }
}
