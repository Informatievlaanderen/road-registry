namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;

public class WhenChangeOutlineGeometryWithInvalidGeometrySrid : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithInvalidGeometrySridFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometrySrid(WhenChangeOutlineGeometryWithInvalidGeometrySridFixture fixture) : base(fixture)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieCRSNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie heeft niet het coördinatenstelsel Lambert 72.";
}
