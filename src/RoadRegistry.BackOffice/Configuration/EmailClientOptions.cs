namespace RoadRegistry.BackOffice.Configuration
{
    public class EmailClientOptions : IHasConfigurationKey
    {
        public string FromEmailAddress { get; set; }
        public string ExtractUploadFailed { get; set; }

        public string GetConfigurationKey() => "EmailClientOptions";
    }
}
