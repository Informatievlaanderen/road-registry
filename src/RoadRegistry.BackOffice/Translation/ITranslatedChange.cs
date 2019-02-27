namespace RoadRegistry.BackOffice.Translation
{
    public interface ITranslatedChange
    {
        void TranslateTo(Messages.RequestedChange message);
    }
}
