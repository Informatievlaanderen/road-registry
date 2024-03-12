namespace RoadRegistry.Jobs.Processor.Upload
{
    using Abstractions;
    using BackOffice.Abstractions.Exceptions;
    using BackOffice.Abstractions.Uploads;
    using BackOffice.Core.ProblemCodes;
    using BackOffice.Exceptions;
    using BackOffice.Extensions;
    using BackOffice.Extracts;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using Microsoft.Extensions.Hosting;
    using TicketingService.Abstractions;
    using UploadProcessorOptions = Infrastructure.Options.UploadProcessorOptions;
    using Task = System.Threading.Tasks.Task;

    public sealed class UploadProcessor : BackgroundService
    {
        private readonly UploadProcessorOptions _uploadProcessorOptions;
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly IBlobClient _blobClient;
        private readonly IMediator _mediator;
        private readonly ILogger<UploadProcessor> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public UploadProcessor(
            UploadProcessorOptions hostOptions,
            JobsContext jobsContext,
            ITicketing ticketing,
            RoadNetworkJobsBlobClient blobClient,
            IMediator mediator,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _uploadProcessorOptions = hostOptions.ThrowIfNull();
            _jobsContext = jobsContext.ThrowIfNull();
            _ticketing = ticketing.ThrowIfNull();
            _blobClient = blobClient.ThrowIfNull();
            _mediator = mediator.ThrowIfNull();
            _logger = loggerFactory.ThrowIfNull().CreateLogger<UploadProcessor>();
            _hostApplicationLifetime = hostApplicationLifetime.ThrowIfNull();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessJobs(stoppingToken);

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

                        await UpdateJobStatus(job, JobStatus.Preparing, stoppingToken);
                        await _ticketing.Pending(job.TicketId, stoppingToken);

                        var blob = await GetBlobObject(job, stoppingToken);
                        if (blob is null)
                        {
                            continue;
                        }

                        try
                        {
                            await UpdateJobStatus(job, JobStatus.Processing, stoppingToken);

                            // Send archive to be further processed
                            await using var blobStream = await blob.OpenAsync(stoppingToken);
                            var readStream = await blobStream.CopyToNewMemoryStream(stoppingToken);

                            var request = BuildRequest(job, blob, readStream);
                            await _mediator.Send(request, stoppingToken);

                            await UpdateJobStatus(job, JobStatus.Completed, stoppingToken);
                        }
                        catch (UnsupportedMediaTypeException)
                        {
                            throw ToValidationException(ProblemCode.Upload.UnsupportedMediaType);
                        }
                        catch (DownloadExtractNotFoundException)
                        {
                            throw ToValidationException(ProblemCode.Extract.NotFound);
                        }
                        catch (ExtractDownloadNotFoundException)
                        {
                            throw ToValidationException(ProblemCode.Extract.NotFound);
                        }
                        catch (ExtractRequestMarkedInformativeException)
                        {
                            throw ToValidationException(ProblemCode.Upload.UploadNotAllowedForInformativeExtract);
                        }
                        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException)
                        {
                            throw ToValidationException(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload);
                        }
                        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException)
                        {
                            throw ToValidationException(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce);
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
                        throw ToValidationException(ProblemCode.Upload.UnexpectedError);
                    }
                }
                catch (ValidationException ex)
                {
                    await _ticketing.Error(job.TicketId, new TicketError(ex.Errors.Select(x => new TicketError(x.ErrorMessage, x.ErrorCode)).ToArray()), stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Error, stoppingToken);
                }
            }
        }

        private ValidationException ToValidationException(ProblemCode problemCode)
        {
            return new ValidationException(new[]
            {
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = problemCode
                }
            }).TranslateToDutch();
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

        private object BuildRequest(Job job, BlobObject blob, Stream blobStream)
        {
            switch (job.UploadType)
            {
                case UploadType.Uploads:
                    {
                        return new BackOffice.Abstractions.Uploads.UploadExtractRequest(new UploadExtractArchiveRequest(blob.Name, blobStream, blob.ContentType), job.TicketId);
                    }
                case UploadType.Extracts:
                    {
                        if (job.DownloadId is null)
                        {
                            throw ToValidationException(ProblemCode.Upload.DownloadIdIsRequired);
                        }

                        return new BackOffice.Abstractions.Extracts.UploadExtractRequest(new DownloadId(job.DownloadId.Value), new UploadExtractArchiveRequest(blob.Name, blobStream, blob.ContentType), job.TicketId);
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
                    blobName = new BlobName(job.ReceivedBlobName + ".zip"); // For dev
                    if (!await _blobClient.BlobExistsAsync(blobName, cancellationToken))
                    {
                        throw new BlobNotFoundException(blobName);
                    }
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
