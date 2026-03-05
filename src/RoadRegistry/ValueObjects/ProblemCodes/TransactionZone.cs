namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class TransactionZone
    {
        public static readonly ProblemCode HasChanged = new("TransactionZoneHasChanged");
    }
}
