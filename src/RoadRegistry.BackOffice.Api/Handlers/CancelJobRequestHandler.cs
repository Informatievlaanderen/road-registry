namespace RoadRegistry.BackOffice.Api.Handlers
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Jobs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using TicketingService.Abstractions;

    public sealed class CancelJobRequestHandler : IRequestHandler<CancelJobRequest, CancelJobResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketing _ticketing;

        public CancelJobRequestHandler(
            JobsContext jobsContext,
            ITicketing ticketing)
        {
            _jobsContext = jobsContext;
            _ticketing = ticketing;
        }

        public async Task<CancelJobResponse> Handle(CancelJobRequest request, CancellationToken cancellationToken)
        {
            var job = await _jobsContext.FindJob(request.JobId, cancellationToken);
            if (job is null)
            {
                throw new ApiException("Onbestaande upload job.", StatusCodes.Status404NotFound);
            }

            Exception NewCancelException() => new ApiException(
                $"De status van de upload job '{request.JobId}' is {job.Status.ToString().ToLower()}, hierdoor kan deze job niet geannuleerd worden.",
                StatusCodes.Status400BadRequest);

            if (job.Status != JobStatus.Created)
            {
                throw NewCancelException();
            }
            
            var ticket = await _ticketing.Get(job.TicketId, cancellationToken);

            await _ticketing.Complete(
                job.TicketId,
                ticket!.Result is not null && ticket.Status == TicketStatus.Error
                    ? new TicketResult(new {JobStatus = "Cancelled", Error = System.Text.Json.JsonSerializer.Deserialize<TicketError>(ticket.Result.ResultAsJson ?? "{}")})
                    : new TicketResult(new {JobStatus = "Cancelled"}),
                cancellationToken);

            job.UpdateStatus(JobStatus.Cancelled);
            await _jobsContext.SaveChangesAsync(cancellationToken);

            return new CancelJobResponse();
        }
    }
}

