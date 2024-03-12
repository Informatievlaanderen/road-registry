namespace RoadRegistry.Jobs
{
    public enum JobStatus
    {
        Created = 1,

        Preparing = 2,
        Processing = 3,

        Completed = 4,
        Cancelled = 5,
        Error = 99
    }
}
