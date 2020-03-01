namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class SurfaceTypeDbaseRecord : DbaseRecord
    {
        public static readonly SurfaceTypeDbaseSchema Schema = new SurfaceTypeDbaseSchema();

        public SurfaceTypeDbaseRecord()
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
