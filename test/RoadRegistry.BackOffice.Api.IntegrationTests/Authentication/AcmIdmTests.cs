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
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class AcmIdmTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public static IEnumerable<object[]> EndpointsMemberData() => AcmIdmEndpoints.All.Select(x => new object[] { x.Method, x.Uri, x.RequiredScopes });

        // The road segment edit endpoints take the 'geschetste weg' scope as well as the 'ingemeten weg' scope, and
        // either one on its own is enough to get in: which road segments the caller may then actually touch is decided
        // by the domain, per segment, not by the authorization policy.
        public static IEnumerable<object[]> BothRoadScopeEndpointsMemberData() =>
            from endpoint in AcmIdmEndpoints.All
            where endpoint.RequiredScopes.Contains(Scopes.DvWrGeschetsteWegBeheer)
                  && endpoint.RequiredScopes.Contains(Scopes.DvWrIngemetenWegBeheer)
            from scope in new[] { Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer }
            select new object[] { endpoint.Method, endpoint.Uri, scope };

        public AcmIdmTests(IntegrationTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
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

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint.Replace("{jobId}", $"{Guid.NewGuid():D}")));

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

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint.Replace("{jobId}", $"{Guid.NewGuid():D}")));

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
        [MemberData(nameof(BothRoadScopeEndpointsMemberData))]
        public Task ReturnsSuccessWithEitherRoadScopeOnItsOwn(HttpMethod method, string endpoint, string scope)
        {
            return RetryMultipleTimesUntilNoXunitException(async () =>
            {
                var client = _fixture.TestServer.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _fixture.GetAcmIdmAccessToken(scope));

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint.Replace("{jobId}", $"{Guid.NewGuid():D}")));

                Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
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

                var response = await SendAsync(client, CreateRequestMessage(method, endpoint.Replace("{jobId}", $"{Guid.NewGuid():D}")));

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
