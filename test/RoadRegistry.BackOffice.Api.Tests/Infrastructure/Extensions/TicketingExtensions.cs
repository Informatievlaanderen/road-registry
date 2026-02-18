namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Extensions;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TicketingService.Abstractions;

public class FakeHttpProxyTicketing : ITicketing
{
    public Task<Guid> CreateTicket(IDictionary<string, string> metadata = null, CancellationToken cancellationToken = new())
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<IEnumerable<Ticket>> GetAll(CancellationToken cancellationToken = new())
    {
        return Task.FromResult(Enumerable.Empty<Ticket>());
    }

    public Task<Ticket> Get(Guid ticketId, CancellationToken cancellationToken = new())
    {
        return Task.FromResult(new Ticket(ticketId, TicketStatus.Complete, new Dictionary<string, string>(), new TicketResult()));
    }

    public Task Pending(Guid ticketId, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public Task Pending(Guid ticketId, TicketResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public Task Complete(Guid ticketId, TicketResult result, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task Error(Guid ticketId, TicketError error, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task Delete(Guid ticketId, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task Error(Guid ticketId, TicketError[] errors, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }
}

public static class TicketingExtensions
{
    public static IServiceCollection AddFakeTicketing(this IServiceCollection services)
    {
        return services
                .AddHttpProxyTicketing()
            ;
    }

    private static IServiceCollection AddHttpProxyTicketing(this IServiceCollection services)
    {
        services.AddHttpClient<ITicketing, FakeHttpProxyTicketing>(_ => new FakeHttpProxyTicketing());
        return services;
    }
}
