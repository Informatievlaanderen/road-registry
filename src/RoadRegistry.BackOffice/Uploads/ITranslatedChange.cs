namespace RoadRegistry.BackOffice.Uploads;

using Messages;

public interface ITranslatedChange
{
    void TranslateTo(RequestedChange message);
}
