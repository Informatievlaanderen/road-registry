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
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;
    using Task = System.Threading.Tasks.Task;

    public sealed class UploadProcessor : BackgroundService
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly IBlobClient _blobClient;
        private readonly IMediator _mediator;
        private readonly ILogger<UploadProcessor> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public UploadProcessor(
            JobsContext jobsContext,
            ITicketing ticketing,
            RoadNetworkJobsBlobClient blobClient,
            IMediator mediator,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _jobsContext = jobsContext.ThrowIfNull();
            _ticketing = ticketing.ThrowIfNull();
            _blobClient = blobClient.ThrowIfNull();
            _mediator = mediator.ThrowIfNull();
            _logger = loggerFactory.ThrowIfNull().CreateLogger<UploadProcessor>();
            _hostApplicationLifetime = hostApplicationLifetime.ThrowIfNull();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check created jobs
            var jobs = await _jobsContext.Jobs
                .Where(x => x.Status == JobStatus.Created || x.Status == JobStatus.Preparing)
                .OrderBy(x => x.Created)
                .ToListAsync(stoppingToken);

            if (!jobs.Any())
            {
                _hostApplicationLifetime.StopApplication();
                return;
            }

            foreach (var job in jobs)
            {
                async Task UpdateJobWithError(ProblemCode problemCode)
                {
                    var ex = new ValidationException(new[]
                    {
                        new ValidationFailure
                        {
                            PropertyName = string.Empty,
                            ErrorCode = problemCode
                        }
                    }).TranslateToDutch();

                    await _ticketing.Error(job.TicketId!.Value, new TicketError(ex.Errors.Select(x => new TicketError(x.ErrorMessage, x.ErrorCode)).ToArray()), stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Error, stoppingToken);
                }

                try
                {
                    var blob = await GetBlobObject(job, stoppingToken);
                    if (blob is null)
                    {
                        continue;
                    }

                    // If so, update ticket status and job status => Preparing
                    await _ticketing.Pending(job.TicketId!.Value, stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Preparing, stoppingToken);

                    // Send archive to be further processed
                    var request = await BuildRequest(job, blob, stoppingToken);
                    await _mediator.Send(request, stoppingToken);

                    await UpdateJobStatus(job, JobStatus.Completed, stoppingToken);
                }
                catch (UnsupportedMediaTypeException)
                {
                    await UpdateJobWithError(ProblemCode.Upload.UnsupportedMediaType);
                }
                catch (DownloadExtractNotFoundException)
                {
                    await UpdateJobWithError(ProblemCode.Extract.NotFound);
                }
                catch (ExtractDownloadNotFoundException)
                {
                    await UpdateJobWithError(ProblemCode.Extract.NotFound);
                }
                catch (ExtractRequestMarkedInformativeException)
                {
                    await UpdateJobWithError(ProblemCode.Upload.UploadNotAllowedForInformativeExtract);
                }
                catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException)
                {
                    await UpdateJobWithError(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload);
                }
                catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException)
                {
                    await UpdateJobWithError(ProblemCode.Upload.CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected exception for job '{job.Id}'");
                    await UpdateJobWithError(ProblemCode.Upload.UnexpectedError);
                }
            }

            _hostApplicationLifetime.StopApplication();
        }

        private async Task<object> BuildRequest(Job job, BlobObject blob, CancellationToken stoppingToken)
        {
            var ticket = await _ticketing.Get(job.TicketId!.Value, stoppingToken);
            var uploadType = Enum.Parse(typeof(UploadType), ticket!.Metadata["Type"]);

            await using var stream = await blob.OpenAsync(stoppingToken);

            switch (uploadType)
            {
                case UploadType.Uploads:
                    return new BackOffice.Abstractions.Uploads.UploadExtractRequest(new UploadExtractArchiveRequest(blob.Name, stream, blob.ContentType));
                case UploadType.Extracts:
                    return new BackOffice.Abstractions.Extracts.UploadExtractRequest(ticket.Metadata["DownloadId"], new UploadExtractArchiveRequest(blob.Name, stream, blob.ContentType));
                default:
                    throw new NotSupportedException($"{nameof(UploadType)} {uploadType} is not supported.");
            }
        }

        private async Task UpdateJobStatus(Job job, JobStatus status, CancellationToken stoppingToken)
        {
            job.UpdateStatus(status);
            await _jobsContext.SaveChangesAsync(stoppingToken);
        }
        
        private async Task<BlobObject> GetBlobObject(Job job, CancellationToken stoppingToken)
        {
            var blobName = new BlobName(job.ReceivedBlobName);

            if (!await _blobClient.BlobExistsAsync(blobName, stoppingToken))
            {
                return null;
            }

            var blobObject = await _blobClient.GetBlobAsync(blobName, stoppingToken);
            if (blobObject is null)
            {
                _logger.LogError($"No blob found with name: {job.ReceivedBlobName}");
                return null;
            }
            
            try
            {
                return blobObject;
            }
            catch (BlobNotFoundException)
            {
                _logger.LogError($"No blob found with name: {job.ReceivedBlobName}");
                return null;
            }
        }
    }
}
