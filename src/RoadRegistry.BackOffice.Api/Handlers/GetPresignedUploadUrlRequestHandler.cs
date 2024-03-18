namespace RoadRegistry.BackOffice.Api.Handlers
{
    using Hosts.Infrastructure.Options;
    using Infrastructure;
    using Jobs;
    using MediatR;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using TicketingService.Abstractions;

    public sealed class GetPresignedUploadUrlRequestHandler : IRequestHandler<GetPresignedUploadUrlRequest, GetPresignedUploadUrlResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;
        private readonly TicketingOptions _ticketingOptions;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IJobUploadUrlPresigner _urlPresigner;

        public GetPresignedUploadUrlRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing,
            TicketingOptions ticketingOptions,
            ITicketingUrl ticketingUrl,
            IJobUploadUrlPresigner urlPresigner)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
            _ticketingOptions = ticketingOptions;
            _ticketingUrl = ticketingUrl;
            _urlPresigner = urlPresigner;
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
                    var job = await CreateJob(request.UploadType, request.DownloadId, cancellationToken);

                    var preSignedUrl = _urlPresigner.CreatePresignedUploadUrl(job);
                    
                    var ticketId = await _ticketing.CreateTicket(
                        new Dictionary<string, string>
                        {
                            { "Registry", "RoadRegistry" },
                            { "Action", "Upload" },
                            { "UploadType", request.UploadType.ToString() },
                            { "DownloadId", request.DownloadId?.ToString() },
                            { "JobId", job.Id.ToString("D") }
                        },
                        cancellationToken);

                    var ticketUrl = _ticketingUrl.For(ticketId).ToString().Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl);
                    
                    await UpdateJobWithTicketId(job, ticketId, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new GetPresignedUploadUrlResponse(job.Id, preSignedUrl.Url.ToString(), preSignedUrl.Fields, ticketUrl);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }, null, cancellationToken);
        }
        
        private async Task<Job> CreateJob(UploadType uploadType, DownloadId? downloadId, CancellationToken cancellationToken)
        {
            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                JobStatus.Created,
                uploadType,
                Guid.Empty)
            {
                DownloadId = downloadId
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
