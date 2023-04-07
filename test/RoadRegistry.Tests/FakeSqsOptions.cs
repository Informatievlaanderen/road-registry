namespace RoadRegistry.Tests;

using Amazon;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using RoadRegistry.BackOffice;

public class FakeSqsOptions : SqsOptions
{
    public FakeSqsOptions()
        : base(RegionEndpoint.EUWest1, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())
    {
    }
}
