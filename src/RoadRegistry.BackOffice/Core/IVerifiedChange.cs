namespace RoadRegistry.BackOffice.Core
{
    public interface IVerifiedChange
    {
        IRequestedChange RequestedChange { get; }

        Problems Problems { get; }
    }
}
