namespace RoadRegistry.BackOffice.Api.Infrastructure.Options
{
    public class ResponseOptions
    {
        public const string ConfigKey = "ResponseOptions";

        public string StraatnaamDetailUrlFormat { get; set; }
        public string WegsegmentNaamruimte { get; set; }
        public string WegsegmentDetailUrlFormat { get; set; }
        public string WegknoopNaamruimte { get; set; }
        public string WegknoopDetailUrlFormat { get; set; }
        public string GelijkgrondseKruisingNaamruimte { get; set; }
        public string GelijkgrondseKruisingDetailUrlFormat { get; set; }
        public string OngelijkgrondseKruisingNaamruimte { get; set; }
        public string OngelijkgrondseKruisingDetailUrlFormat { get; set; }
    }
}
