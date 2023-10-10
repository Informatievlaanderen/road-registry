namespace RoadRegistry.BackOffice;

using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Configuration;
using Extracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class ExtractUploadFailedEmailClient : IExtractUploadFailedEmailClient
{
    private readonly AmazonSimpleEmailServiceV2Client _emailClient;
    private readonly EmailClientOptions _emailClientOptions;
    private readonly ILogger<ExtractUploadFailedEmailClient> _logger;

    public ExtractUploadFailedEmailClient(
        AmazonSimpleEmailServiceV2Client emailClient,
        EmailClientOptions emailClientOptions,
        ILogger<ExtractUploadFailedEmailClient> logger)
    {
        _emailClient = emailClient;
        _emailClientOptions = emailClientOptions;
        _logger = logger;
    }

    public async Task SendAsync(string extractDescription, Exception ex, CancellationToken cancellationToken)
    {
        var sb = ex switch
        {
            ValidationException => FormatValidationException(ex),
            CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException => FormatCanNotUploadException(ex),
            CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException => FormatCanNotUploadException(ex),
            CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException => FormatCanNotUploadException(ex),
            ExtractDownloadNotFoundException => FormatValidationException(ex),
            ExtractRequestMarkedInformativeException => FormatValidationException(ex),
            _ => new StringBuilder()
        };

        var emailRequest = CreateSendEmailRequest(extractDescription, sb);

        try
        {
            _logger.LogInformation("Received email request for destination {Destination} and subject {Subject}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data);
            await _emailClient.SendEmailAsync(emailRequest, cancellationToken);
            _logger.LogInformation("Sent email request for destination {Destination} and subject {Subject} with body {Body}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data, emailRequest.Content.Simple.Body.Text.Data);
        }
        catch (Exception) when (emailRequest is null)
        {
            _logger.LogError("Received email request, but client is not configured!");
        }
        catch (Exception myEx)
        {
            _logger.LogError(myEx, "An error occurred with {ClientName}: {ExceptionMessage}", nameof(ExtractUploadFailedEmailClient), myEx.Message);
        }

        StringBuilder FormatValidationException(Exception exception) => new StringBuilder()
            .AppendLine("De oplading kon niet verwerkt worden wegens een validatiefout.").AppendLine()
            .AppendLine(JsonConvert.SerializeObject(ex, Formatting.Indented)).AppendLine()
            .AppendLine(exception.Message).AppendLine();

        StringBuilder FormatTimeoutException(Exception exception) => new StringBuilder()
            .AppendLine("De oplading kon niet tijdig verwerkt worden. Je kan de status volgen via het WR-portaal.").AppendLine()
            .AppendLine(exception.Message).AppendLine();

        StringBuilder FormatCanNotUploadException(Exception exception) => new StringBuilder()
            .AppendLine("De status van de oplading kan niet geverifieerd worden.").AppendLine()
            .AppendLine(exception.Message).AppendLine();
    }

    private SendEmailRequest CreateSendEmailRequest(string extractDescription, StringBuilder sb)
    {
        return string.IsNullOrEmpty(_emailClientOptions?.ExtractUploadFailed)
        ? null
        : new SendEmailRequest
        {
            Destination = new Destination
            {
                ToAddresses = new List<string> { _emailClientOptions.ExtractUploadFailed }
            },
            FromEmailAddress = _emailClientOptions.FromEmailAddress,
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Data = extractDescription is not null
                        ? $"Oplading Wegenregister {extractDescription} is mislukt"
                        : $"Oplading Wegenregister is mislukt"
                    },
                    Body = new Body
                    {
                        Text = new Content { Data = sb.ToString() }
                    }
                }
            }
        };
    }
}

public class NotConfiguredExtractUploadFailedEmailClient : IExtractUploadFailedEmailClient
{
    private readonly ILogger<NotConfiguredExtractUploadFailedEmailClient> _logger;

    public NotConfiguredExtractUploadFailedEmailClient(ILogger<NotConfiguredExtractUploadFailedEmailClient> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string extractDescription, Exception ex, CancellationToken cancellationToken)
    {
        _logger.LogError("Received email request, but client is not configured!");

        return Task.CompletedTask;
    }
}
public interface IExtractUploadFailedEmailClient
{
    Task SendAsync(string extractDescription, Exception ex, CancellationToken cancellationToken);
}
