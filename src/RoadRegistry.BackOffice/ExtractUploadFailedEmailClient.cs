namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Configuration;
using Extracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore.Infrastructure;

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
            CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException => FormatValidationException(ex),
            CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException => FormatValidationException(ex),
            _ => new StringBuilder()
        };

        var emailRequest = CreateSendEmailRequest(extractDescription, sb);

        if (emailRequest is not null)
        {
            _logger.LogInformation("Received email request for destination {Destination} and subject {Subject}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data);
            await _emailClient.SendEmailAsync(emailRequest, cancellationToken);
            _logger.LogInformation("Sent email request for destination {Destination} and subject {Subject} with body {Body}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data, emailRequest.Content.Simple.Body.Text.Data);
        }
        else
        {
            _logger.LogError("Received email request for destination {Destination} and subject {Subject}, but client is not configured!", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data);
        }

        StringBuilder FormatValidationException(Exception ex)
        {
            return new StringBuilder()
                .AppendLine("De oplading kon niet verwerkt worden wegens een validatiefout.").AppendLine()
                .AppendLine(JsonConvert.SerializeObject(ex, Formatting.Indented)).AppendLine();
        }

        StringBuilder FormatCanNotUploadException(Exception ex)
        {
            return new StringBuilder()
                .AppendLine("De status van de oplading kan niet geverifieerd worden.").AppendLine();
        }
    }

    private SendEmailRequest CreateSendEmailRequest(string extractDescription, StringBuilder sb)
    {
        return _emailClientOptions.ExtractUploadFailed is null
        ? null
        : new SendEmailRequest
        {
            Destination = new Destination
            {
                ToAddresses = new List<string> { _emailClientOptions.ExtractUploadFailed }
            },
            FromEmailAddress = "noreply-wegenregister@vlaanderen.be",
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

public interface IExtractUploadFailedEmailClient
{
    Task SendAsync(string extractDescription, Exception ex, CancellationToken cancellationToken);
}
