namespace RoadRegistry.Api.Oslo.ProjectionState.Responses
{
    using Aiv.Vbr.ProjectionHandling.Runner.ProjectionStates;
    using Swashbuckle.AspNetCore.Examples;

    public class ProjectionStateResponse
    {
        /// <summary>Naam van de projectie.</summary>
        public string Naam { get; }

        /// <summary>Positie tot waar de projectie reeds events van de event store heeft verwerkt.</summary>
        public long Positie { get; }

        public ProjectionStateResponse(ProjectionStateItem x)
        {
            Naam = x.Name;
            Positie = x.Position;
        }
    }

    public class ProjectionStateResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new ProjectionStateResponse(new ProjectionStateItem
            {
                Name = "ActueleWegen",
                Position = 13373
            });
        }
    }
}
