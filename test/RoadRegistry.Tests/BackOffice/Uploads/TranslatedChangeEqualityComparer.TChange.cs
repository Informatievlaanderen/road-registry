namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;

    public class TranslatedChangeEqualityComparer<T> : IEqualityComparer<ITranslatedChange>
    {
        private readonly IEqualityComparer<T> _comparer;

        public TranslatedChangeEqualityComparer(IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public bool Equals(ITranslatedChange left, ITranslatedChange right)
        {
            return _comparer.Equals((T)left, (T)right);
        }

        public int GetHashCode(ITranslatedChange instance)
        {
            throw new NotSupportedException();
        }
    }
}
