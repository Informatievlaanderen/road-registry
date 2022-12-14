namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Extensions;

using BackOffice.Messages;
using Projections;
using Schema;

public static class MessagesExtensions
{
    public static Origin ToOrigin(this RoadNetworkChangesAccepted message)
    {
        return new Origin(LocalDateTimeTranslator.TranslateFromWhen(message.When), message.Organization);
    }

    public static Origin ToOrigin(this ImportedOriginProperties message)
    {
        return new Origin(message.Since, message.Organization);
    }
}
