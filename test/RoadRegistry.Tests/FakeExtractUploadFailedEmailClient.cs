namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;

    public class FakeExtractUploadFailedEmailClient : IExtractUploadFailedEmailClient
    {
        public Task SendAsync(FailedExtractUpload extract, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
