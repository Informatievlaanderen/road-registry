//namespace RoadRegistry.Jobs
//{
//    using Be.Vlaanderen.Basisregisters.Shaperon;

//    public class JobResultDbaseRecord : DbaseRecord
//    {
//        public static readonly JobResultDbaseSchema Schema = new JobResultDbaseSchema();

//        public DbaseInt32 Idn { get; }
//        public DbaseInt32 GrbObject { get; set; }
//        public DbaseCharacter GrId { get; }

//        public JobResultDbaseRecord()
//        {
//            Idn = new DbaseInt32(Schema.GrbIdn);
//            GrbObject = new DbaseInt32(Schema.GrbObject);
//            GrId = new DbaseCharacter(Schema.GrId);

//            Values = new DbaseFieldValue[]
//            {
//                Idn,
//                GrbObject,
//                GrId
//            };
//        }
//    }
//}
