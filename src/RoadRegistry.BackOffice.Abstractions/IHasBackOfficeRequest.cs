namespace RoadRegistry.BackOffice.Abstractions
{
    public interface IHasBackOfficeRequest<TBackOfficeRequest>
    {
        public TBackOfficeRequest Request { get; init; }
    }
}
