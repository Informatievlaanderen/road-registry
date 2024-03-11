namespace RoadRegistry.BackOffice.Api.Handlers
{
    using Hosts.Infrastructure.Modules;
    using Infrastructure.Options;
    using Jobs;
    using Jobs.Abstractions;
    using MediatR;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Hosts.Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using TicketingService.Abstractions;

    public sealed class UploadPreSignedUrlRequestHandler : IRequestHandler<UploadPreSignedUrlRequest, UploadPreSignedUrlResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly TicketingOptions _ticketingOptions;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IAmazonS3Extended _s3Extended;
        private readonly JobsBucketOptions _bucketOptions;

        public UploadPreSignedUrlRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing,
            TicketingOptions ticketingOptions,
            ITicketingUrl ticketingUrl,
            IAmazonS3Extended s3Extended,
            IOptions<JobsBucketOptions> bucketOptions)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
            _ticketingOptions = ticketingOptions;
            _ticketingUrl = ticketingUrl;
            _s3Extended = s3Extended;
            _bucketOptions = bucketOptions.Value;
        }

        public Task<UploadPreSignedUrlResponse> Handle(
            UploadPreSignedUrlRequest request,
            CancellationToken cancellationToken)
        {
            return _jobsContext.Database.CreateExecutionStrategy().ExecuteAsync(request, async (_, _, _) =>
            {
                await using var transaction = await _jobsContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var job = await CreateJob(cancellationToken);

                    var preSignedUrl = _s3Extended.CreatePresignedPost(
                        new CreatePresignedPostRequest(
                            _bucketOptions.BucketName,
                            job.UploadBlobName,
                            new List<ExactMatchCondition>(),
                            TimeSpan.FromMinutes(_bucketOptions.UrlExpirationInMinutes)));
                    
                    var ticketId = await _ticketing.CreateTicket(
                        new Dictionary<string, string>
                        {
                            { "Registry", "RoadRegistry" },
                            { "Action", "Upload" },
                            { "Type", request.Type.ToString() },
                            { "DownloadId", request.DownloadId ?? string.Empty },
                            { "UploadId", job.Id.ToString("D") }
                        },
                        cancellationToken);

                    var ticketUrl = _ticketingUrl.For(ticketId).ToString().Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl);
                    
                    await UpdateJobWithTicketUrl(job, ticketId, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new UploadPreSignedUrlResponse(job.Id, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketUrl);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }, null, cancellationToken);
        }

        private async Task<Job> CreateJob(CancellationToken cancellationToken)
        {
            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                JobStatus.Created);

            await _jobsContext.Jobs.AddAsync(job, cancellationToken);
            await _jobsContext.SaveChangesAsync(cancellationToken);

            return job;
        }

        private async Task UpdateJobWithTicketUrl(Job job, Guid ticketId, CancellationToken cancellationToken)
        {
            job.TicketId = ticketId;
            await _jobsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
