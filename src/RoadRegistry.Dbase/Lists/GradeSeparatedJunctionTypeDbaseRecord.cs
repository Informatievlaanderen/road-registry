// ReSharper disable InconsistentNaming
namespace RoadRegistry.Dbase.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionTypeDbaseRecord : DbaseRecord
    {
        public static readonly GradeSeparatedJunctionTypeDbaseSchema Schema = new GradeSeparatedJunctionTypeDbaseSchema();

        public GradeSeparatedJunctionTypeDbaseRecord()
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
