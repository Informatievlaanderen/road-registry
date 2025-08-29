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
    using Jobs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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

        public GetPresignedUploadUrlRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing,
            TicketingOptions ticketingOptions,
            ITicketingUrl ticketingUrl,
            IJobUploadUrlPresigner urlPresigner,
            IHttpContextAccessor httpContextAccessor)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
            _ticketingOptions = ticketingOptions;
            _ticketingUrl = ticketingUrl;
            _urlPresigner = urlPresigner;
            _httpContextAccessor = httpContextAccessor;
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
                            { "DownloadId", request.DownloadId?.ToString() }
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

        private async Task<Job> CreateJob(
            UploadType uploadType,
            DownloadId? downloadId,
            CancellationToken cancellationToken)
        {
            var operatorName = _httpContextAccessor.HttpContext!.GetOperatorName();

            var job = new Job(
                SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                JobStatus.Created,
                uploadType,
                Guid.Empty)
            {
                DownloadId = downloadId,
                OperatorName = operatorName,
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
