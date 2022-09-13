namespace RoadRegistry.Projector.Infrastructure
{
    public class ProjectionDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WellKnownConnectionName { get; set; }
        public string FallbackDesiredState { get; set; }
        public bool IsSyndication { get; set; }
    }
}
