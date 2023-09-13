namespace RoadRegistry.StreetName
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;

    public class StreetNameApiClient: IStreetNameClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreetNameRegistryOptions _options;

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
            var uri = CreateUri(id);

            var response = await httpClient.GetAsync(uri, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Gone)
            {
                return null;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new StreetNameRegistryException(response.StatusCode);
            }

            var streetName = await response.Content.ReadFromJsonAsync<StreetNameSnapshotOsloRecord>(cancellationToken: cancellationToken);

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

        private static string? GetSpelling(List<DeseriazableGeografischeNaam>? namen, Taal taal)
        {
            return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
        }
    }
}
