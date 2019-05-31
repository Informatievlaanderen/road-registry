namespace RoadRegistry.BackOffice.Projections
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Framework.Testing.Projections;
    using Messages;
    using Schema;
    using Translation;
    using Xunit;

    public class RoadNetworkActivityProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadNetworkActivityProjectionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public Task When_uploading_an_archive()
        {
            var archiveId = _fixture.Create<ArchiveId>();
            return new RoadNetworkActivityProjection()
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                })
                .Expect(new RoadNetworkChangesArchiveUploadedActivity
                {
                    Id = 1,
                    ArchiveId = archiveId,
                    Title = "Oplading bestand ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded)
                });
        }

        [Fact]
        public Task When_an_archive_is_accepted()
        {
            var file = _fixture.Create<string>();
            var archiveId = _fixture.Create<ArchiveId>();
            return new RoadNetworkActivityProjection()
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                }, new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = archiveId,
                    Problems = new[]
                    {
                        new Messages.FileProblem
                        {
                            File = file,
                            Severity = FileProblemSeverity.Error,
                            Reason = nameof(ZipArchiveProblems.ShapeRecordGeometryMismatch),
                            Parameters = new[]
                            {
                                new ProblemParameter
                                {
                                    Name = "RecordNumber", Value = "1"
                                },
                                new ProblemParameter
                                {
                                    Name = "ExpectedShapeType", Value = "Point"
                                },
                                new ProblemParameter
                                {
                                    Name = "ActualShapeType", Value = "PolygonM"
                                }
                            }
                        }
                    }
                })
                .Expect(new RoadNetworkChangesArchiveUploadedActivity
                {
                    Id = 1,
                    ArchiveId = archiveId,
                    Title = "Oplading bestand ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded)
                }, new RoadNetworkChangesArchiveAcceptedActivity
                {
                    Id = 2,
                    ArchiveId = archiveId,
                    Title = "Oplading bestand werd aanvaard",
                    Type = nameof(RoadNetworkChangesArchiveAccepted),
                    Files = new[]
                    {
                        new RoadNetworkChangesArchiveAcceptedFile
                        {
                            File = file,
                            Problems = new[]
                            {
                                "De shape record 1 geometrie is ongeldig."
                            },
                            AllProblems = "De shape record 1 geometrie is ongeldig."
                        }
                    }
                });
        }
    }
}
