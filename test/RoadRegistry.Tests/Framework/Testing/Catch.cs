namespace RoadRegistry.Tests.Framework.Testing;

public static class Catch
{
    public static async Task<Exception> Exception(Func<Task> action)
    {
        Exception caught = null;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            caught = exception;
        }

        return caught;
    }
}