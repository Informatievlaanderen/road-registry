namespace RoadRegistry.BackOffice;

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

internal class ExtractUploadFailedEmailClient : IExtractUploadFailedEmailClient
{
    private readonly AmazonSimpleEmailServiceV2Client _emailClient;
    private readonly ILogger<ExtractUploadFailedEmailClient> _logger;

    public ExtractUploadFailedEmailClient(
        AmazonSimpleEmailServiceV2Client emailClient,
        ILogger<ExtractUploadFailedEmailClient> logger)
    {
        _emailClient = emailClient;
        _logger = logger;
    }

    public async Task SendAsync(string subject, ValidationException ex, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received email request for destination {Destination} and subject {Subject}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data);

        var emailRequest = CreateSendEmailRequest(subject, JsonConvert.SerializeObject(ex, Formatting.Indented));
        await _emailClient.SendEmailAsync(emailRequest, cancellationToken);

        _logger.LogInformation("Sent email request for destination {Destination} and subject {Subject} with body {Body}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data, emailRequest.Content.Simple.Body.Text.Data);
    }

    [Pure]
    private static SendEmailRequest CreateSendEmailRequest(string subject, string body)
    {
        return new SendEmailRequest
        {
            Destination = new Destination
            {
                ToAddresses = new List<string> { "opladingen.wegenregister@vlaanderen.be" }
            },
            FromEmailAddress = "noreply-wegenregister@vlaanderen.be",
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = subject },
                    Body = new Body
                    {
                        Text = new Content { Data = body }
                    }
                }
            }
        };
    }
}

public interface IExtractUploadFailedEmailClient
{
    Task SendAsync(string subject, ValidationException ex, CancellationToken cancellationToken);
}
