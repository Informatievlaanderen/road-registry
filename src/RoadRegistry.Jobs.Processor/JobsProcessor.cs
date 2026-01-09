namespace RoadRegistry.Jobs.Processor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Abstractions;
    using BackOffice.Abstractions.Exceptions;
    using BackOffice.Abstractions.Jobs;
    using BackOffice.Abstractions.Uploads;
    using BackOffice.Exceptions;
    using BackOffice.Extracts;
    using BackOffice.Handlers.Sqs.Extracts;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Extensions;
    using Extracts;
    using Extracts.Infrastructure.Extensions;
    using Extracts.Schema;
    using Extracts.Uploads;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RoadRegistry.Infrastructure;
    using RoadRegistry.Infrastructure.DutchTranslations;
    using TicketingService.Abstractions;
    using ValueObjects.ProblemCodes;
    using ValueObjects.Problems;
    using JobsProcessorOptions = Infrastructure.Options.JobsProcessorOptions;
    using Task = System.Threading.Tasks.Task;

    public sealed class JobsProcessor : BackgroundService
    {
        private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
        private readonly ExtractsDbContext _extractsDbContext;
        private readonly IExtractRequestCleaner _extractRequestCleaner;
        private readonly JobsProcessorOptions _uploadProcessorOptions;
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly IBlobClient _blobClient;
        private readonly IMediator _mediator;
        private readonly ILogger<JobsProcessor> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public JobsProcessor(
            JobsProcessorOptions hostOptions,
            JobsContext jobsContext,
            ITicketing ticketing,
            RoadNetworkJobsBlobClient blobClient,
            IMediator mediator,
            IExtractRequestCleaner extractRequestCleaner,
            RoadNetworkUploadsBlobClient uploadsBlobClient,
            ExtractsDbContext extractsDbContext,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _uploadProcessorOptions = hostOptions.ThrowIfNull();
            _jobsContext = jobsContext.ThrowIfNull();
            _ticketing = ticketing.ThrowIfNull();
            _blobClient = blobClient.ThrowIfNull();
            _mediator = mediator.ThrowIfNull();
            _extractRequestCleaner = extractRequestCleaner.ThrowIfNull();
            _uploadsBlobClient = uploadsBlobClient;
            _extractsDbContext = extractsDbContext;
            _logger = loggerFactory.ThrowIfNull().CreateLogger<JobsProcessor>();
            _hostApplicationLifetime = hostApplicationLifetime.ThrowIfNull();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessJobs(stoppingToken);
                await _extractRequestCleaner.CloseOldExtracts(stoppingToken);

                if (!_uploadProcessorOptions.AlwaysRunning)
                {
                    _hostApplicationLifetime.StopApplication();
                    return;
                }

                Thread.Sleep(TimeSpan.FromSeconds(_uploadProcessorOptions.ConsumerDelaySeconds));
            }
        }

        private async Task<ICollection<Job>> GetJobsReadyToBeProcessed(CancellationToken cancellationToken)
        {
            return await _jobsContext.Jobs
                .Where(x => x.Status == JobStatus.Created && x.TicketId != Guid.Empty)
                .OrderBy(x => x.Created)
                .ToListAsync(cancellationToken);
        }

        private async Task ProcessJobs(CancellationToken stoppingToken)
        {
            var jobs = await GetJobsReadyToBeProcessed(stoppingToken);

            foreach (var job in jobs)
            {
                try
                {
                    try
                    {
                        if (job.IsExpired(TimeSpan.FromMinutes(_uploadProcessorOptions.MaxJobLifeTimeInMinutes)))
                        {
                            await CancelJob(job, stoppingToken);
                            continue;
                        }

                        var blob = await GetBlobObject(job, stoppingToken);
                        if (blob is null)
                        {
                            continue;
                        }

                        try
                        {
                            await _ticketing.Pending(job.TicketId, stoppingToken);
                            await UpdateJobStatus(job, JobStatus.Processing, stoppingToken);

                            // Send archive to be further processed
                            await using var blobStream = await blob.OpenAsync(stoppingToken);
                            var readStream = await blobStream.CopyToNewMemoryStreamAsync(stoppingToken);

                            var request = await BuildRequest(job, blob, readStream, stoppingToken);

                            if (request is EndpointRequest endpointRequest)
                            {
                                endpointRequest.ProvenanceData =
                                    new RoadRegistryProvenanceData(Modification.Unknown, job.OperatorName);
                            }

                            await _mediator.Send(request, stoppingToken);

                            await UpdateJobStatus(job, JobStatus.Completed, stoppingToken);
                        }
                        catch (UnsupportedMediaTypeException ex)
                        {
                            if (ex.ContentType is not null)
                            {
                                var problem = new UnsupportedMediaType(ex.ContentType.Value).Translate();
                                throw ToDutchValidationException(problem.ToValidationFailure());
                            }
                            throw ToDutchValidationException(ProblemCode.Upload.UnsupportedMediaType);
                        }
                        catch (DownloadExtractNotFoundException)
                        {
                            throw ToDutchValidationException(ProblemCode.Extract.NotFound);
                        }
                        catch (ExtractDownloadNotFoundException)
                        {
                            throw ToDutchValidationException(ProblemCode.Extract.NotFound);
                        }
                        catch (ExtractRequestMarkedInformativeException)
                        {
                            throw ToDutchValidationException(ProblemCode.Upload.UploadNotAllowedForInformativeExtract);
                        }
                        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException)
                        {
                            throw ToDutchValidationException(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload);
                        }
                        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException)
                        {
                            throw ToDutchValidationException(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce);
                        }
                        catch (ZipArchiveValidationException ex)
                        {
                            throw ex.ToDutchValidationException();
                        }
                        finally
                        {
                            await _blobClient.DeleteBlobAsync(blob.Name, stoppingToken);
                        }
                    }
                    catch (ValidationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unexpected exception for job '{job.Id}'");
                        throw ToDutchValidationException(ProblemCode.Upload.UnexpectedError);
                    }
                }
                catch (ValidationException ex)
                {
                    await _ticketing.Error(job.TicketId, new TicketError(ex.Errors.Select(x => new TicketError(x.ErrorMessage, $"{x.PropertyName}_{x.ErrorCode}".Trim().Trim('_'))).ToArray()), stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Error, stoppingToken);
                }
            }
        }

        private DutchValidationException ToDutchValidationException(ProblemCode problemCode)
        {
            return ToDutchValidationException(new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = problemCode
                });
        }
        private DutchValidationException ToDutchValidationException(ValidationFailure validationFailure)
        {
            return new ValidationException([
                validationFailure
            ]).TranslateToDutch();
        }

        private async Task CancelJob(Job job, CancellationToken stoppingToken)
        {
            await _ticketing.Complete(
                job.TicketId,
                new TicketResult(new { JobStatus = "Cancelled" }),
                stoppingToken);

            await UpdateJobStatus(job, JobStatus.Cancelled, stoppingToken);

            _logger.LogWarning("Cancelled expired job '{jobId}'.", job.Id);
        }

        private async Task<object> BuildRequest(Job job, BlobObject blob, Stream blobStream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (job.UploadType)
            {
                case UploadType.Uploads:
                    {
                        return new UploadExtractRequest(new UploadExtractArchiveRequest(blob.Name, blobStream, blob.ContentType), job.TicketId);
                    }
                case UploadType.Extracts:
                    {
                        if (job.DownloadId is null)
                        {
                            throw ToDutchValidationException(ProblemCode.Upload.DownloadIdIsRequired);
                        }

                        return new BackOffice.Abstractions.Extracts.UploadExtractRequest(new DownloadId(job.DownloadId.Value), new UploadExtractArchiveRequest(blob.Name, blobStream, blob.ContentType), job.TicketId);
                    }
                case UploadType.ExtractsV2:
                    {
                        if (job.DownloadId is null)
                        {
                            throw ToDutchValidationException(ProblemCode.Extract.DownloadIdIsRequired);
                        }

                        var uploadId = new UploadId(Guid.NewGuid());
                        var fileNames = blob.Metadata
                            .Where(pair => pair.Key == new MetadataKey("filename"))
                            .Select(x => x.Value)
                            .ToArray();
                        var fileName = fileNames.Length == 1 ? fileNames.Single() : $"{uploadId}.zip";
                        var metadata = Metadata.None.Add(
                            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), fileName)
                        );

                        await _uploadsBlobClient.CreateBlobAsync(
                            new BlobName(uploadId.ToString()),
                            metadata,
                            blob.ContentType,
                            blobStream,
                            cancellationToken
                        );

                        var extractDownload = await _extractsDbContext.ExtractDownloads
                            .SingleAsync(x => x.DownloadId == job.DownloadId.Value, cancellationToken);
                        var extractRequestId = ExtractRequestId.FromString(extractDownload.ExtractRequestId);

                        if (extractDownload.ZipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2)
                        {
                            var extractRequest = await _extractsDbContext.ExtractRequests
                                .SingleAsync(x => x.ExtractRequestId == extractDownload.ExtractRequestId, cancellationToken);

                            return new UploadExtractSqsRequestV2
                            {
                                TicketId = job.TicketId,
                                DownloadId = new DownloadId(job.DownloadId.Value),
                                UploadId = uploadId,
                                ExtractRequestId = extractRequestId,
                                SendFailedEmail = extractRequest.ExternalRequestId is not null,
                                ProvenanceData = new RoadRegistryProvenanceData(operatorName: job.OperatorName, reason: extractRequest.Description)
                            };
                        }

                        return new UploadExtractSqsRequest
                        {
                            TicketId = job.TicketId,
                            DownloadId = new DownloadId(job.DownloadId.Value),
                            UploadId = uploadId,
                            ExtractRequestId = extractRequestId,
                            ProvenanceData = new RoadRegistryProvenanceData(operatorName: job.OperatorName)
                        };
                    }
                case UploadType.Inwinning:
                    {
                        if (job.DownloadId is null)
                        {
                            throw ToDutchValidationException(ProblemCode.Extract.DownloadIdIsRequired);
                        }

                        var uploadId = new UploadId(Guid.NewGuid());
                        var fileNames = blob.Metadata
                            .Where(pair => pair.Key == new MetadataKey("filename"))
                            .Select(x => x.Value)
                            .ToArray();
                        var fileName = fileNames.Length == 1 ? fileNames.Single() : $"{uploadId}.zip";
                        var metadata = Metadata.None.Add(
                            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), fileName)
                        );

                        await _uploadsBlobClient.CreateBlobAsync(
                            new BlobName(uploadId.ToString()),
                            metadata,
                            blob.ContentType,
                            blobStream,
                            cancellationToken
                        );

                        var extractDownload = await _extractsDbContext.ExtractDownloads
                            .SingleAsync(x => x.DownloadId == job.DownloadId.Value, cancellationToken);

                        var extractRequest = await _extractsDbContext.ExtractRequests
                            .SingleAsync(x => x.ExtractRequestId == extractDownload.ExtractRequestId, cancellationToken);

                        return new UploadInwinningExtractSqsRequest
                        {
                            TicketId = job.TicketId,
                            DownloadId = new DownloadId(job.DownloadId.Value),
                            UploadId = uploadId,
                            ExtractRequestId = ExtractRequestId.FromString(extractDownload.ExtractRequestId),
                            ProvenanceData = new RoadRegistryProvenanceData(operatorName: job.OperatorName, reason: extractRequest.Description),
                        };
                    }
                default:
                    throw new NotSupportedException($"{nameof(UploadType)} {job.UploadType} is not supported.");
            }
        }

        private async Task UpdateJobStatus(Job job, JobStatus status, CancellationToken cancellationToken)
        {
            job.UpdateStatus(status);
            await _jobsContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<BlobObject> GetBlobObject(Job job, CancellationToken cancellationToken)
        {
            try
            {
                var blobName = new BlobName(job.ReceivedBlobName);
                if (!await _blobClient.BlobExistsAsync(blobName, cancellationToken))
                {
                    throw new BlobNotFoundException(blobName);
                }

                var blobObject = await _blobClient.GetBlobAsync(blobName, cancellationToken);
                return blobObject;
            }
            catch (BlobNotFoundException)
            {
                _logger.LogError($"No blob found with name '{job.ReceivedBlobName}' for job {job.Id}");
                return null;
            }
        }
    }
}
