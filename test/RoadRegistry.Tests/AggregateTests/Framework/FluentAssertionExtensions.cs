namespace RoadRegistry.Tests.AggregateTests.Framework;

using FluentAssertions.Collections;
using RoadRegistry.BackOffice.Core;
using Xunit.Sdk;

public static class FluentAssertionExtensions
{
    public static void HaveNoError(this GenericCollectionAssertions<Problem> assertions)
    {
        if (assertions.Subject.OfType<Error>().Any())
        {
            throw new XunitException($"Expected problems.HasError() to be False, but found True.{Environment.NewLine}{string.Join(Environment.NewLine, assertions.Subject.Select(x => x.Describe()))}");
        }
    }
}
