namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithInvalidGeometrySrid : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithInvalidGeometrySridFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometrySrid(WhenChangeOutlineGeometryWithInvalidGeometrySridFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieCRSNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie heeft niet het coördinatenstelsel Lambert 72.";
}
