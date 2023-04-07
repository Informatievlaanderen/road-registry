namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithInvalidGeometry : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithInvalidGeometryFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometry(WhenChangeOutlineGeometryWithInvalidGeometryFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Middellijngeometrie is foutief";
}
