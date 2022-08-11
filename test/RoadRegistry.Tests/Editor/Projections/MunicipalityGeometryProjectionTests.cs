namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Framework.Projections;
using Xunit;
using MunicipalityGeometry = Schema.MunicipalityGeometry;

public class MunicipalityGeometryProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;

    public MunicipalityGeometryProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeMunicipalityGeometry();
    }

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
