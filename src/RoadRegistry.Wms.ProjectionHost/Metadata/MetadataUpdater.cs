using System;

namespace RoadRegistry.Wms.ProjectionHost.Metadata
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Unicode;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using NodaTime;

    public class MetadataUpdater : IMetadataUpdater
    {
        private readonly MetadataConfiguration _configuration;
        private readonly IClock _clock;

        public MetadataUpdater(MetadataConfiguration configuration, IClock clock)
        {
            _configuration = configuration;
            _clock = clock;
        }

        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var makeBody = MakeBody(_clock.GetCurrentInstant(), _configuration.Id);
                var xDoc = await PerformCswRequest(_configuration.Uri, makeBody, cancellationToken);

                var ns = new System.Xml.XmlNamespaceManager(new System.Xml.NameTable());
                ns.AddNamespace("csw", "http://www.opengis.net/cat/csw/2.0.2");
                var totalUpdated = int.Parse(xDoc.XPathSelectElement("csw:TransactionResponse/csw:TransactionSummary/csw:totalUpdated", ns)?.Value ?? string.Empty);
                if (totalUpdated <= 0)
                    throw new Exception($"Metadata not updated, response from metadata service: \n{xDoc}");
            }
            catch (Exception ex)
            {
                throw new Exception("Could not update metadata: " + ex.Message);
            }
        }

        private static string MakeBody(Instant currentInstant, string id)
        {
            var shortDate = currentInstant.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var description = $"Toestand {shortDate}";

            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                   "<csw:Transaction service=\"CSW\" version=\"2.0.2\" xmlns:csw=\"http://www.opengis.net/cat/csw/2.0.2\"" +
                   " xmlns:ogc=\"http://www.opengis.net/ogc\"" +
                   " xmlns:apiso=\"http://www.opengis.net/cat/csw/apiso/1.0\">" + "<csw:Update>" +
                   "<csw:RecordProperty>" +
                   "<csw:Name>gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:date/gmd:CI_Date[gmd:dateType/gmd:CI_DateTypeCode/@codeListValue=\"publication\"]/gmd:date/gco:Date</csw:Name>" +
                   $"<csw:Value>{shortDate}</csw:Value>" + "</csw:RecordProperty>" + "<csw:RecordProperty>" +
                   "<csw:Name>gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:date/gmd:CI_Date[gmd:dateType/gmd:CI_DateTypeCode/@codeListValue=\"revision\"]/gmd:date/gco:Date</csw:Name>" +
                   $"<csw:Value>{shortDate}</csw:Value>" + "</csw:RecordProperty>" + "<csw:RecordProperty>" +
                   "<csw:Name>gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:edition/gco:CharacterString</csw:Name>" +
                   $"<csw:Value>{description}</csw:Value>" + "</csw:RecordProperty>" + "<csw:RecordProperty>" +
                   "<csw:Name>gmd:identificationInfo/gmd:MD_DataIdentification/gmd:extent/gmd:EX_Extent/gmd:temporalElement/gmd:EX_TemporalExtent/gmd:extent/gml:TimePeriod/gml:endPosition</csw:Name>" +
                   $"<csw:Value>{shortDate}</csw:Value>" + "</csw:RecordProperty>" + "<csw:RecordProperty>" +
                   "<csw:Name>gmd:dateStamp/gco:DateTime</csw:Name>" +
                   $"<csw:Value>{currentInstant:yyyy-MM-ddThh:mm:ss}</csw:Value>" +
                   "</csw:RecordProperty>" + "<csw:Constraint version=\"1.1.0\">" + "<ogc:Filter>" +
                   "<ogc:PropertyIsEqualTo>" + "<ogc:PropertyName>Identifier</ogc:PropertyName>" +
                   $"<ogc:Literal>{id}</ogc:Literal>" + "</ogc:PropertyIsEqualTo>" + "</ogc:Filter>" +
                   "</csw:Constraint>" + "</csw:Update>" + "</csw:Transaction>";
        }

        private async Task<XDocument> PerformCswRequest(string uri, string bodyXml, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", EncodeBasicAuth(_configuration.Username, _configuration.Password));

            // We need to send a POST request to receive an xsrf token, see https://geonetwork-opensource.org/manuals/trunk/en/customizing-application/misc.html
            var xsrfPostMessage = await client.PostAsync(_configuration.LoginUri, new StringContent(string.Empty), cancellationToken);

            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", ExtractXsrfToken(xsrfPostMessage));

            var updateMetadataResponse = await client.PostAsync(uri,
                new ByteArrayContent(Encoding.UTF8.GetBytes(bodyXml))
                {
                    Headers =
                    {
                        ContentType = MediaTypeHeaderValue.Parse("application/xml")
                    }
                }, cancellationToken);
            var content = await updateMetadataResponse.Content.ReadAsStreamAsync(cancellationToken);
            return await XDocument.LoadAsync(content, LoadOptions.None, cancellationToken);
        }

        private static string EncodeBasicAuth(string username, string password)
        {
            return $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}";
        }

        private static string ExtractXsrfToken(HttpResponseMessage xsrfPostMessage)
        {
            xsrfPostMessage.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders);

            if (setCookieHeaders == null)
                throw new Exception("Could not find any Set-Cookie header to update metadata");

            var token = setCookieHeaders.Select(cookie => cookie.Split(';'))
                .FirstOrDefault(split => split[0].Contains("XSRF-TOKEN="));

            if(token == null)
                throw new Exception("Could not find any xsrf token in the Set-Cookie headers");

            return token[0].Split('=')[1];
        }
    }
}
