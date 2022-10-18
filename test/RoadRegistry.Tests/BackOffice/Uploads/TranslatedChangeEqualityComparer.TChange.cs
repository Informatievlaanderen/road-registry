namespace RoadRegistry.Tests.BackOffice.Uploads;

using RoadRegistry.BackOffice.Uploads;

public class TranslatedChangeEqualityComparer<T> : IEqualityComparer<ITranslatedChange>
{
    public TranslatedChangeEqualityComparer(IEqualityComparer<T> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    private readonly IEqualityComparer<T> _comparer;

    public bool Equals(ITranslatedChange left, ITranslatedChange right)
    {
        var result = _comparer.Equals((T)left, (T)right);
        return result;
    }

    public int GetHashCode(ITranslatedChange instance)
    {
        throw new NotSupportedException();
    }
}
