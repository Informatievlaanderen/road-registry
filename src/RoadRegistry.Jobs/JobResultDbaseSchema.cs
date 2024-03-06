//namespace RoadRegistry.Jobs
//{
//    using Be.Vlaanderen.Basisregisters.Shaperon;

//    public class JobResultDbaseSchema : DbaseSchema
//    {
//        public DbaseField GrbIdn => Fields[0];
//        public DbaseField GrbObject => Fields[1];
//        public DbaseField GrId => Fields[2];

//        public JobResultDbaseSchema() => Fields = new[]
//        {
//            DbaseField.CreateNumberField(new DbaseFieldName("GRBIDN"), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
//            DbaseField.CreateNumberField(new DbaseFieldName("GRBOBJECT"), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
//            DbaseField.CreateCharacterField(new DbaseFieldName("GRID"), new DbaseFieldLength(100))
//        };
//    }
//}
