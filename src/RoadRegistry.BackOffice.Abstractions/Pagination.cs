namespace RoadRegistry.BackOffice.Abstractions
{
    public sealed class Pagination
    {
        public const int MaxLimit = 200;

        public int? Offset
        {
            get => _offset;
            set => _offset = value is null or < 0 ? 0 : value.Value;
        }

        public int? Limit
        {
            get => _limit;
            set => _limit = value is null or 0 or > MaxLimit ? MaxLimit : value.Value;
        }

        public Pagination(int? offset, int? limit)
        {
            Offset = offset;
            Limit = limit;
        }

        public int NextPageOffset => _offset + _limit;

        private int _limit = MaxLimit;
        private int _offset = 0;
    }
}
