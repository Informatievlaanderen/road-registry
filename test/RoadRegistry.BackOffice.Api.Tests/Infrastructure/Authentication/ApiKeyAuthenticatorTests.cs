namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Authentication
{
    using System.Linq;
    using System.Security.Claims;
    using Api.Infrastructure.Controllers.Attributes;
    using AutoFixture;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;

    public class ApiKeyAuthenticatorTests
    {
        private readonly Fixture _fixture;

        public ApiKeyAuthenticatorTests()
        {
            _fixture = FixtureFactory.Create();
        }

        [Theory]
        [InlineData("10b7730e-8ce9-4e67-a72d-24528650f6a1", "10b7730e***8650f6a1")]
        [InlineData("1234567890", "12***90")]
        [InlineData("10b7730e-8ce9-4e67-a72d-24528650f6a1-10b7730e-8ce9-4e67-a72d-24528650f6a1", "10b7730e***8650f6a1")]
        [InlineData("10b7730e-8ce9-4e67-a72d-24528650f6a1-10b7730e-8ce9-4e67-a72d-24528650f6a12", "10b7730e***650f6a12")]
        [InlineData("10b7730e-8ce9-4e67-a72d-24528650f6a1-10b7730e-8ce9-4e67-a72d-24528650f6a123", "10b7730e***50f6a123")]
        public async Task ThenOperator(string apiKey, string expectedOperator)
        {
            var tokenReader = new Mock<IApiTokenReader>();
            tokenReader
                .Setup(x => x.ReadAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApiToken(apiKey, _fixture.Create<string>(), new ApiTokenMetadata(true)));

            var authenticator = new ApiKeyAuthenticator(tokenReader.Object, new ConfigurationBuilder().Build());

            var identity = (ClaimsIdentity) await authenticator.AuthenticateAsync(apiKey, CancellationToken.None);

            identity.Claims.Single(x => x.Type == "operator").Value.Should().Be(expectedOperator);
        }
    }
}
