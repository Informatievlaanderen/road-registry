namespace RoadRegistry.Tests.Framework;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;

public class FakeRetryPolicy : ICustomRetryPolicy
{
    public Task Retry(Func<Task> functionToRetry)
    {
        return functionToRetry();
    }
}
