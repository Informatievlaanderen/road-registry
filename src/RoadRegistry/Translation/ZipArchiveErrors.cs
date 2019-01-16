namespace RoadRegistry.Translation
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Model;

    public class ZipArchiveErrors: IReadOnlyCollection<Error>
    {
        private readonly ImmutableList<Error> _errors;

        public static readonly ZipArchiveErrors None = new ZipArchiveErrors(ImmutableList<Error>.Empty);

        private ZipArchiveErrors(ImmutableList<Error> errors)
        {
            _errors = errors;
        }

        public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _errors.Count;

        public ZipArchiveErrors FileMissing(string file)
        {
            return this;
        }
    }
}