namespace RoadRegistry.Editor.Schema.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new RoadSegmentAccessRestrictionDbaseSchema();

        public RoadSegmentAccessRestrictionDbaseRecord()
        {
            TYPE = new DbaseInt32(Schema.TYPE);
            LBLTYPE = new DbaseString(Schema.LBLTYPE);
            DEFTYPE = new DbaseString(Schema.DEFTYPE);

            Values = new DbaseFieldValue[]
            {
                TYPE, LBLTYPE, DEFTYPE
            };
        }

        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseString DEFTYPE { get; }
    }
}
