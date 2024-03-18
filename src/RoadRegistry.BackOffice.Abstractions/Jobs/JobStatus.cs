namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    public enum JobStatus
    {
        Created = 1,

        Processing = 2,

        Completed = 4,
        Cancelled = 5,
        Error = 99
    }
}
