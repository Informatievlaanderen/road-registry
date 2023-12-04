namespace RoadRegistry.BackOffice.Messages;

using System;

public class ImportedOriginProperties: IHaveTransactionId
{
    public string Application { get; set; }
    public string Operator { get; set; }
    public string Organization { get; set; }
    public string OrganizationId { get; set; }
    public DateTime Since { get; set; }
    public int TransactionId { get; set; }
}
