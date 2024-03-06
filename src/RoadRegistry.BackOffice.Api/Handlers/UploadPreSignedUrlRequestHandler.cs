namespace RoadRegistry.BackOffice.Api.Handlers
{
    using Hosts.Infrastructure.Modules;
    using Jobs;
    using MediatR;
    using NodaTime;
    using RoadRegistry.BackOffice.Abstractions.Uploads;
    using RoadRegistry.BackOffice.Api.Infrastructure.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;

    public sealed class UploadPreSignedUrlRequestHandler : IRequestHandler<UploadPreSignedUrlRequest, UploadPreSignedUrlResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IAmazonS3Extended _s3Extended;
        private readonly JobsBucketOptions _bucketOptions;

        public UploadPreSignedUrlRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            IAmazonS3Extended s3Extended,
            JobsBucketOptions bucketOptions)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
            _s3Extended = s3Extended;
            _bucketOptions = bucketOptions;
        }

        public async Task<UploadPreSignedUrlResponse> Handle(
            UploadPreSignedUrlRequest request,
            CancellationToken cancellationToken)
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
                        { "Action", "Upload" }, //TODO-rik or "ExtractUpload"
                        { "UploadId", job.Id.ToString("D") }
                    },
                    cancellationToken);

                var ticketUrl = _ticketingUrl.For(ticketId).ToString();

                await UpdateJobWithTicketUrl(job, ticketId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new UploadPreSignedUrlResponse(job.Id, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketUrl);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<Job> CreateJob(CancellationToken cancellationToken)
        {
            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                Jobs.JobStatus.Created);

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
