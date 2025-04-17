namespace RoadRegistry.Product.PublishHost.HttpClients;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class MetaDataCenterHttpClient
{
    private readonly Uri _baseUrl;
    private readonly MetadataCenterOptions _options;
    private readonly ConcurrentDictionary<string, IEnumerable<string>> _defaultHeaders;
    private readonly ILogger<MetaDataCenterHttpClient>? _logger;
    private readonly ITokenProvider _tokenProvider;
    private IDictionary<string, IEnumerable<string>> DefaultHeaders => _defaultHeaders;

    public MetaDataCenterHttpClient(
        IOptions<MetadataCenterOptions> options,
        ITokenProvider tokenProvider,
        ILogger<MetaDataCenterHttpClient>? logger)
    {
        _logger = logger;
        _options = options.Value;
        _tokenProvider = tokenProvider;
        _baseUrl = new Uri(_options.BaseUrl);
        _defaultHeaders = new ConcurrentDictionary<string, IEnumerable<string>>();
    }

    private ByteArrayContent GenerateCswPublicationBody(string identifier, DateTime dateStamp)
    {
        var body = new ByteArrayContent(Encoding.UTF8.GetBytes($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<csw:Transaction service=""CSW"" version=""2.0.2""
	xmlns:csw=""http://www.opengis.net/cat/csw/2.0.2""
	xmlns:ogc=""http://www.opengis.net/ogc""
	xmlns:apiso=""http://www.opengis.net/cat/csw/apiso/1.0"">
	<csw:Update>
        <csw:RecordProperty>
            <csw:Name>gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:date/gmd:CI_Date/gmd:date/gco:Date[../../gmd:dateType/gmd:CI_DateTypeCode/@codeListValue=""publication""]</csw:Name>
            <csw:Value>{dateStamp:O}</csw:Value>
        </csw:RecordProperty>
        <csw:RecordProperty>
            <csw:Name>gmd:dateStamp/gco:Date</csw:Name>
            <csw:Value>{dateStamp:O}</csw:Value>
        </csw:RecordProperty>
		<csw:Constraint version=""1.1.0"">
			<ogc:Filter>
				<ogc:PropertyIsEqualTo>
					<ogc:PropertyName>Identifier</ogc:PropertyName>
					<ogc:Literal>{identifier}</ogc:Literal>
				</ogc:PropertyIsEqualTo>
			</ogc:Filter>
		</csw:Constraint>
	</csw:Update>
</csw:Transaction>
"));
        body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");
        return body;
    }

    private async Task<string?> GetXsrfToken(CancellationToken cancellationToken)
    {
        var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUrl, "/srv/eng/info?type=me"));
        foreach (var header in DefaultHeaders)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"Bearer {await _tokenProvider.GetAccessToken()}");

        string? xsrfToken = null;
        try
        {
            using var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });

            var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.OK &&
                response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
            {
                var cookieHeader = "XSRF-TOKEN=";
                xsrfToken = cookieValues.FirstOrDefault(i => i.Contains(cookieHeader))
                    ?.Split(";")
                    .First(i => i.Contains(cookieHeader))
                    .Replace(cookieHeader, string.Empty);
            }

            if (string.IsNullOrWhiteSpace(xsrfToken))
            {
                _logger?.LogCritical("Unable to retrieve XSRF-TOKEN");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Unable to retrieve XSRF-TOKEN");
        }

        return xsrfToken;
    }

    public async Task<XDocument?> UpdateCswPublication(
        DateTime dateStamp,
        CancellationToken cancellationToken)
    {
        var xsrfToken = await GetXsrfToken(cancellationToken);
        if (string.IsNullOrWhiteSpace(xsrfToken))
        {
            return null;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_baseUrl, "/srv/dut/csw-publication"));
        foreach (var header in DefaultHeaders)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"Bearer {await _tokenProvider.GetAccessToken()}");
        requestMessage.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrfToken);

        requestMessage.Content = GenerateCswPublicationBody(_options.FullIdentifier, dateStamp);

        using var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });

        HttpResponseMessage response;
        var retry = 0;
        while (true)
        {
            response = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger?.LogCritical("Unable to update CswPublication");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                retry++;
                if (retry == 10)
                {
                    throw new InvalidOperationException($"Unable to update CswPublication, response: {(int)response.StatusCode}");
                }

                continue;
            }

            break;
        }

        var xmlResponseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        var xmlResponse = await XDocument.LoadAsync(xmlResponseContent, LoadOptions.None, cancellationToken);
        return xmlResponse;
    }

    public async Task<string> GetXmlAsString(CancellationToken cancellationToken)
    {
        var requestMessage =
            new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(_baseUrl, $"/srv/api/records/{_options.FullIdentifier}/formatters/xml"));
        foreach (var header in DefaultHeaders)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"Bearer {await _tokenProvider.GetAccessToken()}");

        using var httpClient = new HttpClient(new HttpClientHandler() { UseCookies = false });
        var response = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger?.LogCritical("Unable to get XML");
            throw new InvalidOperationException($"Unable to get XML, response: {(int)response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<byte[]> GetPdfAsByteArray(CancellationToken cancellationToken)
    {
        var requestMessage =
            new HttpRequestMessage(
                HttpMethod.Get,
                new Uri(_baseUrl, $"/srv/api/records/{_options.FullIdentifier}/formatters/xsl-view?output=pdf&language=dut&attachment=true"));
        foreach (var header in DefaultHeaders)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        requestMessage.Headers.TryAddWithoutValidation("Authorization", $"Bearer {await _tokenProvider.GetAccessToken()}");

        using var httpClient = new HttpClient(new HttpClientHandler { UseCookies = false });
        var response = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger?.LogCritical("Unable to get pdf");
            throw new InvalidOperationException($"Unable to get pdf, response: {(int)response.StatusCode}");
        }

        var pdf = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return pdf;
    }
}
