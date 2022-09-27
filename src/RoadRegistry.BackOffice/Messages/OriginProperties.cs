namespace RoadRegistry.BackOffice.Messages;

// TODO: Push this from command to event side via value objects
public class OriginProperties
{
    public string Operator { get; set; }
    public string Organization { get; set; }
    public string Reason { get; set; }
    public string Time { get; set; }
}
