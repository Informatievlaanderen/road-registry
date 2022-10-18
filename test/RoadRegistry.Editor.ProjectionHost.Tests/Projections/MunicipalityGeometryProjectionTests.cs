namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using MunicipalityGeometry = Editor.Schema.MunicipalityGeometry;

public class MunicipalityGeometryProjectionTests : IClassFixture<ProjectionTestServices>
{
    public MunicipalityGeometryProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeMunicipalityGeometry();
    }

    private readonly Fixture _fixture;

    [Fact]
    public Task When_municipalities_are_imported()
    {
        var data = _fixture
            .CreateMany<ImportedMunicipality>(new Random().Next(1, 1))
            .Select((@event, i) =>
            {
                var expected = new MunicipalityGeometry
                {
                    NisCode = @event.NISCode,
                    Geometry = GeometryTranslator.Translate(@event.Geometry)
                };
                return new
                {
                    ImportedMunicipality = @event,
                    Expected = expected
                };
            }).ToList();

        return new MunicipalityGeometryProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedMunicipality))
            .Expect(data.Select(d => d.Expected));
    }
}
