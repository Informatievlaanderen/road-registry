namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using Editor.Schema.GradeSeparatedJunctions;
using Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;
using Microsoft.IO;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class GradeSeparatedJunctionRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public GradeSeparatedJunctionRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = FixtureFactory.Create();
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

        _fixture.CustomizeRoadNetworkChangesAccepted();

        _fixture.CustomizeGradeSeparatedJunctionAdded();
        _fixture.CustomizeGradeSeparatedJunctionModified();
        _fixture.CustomizeGradeSeparatedJunctionRemoved();
    }

    [Fact]
    public Task When_adding_grade_separated_junctions()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<GradeSeparatedJunctionAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionAdded;
            return (object)new GradeSeparatedJunctionRecord
            {
                Id = junction.Id,
                UpperRoadSegmentId = junction.UpperRoadSegmentId,
                LowerRoadSegmentId = junction.LowerRoadSegmentId,
                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = junction.Id },
                    TYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier },
                    LBLTYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name },
                    BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                    ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            };
        });

        return new GradeSeparatedJunctionRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
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
                    UpperRoadSegmentId = junction.UpperRoadSegmentId,
                    LowerRoadSegmentId = junction.LowerRoadSegmentId,
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

        return new GradeSeparatedJunctionRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.junction))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionModified>());

        var expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionModified.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionModified;
            return (object)new GradeSeparatedJunctionRecord
            {
                Id = junction.Id,
                UpperRoadSegmentId = junction.UpperRoadSegmentId,
                LowerRoadSegmentId = junction.LowerRoadSegmentId,
                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = junction.Id },
                    TYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier },
                    LBLTYPE = { Value = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name },
                    BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                    ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionModified.When) },
                    BEGINORG = { Value = acceptedGradeSeparatedJunctionModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedGradeSeparatedJunctionModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            };
        });

        return new GradeSeparatedJunctionRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionRemoved>());

        return new GradeSeparatedJunctionRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved)
            .ExpectNone();
    }
}
