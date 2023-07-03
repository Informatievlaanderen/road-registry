namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;

    public class FakeExtractUploadFailedEmailClient : IExtractUploadFailedEmailClient
    {
        public Task SendAsync(string extractDescription, Exception ex, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
