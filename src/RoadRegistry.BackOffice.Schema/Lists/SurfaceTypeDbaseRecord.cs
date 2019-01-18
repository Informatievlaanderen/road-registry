namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class SurfaceTypeDbaseRecord : DbaseRecord
    {
        private static readonly SurfaceTypeDbaseSchema Schema = new SurfaceTypeDbaseSchema();

        public SurfaceTypeDbaseRecord()
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
