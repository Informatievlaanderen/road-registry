namespace RoadRegistry.BackOffice.Projections
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Framework.Testing.Projections;
    using Messages;
    using Newtonsoft.Json;
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
        public Task When_import_began()
        {
            return new RoadNetworkChangeFeedProjection()
                .Scenario()
                .Given(new BeganRoadNetworkImport())
                .Expect(new RoadNetworkChange
                {
                    Id = 0,
                    Title = "Begonnen met importeren",
                    Type = nameof(BeganRoadNetworkImport),
                    Content = null
                });
        }

        [Fact]
        public Task When_import_completed()
        {
            return new RoadNetworkChangeFeedProjection()
                .Scenario()
                .Given(new BeganRoadNetworkImport(), new CompletedRoadNetworkImport())
                .Expect(new RoadNetworkChange
                    {
                        Id = 0,
                        Title = "Begonnen met importeren",
                        Type = nameof(BeganRoadNetworkImport),
                        Content = null
                    },
                    new RoadNetworkChange
                    {
                        Id = 1,
                        Title = "Klaar met importeren",
                        Type = nameof(CompletedRoadNetworkImport),
                        Content = null
                    });
        }

        [Fact]
        public Task When_uploading_an_archive()
        {
            var archiveId = _fixture.Create<ArchiveId>();
            return new RoadNetworkChangeFeedProjection()
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                })
                .Expect(new RoadNetworkChange
                {
                    Id = 0,
                    Title = "Oplading bestand ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        ArchiveId = archiveId
                    })
                });
        }

        [Fact]
        public Task When_an_archive_is_accepted()
        {
            var file = _fixture.Create<string>();
            var archiveId = _fixture.Create<ArchiveId>();
            return new RoadNetworkChangeFeedProjection()
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
                .Expect(new RoadNetworkChange
                {
                    Id = 0,
                    Title = "Oplading bestand ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        ArchiveId = archiveId
                    })
                }, new RoadNetworkChange
                {
                    Id = 1,
                    Title = "Oplading bestand werd aanvaard",
                    Type = nameof(RoadNetworkChangesArchiveAccepted),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveAcceptedEntry
                    {
                        ArchiveId = archiveId,
                        Files = new[]
                        {
                            new RoadNetworkChangesArchiveFile
                            {
                                File = file,
                                Problems = new[]
                                {
                                    "De shape record 1 geometrie is ongeldig."
                                }
                            }
                        }
                    })
                });
        }

        [Fact]
        public Task When_an_archive_is_rejected()
        {
            var file = _fixture.Create<string>();
            var archiveId = _fixture.Create<ArchiveId>();
            return new RoadNetworkChangeFeedProjection()
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                }, new RoadNetworkChangesArchiveRejected
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
                .Expect(new RoadNetworkChange
                {
                    Id = 0,
                    Title = "Oplading bestand ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        ArchiveId = archiveId
                    })
                }, new RoadNetworkChange
                {
                    Id = 1,
                    Title = "Oplading bestand werd geweigerd",
                    Type = nameof(RoadNetworkChangesArchiveRejected),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveRejectedEntry
                    {
                        ArchiveId = archiveId,
                        Files = new[]
                        {
                            new RoadNetworkChangesArchiveFile
                            {
                                File = file,
                                Problems = new[]
                                {
                                    "De shape record 1 geometrie is ongeldig."
                                }
                            }
                        }
                    })
                });
        }
    }
}
