namespace RoadRegistry.Editor.Projections
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Framework.Projections;
    using Newtonsoft.Json;
    using Schema;
    using Xunit;

    public class RoadNetworkChangeFeedProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly Fixture _fixture;
        private readonly MemoryBlobClient _client;

        public RoadNetworkChangeFeedProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeArchiveId();
            _client = new MemoryBlobClient();
        }

        [Fact]
        public Task When_import_began()
        {
            return new RoadNetworkChangeFeedProjection(_client)
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
            return new RoadNetworkChangeFeedProjection(_client)
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
        public async Task When_uploading_an_archive()
        {
            var archiveId = _fixture.Create<ArchiveId>();
            var filename = _fixture.Create<string>();
            await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
                Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
                ContentType.Parse("application/zip"), Stream.Null);

            await new RoadNetworkChangeFeedProjection(_client)
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
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                    })
                });
        }

        [Fact]
        public async Task When_an_archive_is_accepted()
        {
            var file = _fixture.Create<string>();
            var archiveId = _fixture.Create<ArchiveId>();
            var filename = _fixture.Create<string>();
            await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
                Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
                ContentType.Parse("application/zip"), Stream.Null);

            await new RoadNetworkChangeFeedProjection(_client)
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                }, new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = archiveId,
                    Problems = new[]
                    {
                        new BackOffice.Messages.FileProblem
                        {
                            File = file,
                            Severity = ProblemSeverity.Error,
                            Reason = nameof(ShapeFileProblems.ShapeRecordGeometryMismatch),
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
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                    })
                }, new RoadNetworkChange
                {
                    Id = 1,
                    Title = "Oplading bestand werd aanvaard",
                    Type = nameof(RoadNetworkChangesArchiveAccepted),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveAcceptedEntry
                    {
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = filename },
                        Files = new[]
                        {
                            new RoadNetworkChangesArchiveFile
                            {
                                File = file,
                                Problems = new []
                                {
                                    new RoadNetworkChangesArchiveFileProblem
                                    {
                                        Severity = "Error",
                                        Text = "De shape record 1 geometrie is ongeldig."
                                    }
                                }
                            }
                        }
                    })
                });
        }

        [Fact]
        public async Task When_an_archive_is_rejected()
        {
            var file = _fixture.Create<string>();
            var archiveId = _fixture.Create<ArchiveId>();
            var filename = _fixture.Create<string>();
            await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
                Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
                ContentType.Parse("application/zip"), Stream.Null);
            await new RoadNetworkChangeFeedProjection(_client)
                .Scenario()
                .Given(new RoadNetworkChangesArchiveUploaded
                {
                    ArchiveId = archiveId
                }, new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = archiveId,
                    Problems = new[]
                    {
                        new BackOffice.Messages.FileProblem
                        {
                            File = file,
                            Severity = ProblemSeverity.Error,
                            Reason = nameof(ShapeFileProblems.ShapeRecordGeometryMismatch),
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
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                    })
                }, new RoadNetworkChange
                {
                    Id = 1,
                    Title = "Oplading bestand werd geweigerd",
                    Type = nameof(RoadNetworkChangesArchiveRejected),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveRejectedEntry
                    {
                        Archive = new RoadNetworkChangesArchiveInfo { Id = archiveId, Available = true, Filename = filename },
                        Files = new[]
                        {
                            new RoadNetworkChangesArchiveFile
                            {
                                File = file,
                                Problems = new[]
                                {
                                    new RoadNetworkChangesArchiveFileProblem
                                    {
                                        Severity = "Error",
                                        Text = "De shape record 1 geometrie is ongeldig."
                                    }
                                }
                            }
                        }
                    })
                });
        }
    }
}
