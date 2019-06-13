namespace RoadRegistry.Api.Activities.Responses
{
    using System;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using Swashbuckle.AspNetCore.Filters;

    public class ActivityResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new ActivityResponse
            {
                Activities = new []
                {
                    new ActivityResponseItem
                    {
                        Id = 1,
                        Title = "De oplading werd ontvangen.",
                        Type = nameof(RoadNetworkChangesArchiveUploaded),
                        Content = new RoadNetworkChangesArchiveUploadedActivity
                        {
                            ArchiveId = Guid.NewGuid().ToString("N")
                        }
                    },
                    new ActivityResponseItem
                    {
                        Id = 2,
                        Title = "De oplading werd aanvaard.",
                        Type = nameof(RoadNetworkChangesArchiveAccepted),
                        Content = new RoadNetworkChangesArchiveAcceptedActivity
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
