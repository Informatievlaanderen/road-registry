namespace RoadRegistry.BackOffice.Api.IntegrationTests.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class AcmIdmTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        private sealed record Endpoint(HttpMethod Method, string Uri, params string[] RequiredScopes);
        private static readonly Endpoint[] Endpoints =
        {
            new(HttpMethod.Get, "v1/changefeed/entry/{id}/content"),
            new(HttpMethod.Get, "v1/changefeed/head"),
            new(HttpMethod.Get, "v1/changefeed/next"),
            new(HttpMethod.Get, "v1/changefeed/previous"),

            new(HttpMethod.Get, "v1/download/for-editor"),
            new(HttpMethod.Get, "v1/download/for-product/{date}"),

            new(HttpMethod.Get, "v1/extracts/{downloadId}", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Put, "v1/extracts/{downloadId}/close", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/downloadrequests", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/downloadrequests/bycontour", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/downloadrequests/byfile", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/downloadrequests/byniscode", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracts/download/{downloadId}", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/download/{downloadId}/uploads/fc", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/extracts/download/{downloadId}/jobs", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracts/upload/{uploadId}/status", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/extracts/overlappingtransactionzones.geojson"),
            new(HttpMethod.Get, "v1/extracts/transactionzones.geojson"),

            new(HttpMethod.Get, "v1/information"),
            new(HttpMethod.Post, "v1/information/validate-wkt", Scopes.DvWrIngemetenWegBeheer),

            new(HttpMethod.Get, "v1/security/user", Scopes.VoInfo),
            new(HttpMethod.Get, "v1/security/info"),
            new(HttpMethod.Get, "v1/security/exchange"),

            new(HttpMethod.Post, "v1/system/correct/roadnodeversions", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/correct/roadsegmentorganizationnames", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/correct/roadsegmentstatus/dutch-translations", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/correct/roadsegmentversions", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/migrate/outlinedroadsegmentsoutofroadnetwork", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Post, "v1/system/snapshots/refresh", Scopes.DvWrUitzonderingenBeheer),

            new(HttpMethod.Get, "v1/organizations"),
            new(HttpMethod.Post, "v1/organizations", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Patch, "v1/organizations/{id}", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Delete, "v1/organizations/{id}", Scopes.DvWrUitzonderingenBeheer),
            new(HttpMethod.Patch, "v1/organizations/{id}/rename", Scopes.DvWrUitzonderingenBeheer),

            new(HttpMethod.Post, "v1/wegsegmenten/acties/wijzigen/attributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/acties/wijzigen/dynamischeattributen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/acties/schetsen", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Get, "v1/wegsegmenten/{id}"),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/straatnaamkoppelen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/straatnaamontkoppelen", Scopes.DvWrAttribuutWaardenBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/verwijderen/schets", Scopes.DvWrGeschetsteWegBeheer),
            new(HttpMethod.Post, "v1/wegsegmenten/{id}/acties/wijzigen/schetsgeometrie", Scopes.DvWrGeschetsteWegBeheer),

            new(HttpMethod.Post, "v1/upload/fc", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/upload/{identifier}", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Post, "v1/upload/jobs", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/upload/jobs", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/upload/jobs/active", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Get, "v1/upload/jobs/1e052f20-ec8b-4243-aa6e-c99a3bb7439c", Scopes.DvWrIngemetenWegBeheer),
            new(HttpMethod.Delete, "v1/upload/jobs/1e052f20-ec8b-4243-aa6e-c99a3bb7439c", Scopes.DvWrIngemetenWegBeheer)
        };
        public static IEnumerable<object[]> EndpointsMemberData() => Endpoints.Select(x => new object[] { x.Method, x.Uri, x.RequiredScopes });

        public AcmIdmTests(IntegrationTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void EnsureAllEndpointsAreTested()
        {
            var apiExplorer = _fixture.TestServer.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            var apiDescriptions = apiExplorer.ApiDescriptionGroups.Items.SelectMany(apiDescriptionGroup => apiDescriptionGroup.Items).ToList();
            Assert.NotEmpty(apiDescriptions);

            foreach (var apiDescription in apiDescriptions)
            {
                var testEndpoint = Endpoints.SingleOrDefault(x => x.Method.ToString().Equals(apiDescription.HttpMethod, StringComparison.InvariantCultureIgnoreCase)
                                                                  && x.Uri.Equals(apiDescription.RelativePath, StringComparison.InvariantCultureIgnoreCase));
                if (testEndpoint is null)
                {
                    Assert.Fail($"Endpoint '{apiDescription.HttpMethod} {apiDescription.RelativePath}' has no matching test");
                }
            }
        }

        [Theory]
        [MemberData(nameof(EndpointsMemberData))]
        public Task ReturnsSuccess(HttpMethod method, string endpoint, string[] requiredScopes)
        {
            return RetryMultipleTimesUntilNoXunitException(async () =>
            {
                var client = _fixture.TestServer.CreateClient();
                if (requiredScopes.Any())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAcmIdmAccessToken(string.Join(" ", requiredScopes)));
                }

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint));

                Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
            });
        }

        [Theory]
        [MemberData(nameof(EndpointsMemberData))]
        public Task ReturnsUnauthorized(HttpMethod method, string endpoint, string[] requiredScopes)
        {
            return RetryMultipleTimesUntilNoXunitException(async () =>
            {
                var client = _fixture.TestServer.CreateClient();

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint));

                if (requiredScopes.Any())
                {
                    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                }
                else
                {
                    Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                }
            });
        }

        [Theory]
        [MemberData(nameof(EndpointsMemberData))]
        public Task ReturnsForbidden(HttpMethod method, string endpoint, string[] requiredScopes)
        {
            return RetryMultipleTimesUntilNoXunitException(async () =>
            {
                var client = _fixture.TestServer.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAcmIdmAccessToken());

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint));

                if (requiredScopes.Any(x => x != Scopes.VoInfo))
                {
                    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
                }
                else
                {
                    Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
                }
            });
        }

        private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, new Uri(endpoint, UriKind.RelativeOrAbsolute));
            switch (method.ToString())
            {
                case nameof(HttpMethod.Post):
                case nameof(HttpMethod.Put):
                    request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                    break;
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
        {
            var response = await client.SendAsync(request, CancellationToken.None);

            Assert.NotNull(response);
            _testOutputHelper.WriteLine($"Response status code: {response.StatusCode}");
            Assert.NotEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);

            return response;
        }

        private static async Task RetryMultipleTimesUntilNoXunitException(Func<Task> action, int retryTimes = 5)
        {
            for (var i = 0; i < retryTimes; i++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (XunitException)
                {
                    if (i == retryTimes - 1)
                    {
                        throw;
                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
