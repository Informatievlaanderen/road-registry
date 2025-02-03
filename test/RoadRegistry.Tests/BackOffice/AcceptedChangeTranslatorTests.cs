namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using RoadRegistry.BackOffice.Messages;
using Scenarios;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;

public class AcceptedChangeTranslatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AcceptedChangeTranslatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EnsureAllAcceptedChangesHaveADutchTranslation()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;
        var templateChange = fixture.Create<AcceptedChange>();

        var properties = templateChange.GetType().GetProperties().Where(x => x.Name != nameof(AcceptedChange.Problems)).ToArray();
        Assert.NotEmpty(properties);

        var invalidChanges = new List<string>();

        foreach (var pi in properties)
        {
            var change = new AcceptedChange();
            pi.SetValue(change, pi.GetValue(templateChange));

            var changeName = change.Flatten().GetType().Name;

            try
            {
                var problemTranslation = RoadRegistry.BackOffice.DutchTranslations.AcceptedChange.Translator(change);
                if (problemTranslation.Contains("has no translation"))
                {
                    invalidChanges.Add(changeName);
                }
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Failed trying to translate accepted change {changeName}: {ex.Message}");
                invalidChanges.Add(changeName);
            }
        }

        Assert.Empty(invalidChanges);
    }
}
