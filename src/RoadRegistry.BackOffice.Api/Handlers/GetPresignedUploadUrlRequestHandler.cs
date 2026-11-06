namespace RoadRegistry.BackOffice.Api.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using Hosts.Infrastructure.Options;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Infrastructure.Options;
    using Jobs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using TicketingService.Abstractions;

    public sealed class GetPresignedUploadUrlRequestHandler : IRequestHandler<GetPresignedUploadUrlRequest, GetPresignedUploadUrlResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly TicketingOptions _ticketingOptions;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IJobUploadUrlPresigner _urlPresigner;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JobsBucketOptions _jobsBucketOptions;

        public GetPresignedUploadUrlRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing,
            TicketingOptions ticketingOptions,
            ITicketingUrl ticketingUrl,
            IJobUploadUrlPresigner urlPresigner,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JobsBucketOptions> jobsBucketOptions)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
            _ticketingOptions = ticketingOptions;
            _ticketingUrl = ticketingUrl;
            _urlPresigner = urlPresigner;
            _httpContextAccessor = httpContextAccessor;
            _jobsBucketOptions = jobsBucketOptions.Value;
        }

        public Task<GetPresignedUploadUrlResponse> Handle(
            GetPresignedUploadUrlRequest request,
            CancellationToken cancellationToken)
        {
            return _jobsContext.Database.CreateExecutionStrategy().ExecuteAsync(request, async (_, _, _) =>
            {
                await using var transaction = await _jobsContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    if (request.DownloadId is not null)
                    {
                        await AcquireUploadLock(request.DownloadId.Value, cancellationToken);
                        await ThrowIfAnUploadIsPending(request.DownloadId.Value, cancellationToken);
                    }

                    var job = await CreateJob(request.UploadType, request.DownloadId, request.DryRun, cancellationToken);

                    var preSignedUrl = _urlPresigner.CreatePresignedUploadUrl(job);

                    var ticketId = await _ticketing.CreateTicket(
                        new Dictionary<string, string>
                        {
                            { "Registry", "RoadRegistry" },
                            { "Action", "Upload" },
                            { "UploadType", request.UploadType.ToString() },
                            { "DownloadId", request.DownloadId?.ToString() },
                        },
                        cancellationToken);

                    var ticketUrl = _ticketingUrl.For(ticketId).ToString().Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl);

                    await UpdateJobWithTicketId(job, ticketId, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new GetPresignedUploadUrlResponse(job.Id, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketId, ticketUrl);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }, null, cancellationToken);
        }

        // Two requests for the same extract arriving together would both find no upload pending and both create a job.
        // The lock makes the second wait until the first has committed its job, which it then finds. It is held by the
        // transaction, so it is released on commit or rollback.
        private async Task AcquireUploadLock(DownloadId downloadId, CancellationToken cancellationToken)
        {
            if (!_jobsContext.Database.IsSqlServer())
            {
                return;
            }

            var resource = $"RoadRegistryJobs.Upload.{downloadId.ToGuid():N}";
            await _jobsContext.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @result int;
EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 30000;
IF @result < 0 THROW 50000, 'Could not acquire the upload lock for this extract.', 1;", cancellationToken);
        }

        // Clearing LatestUploadId when an upload url is handed out leaves nothing to say an upload is on its way until
        // the uploaded file has been picked up and processed. The job created for the url says so in the meantime.
        // A job that is still waiting for its file past the timeout never got one: the portal uploads right away.
        private async Task ThrowIfAnUploadIsPending(DownloadId downloadId, CancellationToken cancellationToken)
        {
            var abandonedBefore = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
                .AddMinutes(-_jobsBucketOptions.PendingUploadTimeoutInMinutes);
            var downloadGuid = downloadId.ToGuid();

            var uploadIsPending = await _jobsContext.Jobs.AnyAsync(job =>
                job.DownloadId == downloadGuid
                && (job.Status == JobStatus.Processing
                    || (job.Status == JobStatus.Created && job.Created > abandonedBefore)),
                cancellationToken);

            if (uploadIsPending)
            {
                throw UploadNotFinished.Exception();
            }
        }

        private async Task<Job> CreateJob(
            UploadType uploadType,
            DownloadId? downloadId,
            bool dryRun,
            CancellationToken cancellationToken)
        {
            var @operator = _httpContextAccessor.HttpContext!.GetOperator();

            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                JobStatus.Created,
                uploadType,
                Guid.Empty)
            {
                DownloadId = downloadId,
                OperatorName = @operator,
                DryRun = dryRun
            };

            await _jobsContext.Jobs.AddAsync(job, cancellationToken);
            await _jobsContext.SaveChangesAsync(cancellationToken);

            return job;
        }

        private async Task UpdateJobWithTicketId(Job job, Guid ticketId, CancellationToken cancellationToken)
        {
            job.TicketId = ticketId;
            await _jobsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
