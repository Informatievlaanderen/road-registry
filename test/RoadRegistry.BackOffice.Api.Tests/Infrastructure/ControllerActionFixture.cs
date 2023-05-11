namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc;

public abstract class ControllerActionFixture<TRequest> : ApplicationFixture, IAsyncLifetime
{
    public TRequest Request { get; private set; }
    public IActionResult Result { get; private set; }
    public Exception Exception { get; private set; }
    public bool HasException => Exception is not null;

    public async Task InitializeAsync()
    {
        await SetupAsync();

        Request = CreateRequest();
        if (Request is null)
        {
            return;
        }

        await ExecuteAsync(Request);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual TRequest CreateRequest()
    {
        return default;
    }

    public async Task ExecuteAsync(TRequest request)
    {
        try
        {
            Result = await GetResultAsync(request);
            Exception = null;
        }
        catch (Exception ex)
        {
            Result = null;
            Exception = ex;
        }
    }

    protected abstract Task<IActionResult> GetResultAsync(TRequest request);

    protected virtual Task SetupAsync()
    {
        return Task.CompletedTask;
    }
}
