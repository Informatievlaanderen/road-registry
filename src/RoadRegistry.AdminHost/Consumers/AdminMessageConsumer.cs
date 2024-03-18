namespace RoadRegistry.AdminHost.Consumers;

using BackOffice;
using BackOffice.Abstractions;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BackOffice.Core.ProblemCodes;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Hosts.Infrastructure;
using Infrastructure.Options;
using TicketingService.Abstractions;

public class AdminMessageConsumer
{
    private readonly SqsQueueUrlOptions _sqsQueueUrlOptions;
    private readonly ISqsQueueConsumer _sqsConsumer;
    private readonly SqsOptions _sqsOptions;
    private readonly AdminHostOptions _adminHostOptions;
    private readonly ITicketing _ticketing;
    private readonly ILogger _logger;
    private readonly ILifetimeScope _container;

    public AdminMessageConsumer(
        ILifetimeScope container,
        SqsQueueUrlOptions sqsQueueUrlOptions,
        ISqsQueueConsumer sqsQueueConsumer,
        SqsOptions sqsOptions,
        AdminHostOptions adminHostOptions,
        ITicketing ticketing,
        ILogger<AdminMessageConsumer> logger
     )
    {
        _container = container.ThrowIfNull();
        _sqsQueueUrlOptions = sqsQueueUrlOptions.ThrowIfNull();
        _sqsConsumer = sqsQueueConsumer.ThrowIfNull();
        _sqsOptions = sqsOptions.ThrowIfNull();
        _adminHostOptions = adminHostOptions.ThrowIfNull();
        _ticketing = ticketing.ThrowIfNull();
        _logger = logger.ThrowIfNull();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        do
        {
            try
            {
                var lastMessage = await _sqsConsumer.Consume(_sqsQueueUrlOptions.Admin, async message =>
                {
                    var sqsMessageType = message.GetType();
                    _logger.LogInformation("SQS message '{Type}' received", sqsMessageType.FullName);
                    
                    var backOfficeRequest = GetBackOfficeRequestFromSqsRequest(message);
                    if (backOfficeRequest is not null)
                    {
                        _logger.LogInformation("Continuing with BackOffice request of type '{Type}'", backOfficeRequest.GetType().FullName);
                    }

                    var request = backOfficeRequest ?? message;

                    if (request is HealthCheckSqsRequest)
                    {
                        return;
                    }

                    var ticketId = (message as SqsRequest)?.TicketId;
                    if (ticketId is not null)
                    {
                        await _ticketing.Pending(ticketId.Value, cancellationToken);
                    }

                    try
                    {
                        await using var lifetimeScope = _container.BeginLifetimeScope();
                        var mediator = lifetimeScope.Resolve<IMediator>();

                        var result = await mediator.Send(request, cancellationToken);
                        _logger.LogInformation("SQS message result: {Result}", JsonConvert.SerializeObject(result, _sqsOptions.JsonSerializerSettings));

                        if (ticketId is not null)
                        {
                            await _ticketing.Complete(ticketId.Value, new TicketResult(), cancellationToken);
                        }
                    }
                    catch
                    {
                        if (ticketId is not null)
                        {
                            await _ticketing.Error(ticketId.Value, new TicketError(), cancellationToken);
                        }
                        throw;
                    }
                }, cancellationToken);

                if (lastMessage?.Error is not null)
                {
                    _logger.LogError("SQS message processing failed: {Error}", lastMessage.Error);
                }
                else if (string.IsNullOrEmpty(lastMessage?.Message?.Type))
                {
                    _logger.LogInformation("No SQS message received");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"An unhandled exception has occurred: {ex.Message}");
            }
            finally
            {
                if (_adminHostOptions.AlwaysRunning && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(new TimeSpan(0, 0, 30), cancellationToken);
                }
            }
        } while (_adminHostOptions.AlwaysRunning && !cancellationToken.IsCancellationRequested);
    }

    private static object GetBackOfficeRequestFromSqsRequest(object sqsMessage)
    {
        var backOfficeRequestType = sqsMessage.GetType()
            .GetInterfaces()
            .Where(iType => iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IHasBackOfficeRequest<>))
            .Select(iType => iType.GetGenericArguments()[0])
            .FirstOrDefault();

        if (backOfficeRequestType is not null)
        {
            var requestProperty = sqsMessage.GetType().GetProperty(nameof(IHasBackOfficeRequest<object>.Request));
            return requestProperty!.GetValue(sqsMessage);
        }

        return null;
    }
}
