namespace RoadRegistry.Model
{
    using AutoFixture;

    public class ScenarioFixture : Fixture
    {
        public ScenarioFixture()
        {
            this.CustomizeRoadNodeType();
        }
    }
}