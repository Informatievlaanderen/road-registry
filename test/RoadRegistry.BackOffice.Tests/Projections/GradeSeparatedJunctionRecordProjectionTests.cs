namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Framework.Testing.Projections;
    using Messages;
    using Microsoft.IO;
    using Model;
    using Schema.GradeSeparatedJunctions;
    using Xunit;

    public class GradeSeparatedJunctionRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public GradeSeparatedJunctionRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
            _fixture.CustomizeGradeSeparatedJunctionType();
            _fixture.CustomizeGradeSeparatedJunctionId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizeReason();
            _fixture.CustomizeOperatorName();
            _fixture.CustomizeArchiveId();
            _fixture.CustomizeOriginProperties();
            _fixture.CustomizeImportedGradeSeparatedJunction();
            _fixture.CustomizeGradeSeparatedJunctionAdded();
        }

        [Fact]
        public Task When_importing_grade_separated_junctions()
        {
            var data = _fixture
                .CreateMany<ImportedGradeSeparatedJunction>()
                .Select(junction =>
                {
                    var expected = new GradeSeparatedJunctionRecord
                    {
                        Id = junction.Id,
                        DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                        {
                            OK_OIDN = { Value = junction.Id },
                            TYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier },
                            LBLTYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name },
                            BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                            ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                            BEGINTIJD = { Value = junction.Origin.Since },
                            BEGINORG = { Value = junction.Origin.OrganizationId },
                            LBLBGNORG = { Value = junction.Origin.Organization }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    };

                    return new
                    {
                        junction,
                        expected
                    };
                }).ToList();

            return new GradeSeparatedJunctionRecordProjection(new RecyclableMemoryStreamManager(),  Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.junction))
                .Expect(data.Select(d => d.expected));
        }

        [Fact]
        public Task When_adding_grade_separated_junctions()
        {
            var message = _fixture.Create<Messages.RoadNetworkChangesBasedOnArchiveAccepted>();
            var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var junction = change.GradeSeparatedJunctionAdded;
                return (object)new GradeSeparatedJunctionRecord
                {
                    Id = junction.Id,
                    DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                    {
                        OK_OIDN = {Value = junction.Id},
                        TYPE = {Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier},
                        LBLTYPE = {Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name},
                        BO_WS_OIDN = {Value = junction.UpperRoadSegmentId},
                        ON_WS_OIDN = {Value = junction.LowerRoadSegmentId},
                        BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(message.When)},
                        BEGINORG = {Value = message.OrganizationId},
                        LBLBGNORG = {Value = message.Organization}
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                };
            });

            return new GradeSeparatedJunctionRecordProjection(_services.MemoryStreamManager,  Encoding.UTF8)
                .Scenario()
                .Given(message)
                .Expect(expectedRecords);
        }
    }
}
