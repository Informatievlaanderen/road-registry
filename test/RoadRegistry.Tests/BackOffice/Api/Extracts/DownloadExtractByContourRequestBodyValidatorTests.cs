namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentValidation;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.IO;
    using Xunit;

    public class DownloadExtractByContourRequestBodyValidatorTests
    {
        private readonly Fixture _fixture;
        private readonly DownloadExtractByContourRequestBodyValidator _validator;

        public DownloadExtractByContourRequestBodyValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new DownloadExtractByContourRequestBodyValidator
                (new WKTReader(), _fixture.Create<ILogger<DownloadExtractByContourRequestBodyValidator>>());
        }

        public async Task x()
        {
        }
    }
}
