namespace RoadRegistry.Product.PublishHost
{
    using System;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using BackOffice;
    using BackOffice.Abstractions;
    using BackOffice.Abstractions.Exceptions;
    using BackOffice.Uploads;
    using BackOffice.ZipArchiveWriters.ForProduct;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using CloudStorageClients;
    using HttpClients;
    using Infrastructure.Configurations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IO;
    using Schema;

    public class ProductPublisher
    {
        private readonly IStreetNameCache _cache;
        private readonly RoadNetworkProductBlobClient _productBlobClient;
        private readonly AzureBlobOptions _azureBlobOptions;
        private readonly ILifetimeScope _container;
        private readonly ProductContext _context;
        private readonly RecyclableMemoryStreamManager _streamManager;
        private readonly FileEncoding _fileEncoding;
        private readonly ZipArchiveWriterOptions _writerOptions;
        private readonly ILogger _logger;

        public ProductPublisher(
            ProductContext context,
            ZipArchiveWriterOptions writerOptions,
            RecyclableMemoryStreamManager streamManager,
            FileEncoding fileEncoding,
            IStreetNameCache cache,
            RoadNetworkProductBlobClient productBlobClient,
            IOptions<AzureBlobOptions> azureBlobOptions,
            ILifetimeScope container,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _writerOptions = writerOptions;
            _streamManager = streamManager;
            _fileEncoding = fileEncoding;
            _cache = cache;
            _productBlobClient = productBlobClient;
            _azureBlobOptions = azureBlobOptions.Value;
            _container = container;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var tr = await _context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, stoppingToken);

            var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(stoppingToken);
            if (info is null || !info.CompletedImport)
            {
                throw new DownloadProductNotFoundException();
            }

            var archiveDate = info.LastChangedTimestamp;
            _logger.LogInformation("Creating archive using date {Date:yyyy-MM-dd}", archiveDate);
            stoppingToken.ThrowIfCancellationRequested();

            if (await BlobExistsInS3(archiveDate, stoppingToken))
            {
                _logger.LogInformation("Blob already exists on S3, stopping execution.");
                return;
            }

            var archiveStream = new MemoryStream();
            await BuildArchive(archiveStream, stoppingToken);
            stoppingToken.ThrowIfCancellationRequested();

            await UploadToS3(archiveDate, archiveStream, stoppingToken);
            _logger.LogInformation("Upload to S3 completed.");
            stoppingToken.ThrowIfCancellationRequested();

            if (_azureBlobOptions.Enabled)
            {
                await UploadToDownload(archiveStream, stoppingToken);
                _logger.LogInformation("Upload to Download completed.");
                stoppingToken.ThrowIfCancellationRequested();

                await UpdateMetadata(archiveDate, stoppingToken);
                _logger.LogInformation("Metadata updated.");
                stoppingToken.ThrowIfCancellationRequested();
            }
            else
            {
                _logger.LogInformation("Azure blob is disabled.");
            }
        }

        private async Task BuildArchive(MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            var writer = new RoadNetworkForProductPublishToZipArchiveWriter(_writerOptions, _cache, _streamManager, _fileEncoding);
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8);
            await writer.WriteAsync(archive, _context, cancellationToken);
        }

        private async Task<bool> BlobExistsInS3(DateTimeOffset archiveDate, CancellationToken cancellationToken)
        {
            return await _productBlobClient.BlobExistsAsync(
                GetBlobName(archiveDate),
                cancellationToken);
        }

        private async Task UploadToS3(DateTimeOffset archiveDate, MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            archiveStream.Position = 0;

            await _productBlobClient.CreateBlobAsync(
                GetBlobName(archiveDate),
                Metadata.None,
                ContentType.Parse("application/octet-stream"),
                archiveStream,
                cancellationToken);
        }

        private static BlobName GetBlobName(DateTimeOffset archiveDate)
        {
            return new BlobName($"{archiveDate:yyyyMMdd}-DownloadBestand-Wegenregister.zip");
        }

        private async Task UploadToDownload(MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            archiveStream.Position = 0;

            await using var sp = _container.BeginLifetimeScope();
            var client = sp.Resolve<AzureBlobClient>();

            await client.UploadBlobAsync(archiveStream, cancellationToken);
        }

        private async Task UpdateMetadata(DateTimeOffset archiveDate, CancellationToken cancellationToken)
        {
            await using var sp = _container.BeginLifetimeScope();
            var client = sp.Resolve<MetaDataCenterHttpClient>();

            var results = await client.UpdateCswPublication(
                archiveDate.DateTime,
                cancellationToken);
            if (results == null)
            {
                _logger.LogCritical("Failed to update metadata to date {archiveDate}", archiveDate);
            }
        }
    }
}
