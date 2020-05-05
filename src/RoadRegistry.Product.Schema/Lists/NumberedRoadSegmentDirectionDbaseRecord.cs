namespace RoadRegistry.Product.Schema.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadSegmentDirectionDbaseRecord : DbaseRecord
    {
        public static readonly NumberedRoadSegmentDirectionDbaseSchema Schema = new NumberedRoadSegmentDirectionDbaseSchema();

        public NumberedRoadSegmentDirectionDbaseRecord()
        {
            RICHTING = new DbaseInt32(Schema.RICHTING);
            LBLRICHT = new DbaseString(Schema.LBLRICHT);
            DEFRICHT = new DbaseString(Schema.DEFRICHT);

            Values = new DbaseFieldValue[]
            {
                RICHTING, LBLRICHT, DEFRICHT
            };
        }

        public DbaseInt32 RICHTING { get; }
        public DbaseString LBLRICHT { get; }
        public DbaseString DEFRICHT { get; }
    }
}
