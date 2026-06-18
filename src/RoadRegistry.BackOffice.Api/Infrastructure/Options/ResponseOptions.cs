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
        public string GelijkGrondseKruisingNaamruimte { get; set; }
        public string GelijkGrondseKruisingDetailUrlFormat { get; set; }
        public string OngelijkGrondseKruisingNaamruimte { get; set; }
        public string OngelijkGrondseKruisingDetailUrlFormat { get; set; }
    }
}
