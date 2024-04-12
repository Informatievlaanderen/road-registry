namespace RoadRegistry.Jobs.Processor
{
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
    using BackOffice.Abstractions.Jobs;
    using BackOffice.Core;
    using Microsoft.Extensions.Hosting;
    using TicketingService.Abstractions;
    using JobsProcessorOptions = Infrastructure.Options.JobsProcessorOptions;
    using Task = System.Threading.Tasks.Task;

    public sealed class JobsProcessor : BackgroundService
    {
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
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _uploadProcessorOptions = hostOptions.ThrowIfNull();
            _jobsContext = jobsContext.ThrowIfNull();
            _ticketing = ticketing.ThrowIfNull();
            _blobClient = blobClient.ThrowIfNull();
            _mediator = mediator.ThrowIfNull();
            _logger = loggerFactory.ThrowIfNull().CreateLogger<JobsProcessor>();
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
                            var readStream = await blobStream.CopyToNewMemoryStream(stoppingToken);

                            var request = BuildRequest(job, blob, readStream);
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
            return new ValidationException(new[]
            {
                validationFailure
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
                            throw ToDutchValidationException(ProblemCode.Upload.DownloadIdIsRequired);
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
