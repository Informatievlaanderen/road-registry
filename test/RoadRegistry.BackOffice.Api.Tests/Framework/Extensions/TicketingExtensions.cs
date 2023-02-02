namespace RoadRegistry.BackOffice.Api.Tests.Framework.Extensions;

using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TicketingService.Abstractions;
using TicketingService.Proxy.HttpProxy;

public class FakeHttpProxyTicketing : ITicketing
{
    public Task<Guid> CreateTicket(IDictionary<string, string> metadata = null, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<IEnumerable<Ticket>> GetAll(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(Enumerable.Empty<Ticket>());
    }

    public Task<Ticket> Get(Guid ticketId, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(new Ticket(ticketId, TicketStatus.Complete, new Dictionary<string, string>(), new TicketResult()));
    }

    public Task Pending(Guid ticketId, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

    public Task Complete(Guid ticketId, TicketResult result, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

    public Task Error(Guid ticketId, TicketError error, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

    public Task Delete(Guid ticketId, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;
}

public static class TicketingExtensions
{
    private static IServiceCollection AddHttpProxyTicketing(
        this IServiceCollection services,
        Func<IConfiguration, string> baseUrlProvider)
    {
        services.AddHttpClient<ITicketing, FakeHttpProxyTicketing>(_ => new FakeHttpProxyTicketing());

        return services;
    }

    public static IServiceCollection AddTicketing(
        this IServiceCollection services)
    {
        return services
            .AddHttpProxyTicketing(GetBaseUrl)
            .AddSingleton<ITicketingUrl>(sp =>
                new TicketingUrl(GetBaseUrl(sp.GetRequiredService<IConfiguration>()))
            );
    }

    private static string GetBaseUrl(IConfiguration configuration)
    {
        return configuration.GetSection(TicketingOptions.ConfigurationKey).GetRequiredValue<string>(nameof(TicketingOptions.InternalBaseUrl));
    }
}
