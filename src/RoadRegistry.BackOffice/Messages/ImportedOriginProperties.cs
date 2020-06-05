namespace RoadRegistry.BackOffice.Messages
{
    using System;

    public class ImportedOriginProperties
    {
        public string OrganizationId { get; set; }
        public string Organization { get; set; }
        public string Operator { get; set; }
        public string Application { get; set; }
        public DateTime Since { get; set; }
        public int? TransactionId { get; set; }
    }
}
