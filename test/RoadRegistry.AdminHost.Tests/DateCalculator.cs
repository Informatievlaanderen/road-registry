namespace RoadRegistry.AdminHost.Tests;

using System;
using System.Globalization;
using Xunit.Abstractions;

public class DateCalculator
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DateCalculator(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact(Skip = "Used for local quick output calculations")]
    public void CalculateSimpleDate()
    {
        var dt = new DateTime(2023, 6, 28).AddDays(21);
        _testOutputHelper.WriteLine(dt.ToString(CultureInfo.InvariantCulture));

        Assert.True(true);
    }
}
