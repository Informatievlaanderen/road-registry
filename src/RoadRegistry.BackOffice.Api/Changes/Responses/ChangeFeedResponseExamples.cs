namespace RoadRegistry.Api.Changes.Responses
{
    using System;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using Swashbuckle.AspNetCore.Filters;

    public class ChangeFeedResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new ChangeFeedResponse
            {
                Entries = new []
                {
                    new ChangeFeedEntry
                    {
                        Id = 1,
                        Title = "De oplading werd ontvangen.",
                        Type = nameof(RoadNetworkChangesArchiveUploaded),
                        Content = new RoadNetworkChangesArchiveUploadedEntry
                        {
                            ArchiveId = Guid.NewGuid().ToString("N")
                        }
                    },
                    new ChangeFeedEntry
                    {
                        Id = 2,
                        Title = "De oplading werd aanvaard.",
                        Type = nameof(RoadNetworkChangesArchiveAccepted),
                        Content = new RoadNetworkChangesArchiveAcceptedEntry
                        {
                            ArchiveId = Guid.NewGuid().ToString("N"),
                            Files = new RoadNetworkChangesArchiveFile[0]
                        }
                    }
                }
            };
        }
    }
}
