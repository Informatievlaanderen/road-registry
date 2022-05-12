// ReSharper disable InconsistentNaming
namespace RoadRegistry.Dbase.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class LaneDirectionDbaseRecord : DbaseRecord
    {
        public static readonly LaneDirectionDbaseSchema Schema = new LaneDirectionDbaseSchema();

        public LaneDirectionDbaseRecord()
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
