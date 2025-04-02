namespace RoadRegistry.Product.PublishHost
{
    using System;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly AzureBlobClient _azureBlobClient;
        private readonly AzureBlobOptions _azureBlobOptions;
        private readonly MetaDataCenterHttpClient _metaDataCenterHttpClient;
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
            AzureBlobClient azureBlobClient,
            IOptions<AzureBlobOptions> azureBlobOptions,
            MetaDataCenterHttpClient metaDataCenterHttpClient,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _writerOptions = writerOptions;
            _streamManager = streamManager;
            _fileEncoding = fileEncoding;
            _cache = cache;
            _productBlobClient = productBlobClient;
            _azureBlobClient = azureBlobClient;
            _azureBlobOptions = azureBlobOptions.Value;
            _metaDataCenterHttpClient = metaDataCenterHttpClient;
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

        private async Task UploadToS3(DateTimeOffset archiveDate, MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            archiveStream.Position = 0;

            await _productBlobClient.CreateBlobAsync(
                new BlobName($"{archiveDate:yyyyMMdd}-DownloadBestand-Wegenregister"),
                Metadata.None,
                ContentType.Parse("application/octet-stream"),
                archiveStream,
                cancellationToken);
        }

        private async Task UploadToDownload(MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            archiveStream.Position = 0;

            await _azureBlobClient.UploadBlobAsync(archiveStream, cancellationToken);
        }

        private async Task UpdateMetadata(DateTimeOffset archiveDate, CancellationToken cancellationToken)
        {
            var results = await _metaDataCenterHttpClient.UpdateCswPublication(
                archiveDate.DateTime,
                cancellationToken);
            if (results == null)
            {
                _logger.LogCritical("Failed to update metadata to date {archiveDate}", archiveDate);
            }
        }
    }
}
