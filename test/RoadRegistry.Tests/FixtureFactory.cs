namespace RoadRegistry.Tests;

using AutoFixture;

public static class FixtureFactory
{
    public static Fixture Create()
    {
        var fixture = new Fixture();
        fixture.Customize(new RandomBooleanSequenceCustomization());
        return fixture;
    }
}
