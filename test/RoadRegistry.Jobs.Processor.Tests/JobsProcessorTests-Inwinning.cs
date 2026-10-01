namespace RoadRegistry.Jobs.Processor.Tests
{
    using System.IO.Compression;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Abstractions.Jobs;
    using BackOffice.FeatureToggles;
    using BackOffice.Handlers.Sqs.Extracts;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Extracts;
    using Extracts.Infrastructure.Extensions;
    using Extracts.Schema;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
    using RoadRegistry.Tests.BackOffice.Scenarios;
    using TicketingService.Abstractions;

    public partial class JobsProcessorTests
    {
        [Fact]
        public async Task FlowTest_Inwinning()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            var mockTicketing = new Mock<ITicketing>();
            var blobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Inwinning, ticketId)
            {
                DownloadId = downloadId,
                OperatorName = fixture.Create<string>().Substring(0, 20)
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var blobFileName = "file.zip";
            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    Metadata.None
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), blobFileName))
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("guardduty-malware-scan-status"), "NO_THREATS_FOUND")),
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("valid.zip"))));

            var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
            var extractRequestId = fixture.Create<ExtractRequestId>();
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                Description = fixture.Create<string>()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2
            });
            await extractsDbContext.SaveChangesAsync();

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(blobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                extractsDbContext,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.RunOnceAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            mockTicketing.Verify(x =>
                    x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            var createBlobInvocation = blobClient.Invocations
                .Single(x => x.Method.Name == nameof(IBlobClient.CreateBlobAsync));
            var blobMetadataFilename = createBlobInvocation
                .Arguments.OfType<Metadata>()
                .Single()
                .Single(x => x.Key == "filename");
            blobMetadataFilename.Value.Should().Be(blobFileName);
            var blobMetadataMalwareFound = createBlobInvocation
                .Arguments.OfType<Metadata>()
                .Single()
                .Single(x => x.Key == "guardduty-malware-scan-status");
            blobMetadataMalwareFound.Value.Should().Be("NO_THREATS_FOUND");

            var executedRequest = Assert.IsType<UploadInwinningExtractSqsRequest>(mockMediator.Invocations.Single().Arguments.First());
            executedRequest.TicketId.Should().Be(ticketId);
            executedRequest.DownloadId.ToGuid().Should().Be(downloadId);
            executedRequest.ProvenanceData!.Operator.Should().Be(job.OperatorName);
            createBlobInvocation.Arguments.OfType<BlobName>().Single().ToString().Should().Be(executedRequest.UploadId.ToString());

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Inwinning_MissingDownloadId()
        {
            var mockTicketing = new Mock<ITicketing>();
            var blobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Inwinning, ticketId)
            {
                DownloadId = null
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null!,
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("valid.zip"))));

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(blobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
                new FakeExtractsDbContextFactory().CreateDbContext(),
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.RunOnceAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(ticketError =>
                    ticketError.Errors.First().ErrorCode == "DownloadIdVerplicht"
                    && ticketError.Errors.First().ErrorMessage == "Download id is verplicht."),
                It.IsAny<CancellationToken>()), Times.Once);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Inwinning_UnknownFilesAreDeleted()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            var mockTicketing = new Mock<ITicketing>();
            var blobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Inwinning, ticketId)
            {
                DownloadId = downloadId,
                OperatorName = fixture.Create<string>().Substring(0, 20)
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            using var archiveStream = (await EmbeddedResourceReader.ReadAsync("inwinning_unknownfiles.zip")).CopyToNewMemoryStream();
            using var zipArchive = new ZipArchive(archiveStream);
            var unknownFileNames = new[] { "aaa.txt", "bbb.txt" };
            foreach (var unknownFileName in unknownFileNames)
            {
                zipArchive.GetEntry(unknownFileName).Should().NotBeNull();
            }

            var blobFileName = "file.zip";
            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    Metadata.None
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), blobFileName))
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("guardduty-malware-scan-status"), "NO_THREATS_FOUND")),
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("inwinning_unknownfiles.zip"))));

            var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
            var extractRequestId = fixture.Create<ExtractRequestId>();
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                Description = fixture.Create<string>()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2
            });
            await extractsDbContext.SaveChangesAsync();

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(blobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                extractsDbContext,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.RunOnceAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            Assert.IsType<UploadInwinningExtractSqsRequest>(mockMediator.Invocations.Single().Arguments.First());

            var createBlobInvocation = blobClient.Invocations
                .Single(x => x.Method.Name == nameof(IBlobClient.CreateBlobAsync));
            var createdBlobStream = createBlobInvocation
                .Arguments.OfType<Stream>()
                .Single();
            await using var createdBlobStreamCopy = await createdBlobStream.CopyToNewMemoryStreamAsync(CancellationToken.None);
            using var blobZipArchive = new ZipArchive(createdBlobStreamCopy);
            foreach (var unknownFileName in unknownFileNames)
            {
                blobZipArchive.GetEntry(unknownFileName).Should().BeNull();
            }
        }

        public static IEnumerable<object[]> InwinningArchivesWithSingleSubfolder()
        {
            yield return new object[] { new[] { "extract/eWegknoop.dbf", "extract/Transactiezones.dbf", "extract/aaa.txt" } };
            yield return new object[] { new[] { "extract/", "extract/eWegknoop.dbf", "extract/Transactiezones.dbf", "extract/aaa.txt" } };
            yield return new object[] { new[] { "a/", "a/b/", "a/b/c/", "a/b/c/eWegknoop.dbf", "a/b/c/Transactiezones.dbf", "a/b/c/aaa.txt" } };
            yield return new object[] { new[] { @"a\b\eWegknoop.dbf", @"a\b\Transactiezones.dbf", @"a\b\aaa.txt" } };
            yield return new object[] { new[] { "extract/", "extract/eWegknoop.dbf", "extract/Transactiezones.dbf", "__MACOSX/", "__MACOSX/extract/", "__MACOSX/extract/._aaa.txt" } };
            yield return new object[] { new[] { "extract/eWegknoop.dbf", "extract/Transactiezones.dbf", "extract/__cache/aaa.txt" } };
        }

        [Theory]
        [MemberData(nameof(InwinningArchivesWithSingleSubfolder))]
        public async Task FlowTest_Inwinning_FilesInSingleSubfolderAreMovedToRoot(string[] entryNames)
        {
            var resultEntries = await RunInwinningJobWithArchive(entryNames);

            resultEntries.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "eWegknoop.dbf", entryNames.Single(x => x.EndsWith("eWegknoop.dbf")) },
                { "Transactiezones.dbf", entryNames.Single(x => x.EndsWith("Transactiezones.dbf")) }
            });
        }

        [Fact]
        public async Task FlowTest_Inwinning_FilesInDifferentSubfoldersAreNotMoved()
        {
            var resultEntries = await RunInwinningJobWithArchive(["a/eWegknoop.dbf", "b/Transactiezones.dbf"]);

            resultEntries.Should().BeEmpty();
        }

        [Fact]
        public async Task FlowTest_Inwinning_FilesInRootAndSubfolderAreNotMoved()
        {
            var resultEntries = await RunInwinningJobWithArchive(["Transactiezones.dbf", "a/", "a/eWegknoop.dbf"]);

            resultEntries.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "Transactiezones.dbf", "Transactiezones.dbf" }
            });
        }

        /// <summary>
        /// Runs an inwinning upload job for a zip with the given entries (each file contains its own original name)
        /// and returns the entries of the uploaded zip as name -> content.
        /// </summary>
        private static async Task<Dictionary<string, string>> RunInwinningJobWithArchive(string[] entryNames)
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            var blobClient = new Mock<IBlobClient>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var downloadId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Inwinning, Guid.NewGuid())
            {
                DownloadId = downloadId,
                OperatorName = fixture.Create<string>().Substring(0, 20)
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var archiveBytes = CreateArchive(entryNames);

            var blobName = new BlobName(job.ReceivedBlobName);
            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    Metadata.None
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), "file.zip"))
                        .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("guardduty-malware-scan-status"), "NO_THREATS_FOUND")),
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(new MemoryStream(archiveBytes))));

            var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
            var extractRequestId = fixture.Create<ExtractRequestId>();
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                Description = fixture.Create<string>()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2
            });
            await extractsDbContext.SaveChangesAsync();

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                Mock.Of<ITicketing>(),
                new RoadNetworkJobsBlobClient(blobClient.Object),
                Mock.Of<IMediator>(),
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                extractsDbContext,
                new NullLoggerFactory(),
                Mock.Of<IHostApplicationLifetime>());

            // Act
            await sut.RunOnceAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            var createdBlobStream = blobClient.Invocations
                .Single(x => x.Method.Name == nameof(IBlobClient.CreateBlobAsync))
                .Arguments.OfType<Stream>()
                .Single();
            await using var createdBlobStreamCopy = await createdBlobStream.CopyToNewMemoryStreamAsync(CancellationToken.None);
            using var resultArchive = new ZipArchive(createdBlobStreamCopy);
            return resultArchive.Entries.ToDictionary(
                entry => entry.FullName,
                entry =>
                {
                    using var reader = new StreamReader(entry.Open());
                    return reader.ReadToEnd();
                });
        }

        private static byte[] CreateArchive(string[] entryNames)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var entryName in entryNames)
                {
                    var entry = archive.CreateEntry(entryName);
                    if (!entryName.EndsWith('/'))
                    {
                        using var writer = new StreamWriter(entry.Open());
                        writer.Write(entryName);
                    }
                }
            }

            return stream.ToArray();
        }
    }
}
