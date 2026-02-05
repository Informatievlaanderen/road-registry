namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using Actions.DataValidation;
using Actions.UploadInwinningExtract;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using FluentAssertions;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.AggregateTests;

//TODO-pr move to DataValidationSqsLambdaRequest tests
public class DataValidationApiClientTests
{
    [Fact]
    public async Task ThenSerialized()
    {
        // Arrange
        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var sqsOptions = new SqsOptions(jsonSerializerSettings);
        var sqsSerializer = new SqsJsonMessageSerializer(sqsOptions, SqsJsonMessageAssemblies.Assemblies);

        var client = new DataValidationApiClient(extractsDbContext, sqsSerializer);

        var fixture = new RoadNetworkTestDataV2().Fixture;
        var sqsRequest = fixture.Create<MigrateRoadNetworkSqsRequest>();

        // Act
        await client.SendToValidate(sqsRequest, CancellationToken.None);

        // Assert
        var queueItem = await extractsDbContext.DataValidationQueue.FindAsync(sqsRequest.DownloadId.ToGuid());
        queueItem.Should().NotBeNull();
        var deserialized = sqsSerializer.Deserialize(queueItem!.SqsRequestJson);
        deserialized.Should().BeOfType<MigrateRoadNetworkSqsRequest>();
    }

    [Fact]
    public async Task MultipleRunsShouldQueueOnlyOnce()
    {
        // Arrange
        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
        var sqsOptions = new SqsOptions(jsonSerializerSettings);
        var sqsSerializer = new SqsJsonMessageSerializer(sqsOptions, SqsJsonMessageAssemblies.Assemblies);

        var client = new DataValidationApiClient(extractsDbContext, sqsSerializer);

        var fixture = new RoadNetworkTestDataV2().Fixture;
        var sqsRequest = fixture.Create<MigrateRoadNetworkSqsRequest>();

        // Act
        await client.SendToValidate(sqsRequest, CancellationToken.None);
        await client.SendToValidate(sqsRequest, CancellationToken.None);

        // Assert
        var queueItem = await extractsDbContext.DataValidationQueue.FindAsync(sqsRequest.DownloadId.ToGuid());
        queueItem.Should().NotBeNull();

        var deserialized = sqsSerializer.Deserialize(queueItem!.SqsRequestJson);
        deserialized.Should().BeOfType<MigrateRoadNetworkSqsRequest>();
    }
}
