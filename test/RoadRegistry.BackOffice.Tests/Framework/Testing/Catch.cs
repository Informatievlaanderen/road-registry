namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System;
    using System.Threading.Tasks;

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
}
