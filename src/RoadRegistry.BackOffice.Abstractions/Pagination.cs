namespace RoadRegistry.BackOffice.Abstractions
{
    using System;
    using Microsoft.AspNetCore.Http;

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

        public Pagination(IQueryCollection queryCollection)
        {
            Offset = Convert.ToInt32(queryCollection["offset"]);
            Limit = Convert.ToInt32(queryCollection["limit"]);
        }

        public int NextPageOffset => _offset + _limit;

        private int _limit = MaxLimit;
        private int _offset = 0;
    }
}
