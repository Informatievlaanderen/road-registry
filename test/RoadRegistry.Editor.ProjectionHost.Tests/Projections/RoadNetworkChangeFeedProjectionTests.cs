namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Editor.Projections;
using Editor.Schema.RoadNetworkChanges;
using Newtonsoft.Json;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using FileProblem = BackOffice.Messages.FileProblem;

public class RoadNetworkChangeFeedProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly MemoryBlobClient _client;
    private readonly Fixture _fixture;

    public RoadNetworkChangeFeedProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeExtractRequestId();
        _client = new MemoryBlobClient();
    }

    [Fact]
    public async Task When_an_archive_for_an_extract_is_accepted()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var uploadId = _fixture.Create<UploadId>();
        var file = _fixture.Create<string>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractChangesArchiveUploaded
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            }, new RoadNetworkExtractChangesArchiveAccepted
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId,
                Problems = new[]
                {
                    new FileProblem
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
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkExtractChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            }, new RoadNetworkChangeRequestBasedOnArchive
            {
                ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                ArchiveId = archiveId
            });
    }

    [Fact]
    public async Task When_an_archive_for_an_extract_is_rejected()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var uploadId = _fixture.Create<UploadId>();
        var file = _fixture.Create<string>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractChangesArchiveUploaded
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            }, new RoadNetworkExtractChangesArchiveRejected
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId,
                Problems = new[]
                {
                    new FileProblem
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
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkExtractChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            }, new RoadNetworkChange
            {
                Id = 1,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geweigerd",
                Type = nameof(RoadNetworkExtractChangesArchiveRejected),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveRejectedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename },
                    Files = new[]
                    {
                        new FileProblems
                        {
                            File = file,
                            Problems = new[]
                            {
                                new ProblemWithFile
                                {
                                    Severity = "Error",
                                    Text = "De shape record 1 geometrie is ongeldig."
                                }
                            }
                        }
                    }
                })
            }, new RoadNetworkChangeRequestBasedOnArchive
            {
                ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                ArchiveId = archiveId
            });
    }

    [Fact]
    public async Task When_an_archive_is_accepted()
    {
        var description = _fixture.Create<ExtractDescription>();
        var file = _fixture.Create<string>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkChangesArchiveUploaded
            {
                Description = description,
                ArchiveId = archiveId
            }, new RoadNetworkChangesArchiveAccepted
            {
                Description = description,
                ArchiveId = archiveId,
                Problems = new[]
                {
                    new FileProblem
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
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                    Type = nameof(RoadNetworkChangesArchiveUploaded),
                    Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                    {
                        Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                    })
                },
                new RoadNetworkChangeRequestBasedOnArchive
                {
                    ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                    ArchiveId = archiveId
                });
    }

    [Fact]
    public async Task When_an_archive_is_rejected()
    {
        var description = _fixture.Create<ExtractDescription>();
        var file = _fixture.Create<string>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkChangesArchiveUploaded
            {
                Description = description,
                ArchiveId = archiveId
            }, new RoadNetworkChangesArchiveRejected
            {
                Description = description,
                ArchiveId = archiveId,
                Problems = new[]
                {
                    new FileProblem
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
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            }, new RoadNetworkChange
            {
                Id = 1,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geweigerd",
                Type = nameof(RoadNetworkChangesArchiveRejected),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveRejectedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename },
                    Files = new[]
                    {
                        new FileProblems
                        {
                            File = file,
                            Problems = new[]
                            {
                                new ProblemWithFile
                                {
                                    Severity = "Error",
                                    Text = "De shape record 1 geometrie is ongeldig."
                                }
                            }
                        }
                    }
                })
            }, new RoadNetworkChangeRequestBasedOnArchive
            {
                ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                ArchiveId = archiveId
            });
    }

    [Fact]
    public async Task When_extract_download_became_available()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                UploadExpected = true
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Extractaanvraag \"{description}\": download beschikbaar",
                Type = nameof(RoadNetworkExtractDownloadBecameAvailable),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractDownloadBecameAvailableEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            });
    }

    [Fact]
    public async Task When_extract_informative_download_became_available()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                UploadExpected = false
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Informatieve extractaanvraag \"{description}\": download beschikbaar",
                Type = nameof(RoadNetworkExtractDownloadBecameAvailable),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractDownloadBecameAvailableEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            });
    }

    [Fact]
    public async Task When_extract_download_timeout_occurred()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractDownloadTimeoutOccurred
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                UploadExpected = true
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Extractaanvraag \"{description}\": download niet beschikbaar, contour te complex of te groot",
                Type = nameof(RoadNetworkExtractDownloadTimeoutOccurred),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractDownloadTimeoutOccurredEntry
                {
                    ExternalRequestId = externalExtractRequestId,
                    RequestId = extractRequestId,
                    Description = description
                })
            });
    }

    [Fact]
    public async Task When_extract_informative_download_timeout_occurred()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractDownloadTimeoutOccurred
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                UploadExpected = false
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Informatieve extractaanvraag \"{description}\": download niet beschikbaar, contour te complex of te groot",
                Type = nameof(RoadNetworkExtractDownloadTimeoutOccurred),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractDownloadTimeoutOccurredEntry
                {
                    ExternalRequestId = externalExtractRequestId,
                    RequestId = extractRequestId,
                    Description = description
                })
            });
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
    public async Task When_requesting_an_extract()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractGotRequested
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                Contour = new RoadNetworkExtractGeometry { Polygon = null, MultiPolygon = Array.Empty<Polygon>(), SpatialReferenceSystemIdentifier = 0 }
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Extractaanvraag \"{description}\": ontvangen",
                Type = nameof(RoadNetworkExtractGotRequested),
                Content = null
            });
    }

    [Fact]
    public async Task When_uploading_an_archive()
    {
        var description = _fixture.Create<ExtractDescription>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkChangesArchiveUploaded
            {
                Description = description,
                ArchiveId = archiveId
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo
                    {
                        Id = archiveId,
                        Available = true,
                        Filename = filename
                    }
                })
            }, new RoadNetworkChangeRequestBasedOnArchive
            {
                ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                ArchiveId = archiveId
            });
    }

    [Fact]
    public async Task When_uploading_an_archive_for_an_extract()
    {
        var description = _fixture.Create<ExtractDescription>();
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var uploadId = _fixture.Create<UploadId>();
        var archiveId = _fixture.Create<ArchiveId>();
        var filename = _fixture.Create<string>();
        await _client.CreateBlobAsync(new BlobName(archiveId.ToString()),
            Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), filename)),
            ContentType.Parse("application/zip"), Stream.Null);

        var changeRequestId = ChangeRequestId
            .FromArchiveId(archiveId);

        await new RoadNetworkChangeFeedProjection(_client)
            .Scenario()
            .Given(new RoadNetworkExtractChangesArchiveUploaded
            {
                Description = description,
                ExternalRequestId = externalExtractRequestId,
                RequestId = extractRequestId,
                DownloadId = downloadId,
                UploadId = uploadId,
                ArchiveId = archiveId
            })
            .Expect(new RoadNetworkChange
            {
                Id = 0,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkExtractChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkExtractChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = filename }
                })
            }, new RoadNetworkChangeRequestBasedOnArchive
            {
                ChangeRequestId = changeRequestId.ToBytes().ToArray(),
                ArchiveId = archiveId
            });
    }
}
