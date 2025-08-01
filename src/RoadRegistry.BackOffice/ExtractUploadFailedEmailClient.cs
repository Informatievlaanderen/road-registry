namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Configuration;
using Microsoft.Extensions.Logging;

public interface IExtractUploadFailedEmailClient
{
    Task SendAsync(FailedExtractUpload extract, CancellationToken cancellationToken);
}

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

    public async Task SendAsync(FailedExtractUpload extract, CancellationToken cancellationToken)
    {
        var emailRequest = CreateSendEmailRequest(extract);

        try
        {
            _logger.LogInformation("Received email request for destination {Destination} and subject {Subject}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data);
            await _emailClient.SendEmailAsync(emailRequest, cancellationToken);
            _logger.LogInformation("Sent email request for destination {Destination} and subject {Subject} with body: {Body}", string.Join(", ", emailRequest.Destination.ToAddresses), emailRequest.Content.Simple.Subject.Data, emailRequest.Content.Simple.Body.Html.Data);
        }
        catch (Exception) when (emailRequest is null)
        {
            _logger.LogError("Received email request, but client is not configured!");
        }
        catch (Exception myEx)
        {
            _logger.LogError(myEx, "An error occurred with {ClientName}: {ExceptionMessage}", nameof(ExtractUploadFailedEmailClient), myEx.Message);
        }
    }

    private string BuildExtractDetailsUrl(FailedExtractUpload extract)
    {
        return !string.IsNullOrEmpty(_emailClientOptions?.ExtractDetailsPortaalUrl)
            ? _emailClientOptions.ExtractDetailsPortaalUrl.Replace("{downloadId}", extract.DownloadId)
            : string.Empty;
    }

    private SendEmailRequest CreateSendEmailRequest(FailedExtractUpload extract)
    {
        var portaalUrl = BuildExtractDetailsUrl(extract);

        return string.IsNullOrEmpty(_emailClientOptions?.ExtractUploadFailed)
        ? null
        : new SendEmailRequest
        {
            Destination = new Destination
            {
                ToAddresses = [_emailClientOptions.ExtractUploadFailed]
            },
            FromEmailAddress = _emailClientOptions.FromEmailAddress,
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Data = extract.Description is not null
                        ? $"Oplading Wegenregister {extract.Description} is mislukt"
                        : "Oplading Wegenregister is mislukt"
                    },
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Data = $"""
                                    <html>
                                    <body>
                                    <p>De oplading kon niet verwerkt worden wegens een validatiefout.</p>
                                    <a href="{portaalUrl}">{portaalUrl}</a>
                                    </body>
                                    </html>
                                    """
                        }
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

    public Task SendAsync(FailedExtractUpload extract, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received email request, but client is not configured so not doing anything");

        return Task.CompletedTask;
    }
}

public sealed record FailedExtractUpload(DownloadId DownloadId, string Description);
