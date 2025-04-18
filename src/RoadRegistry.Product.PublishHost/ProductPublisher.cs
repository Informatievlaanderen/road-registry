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
    using BackOffice.Extensions;
    using BackOffice.Uploads;
    using BackOffice.ZipArchiveWriters.ForProduct;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using CloudStorageClients;
    using HttpClients;
    using Infrastructure;
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
                await UpdateMetadataAndUploadToAzure(archiveDate, archiveStream, stoppingToken);
                _logger.LogInformation("Upload to Metadata completed.");
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

        private async Task UpdateMetadataAndUploadToAzure(DateTimeOffset archiveDate, MemoryStream archiveStream, CancellationToken cancellationToken)
        {
            await using var sp = _container.BeginLifetimeScope();
            var metadataClient = sp.Resolve<MetaDataCenterHttpClient>();

            var results = await metadataClient.UpdateCswPublication(
                archiveDate.DateTime,
                cancellationToken);
            if (results == null)
            {
                throw new InvalidOperationException($"Failed to update metadata to date {archiveDate}");
            }

            var pdfAsBytes = await metadataClient.GetPdfAsByteArray(cancellationToken);
            var xmlAsString = await metadataClient.GetXmlAsString(cancellationToken);

            archiveStream.Position = 0;
            await using var azureZipArchiveStream = await archiveStream.CopyToNewMemoryStreamAsync(cancellationToken);
            using var azureZipArchive = new ZipArchive(azureZipArchiveStream, ZipArchiveMode.Create, true);

            await azureZipArchive.AddToZipArchive(
                "Meta_Wegenregister.pdf",
                pdfAsBytes,
                cancellationToken);
            await azureZipArchive.AddToZipArchive(
                "Meta_Wegenregister.xml",
                xmlAsString,
                cancellationToken);

            var azureBlobClient = sp.Resolve<AzureBlobClient>();
            await azureBlobClient.UploadBlobAsync(azureZipArchiveStream, cancellationToken);
        }
    }
}
