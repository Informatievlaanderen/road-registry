namespace RoadRegistry.BackOffice.Uploads
{
    public interface ITranslatedChange
    {
        void TranslateTo(Messages.RequestedChange message);
    }
}
