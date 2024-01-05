namespace RoadRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;

    public class StreetNameApiClient: IStreetNameClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreetNameRegistryOptions _options;
        private readonly JsonSerializerSettings _jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public StreetNameApiClient(IHttpClientFactory httpClientFactory, StreetNameRegistryOptions options)
        {
            _httpClientFactory = httpClientFactory.ThrowIfNull();
            _options = options.ThrowIfNull();
        }

        public async Task<StreetNameItem> GetAsync(int id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_options.StreetNameRegistryBaseUrl))
            {
                throw new ConfigurationErrorsException($"{nameof(_options.StreetNameRegistryBaseUrl)} is not configured");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            }
            var response = await httpClient.GetAsync(CreateUri(id), cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Gone)
            {
                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new StreetNameRegistryUnexpectedStatusCodeException(response.StatusCode);
            }

            var streetNameJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var streetName = JsonConvert.DeserializeObject<StreetNameSnapshotOsloRecord>(streetNameJson, _jsonSerializerSettings);

            return new StreetNameItem
            {
                Id = int.Parse(streetName.Identificator.ObjectId),
                Name = GetSpelling(streetName.Straatnamen, Taal.NL),
                Status = streetName.StraatnaamStatus
            };
        }

        private Uri CreateUri(int id)
        {
            return new Uri(new Uri(_options.StreetNameRegistryBaseUrl), $"/v2/straatnamen/{id}");
        }

        private static string GetSpelling(List<DeseriazableGeografischeNaam> namen, Taal taal)
        {
            return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
        }
    }
}
