namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using Framework.Projections;
    using Schema;
    using Xunit;

    public class MunicipalityGeometryRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly Fixture _fixture;

        public MunicipalityGeometryRecordProjectionTests()
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
                    var expected = new MunicipalityGeometryRecord
                    {
                        NisCode = @event.NISCode,
                        Geometry = GeometryTranslator.Translate(@event.Geometry),
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
}
