namespace RoadRegistry.Model
{
    using AutoFixture;

    public static class Customizations
    {
        public static void CustomizeRoadNodeType(this IFixture fixture)
        {
            fixture.Customize<RoadNodeType>(composer =>
                composer.FromFactory<int>(value => RoadNodeType.All[value % RoadNodeType.All.Length]));
        }
    }
}