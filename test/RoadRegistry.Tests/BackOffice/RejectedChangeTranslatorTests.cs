namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using RoadRegistry.BackOffice.Messages;
using Scenarios;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;

public class RejectedChangeTranslatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RejectedChangeTranslatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EnsureAllRejectedChangesHaveADutchTranslation()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;
        var templateChange = fixture.Create<RejectedChange>();

        var properties = templateChange.GetType().GetProperties().Where(x => x.Name != nameof(RejectedChange.Problems)).ToArray();
        Assert.NotEmpty(properties);

        var invalidChanges = new List<string>();

        foreach (var pi in properties)
        {
            var change = new RejectedChange();
            pi.SetValue(change, pi.GetValue(templateChange));

            var changeName = change.Flatten().GetType().Name;

            try
            {
                var problemTranslation = RoadRegistry.BackOffice.DutchTranslations.RejectedChange.Translator(change);
                if (problemTranslation.Contains("has no translation"))
                {
                    invalidChanges.Add(changeName);
                }
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Failed trying to translate rejected change {changeName}: {ex.Message}");
                invalidChanges.Add(changeName);
            }
        }

        Assert.Empty(invalidChanges);
    }
}
