namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;

internal class FakeRetryPolicy : ICustomRetryPolicy
{
    public Task Retry(Func<Task> functionToRetry)
    {
        return functionToRetry();
    }
}
