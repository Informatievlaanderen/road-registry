namespace RoadRegistry.BackOffice.Api.Infrastructure.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    public class EnumFilter<TEnum>
        where TEnum : struct, Enum
    {
        private List<TEnum> EnumList { get; } = new();

        public EnumFilter(IQueryCollection queryCollection, string parameterName)
        {
            var stringEnums = queryCollection[parameterName].ToString();

            if (string.IsNullOrWhiteSpace(stringEnums))
            {
                return;
            }

            var stringEnumList = stringEnums
                .Replace(" ", string.Empty)
                .Split(",")
                .ToList();

            stringEnumList.ForEach(enumStringValue =>
            {
                if (Enum.TryParse(enumStringValue, true, out TEnum ts))
                {
                    EnumList.Add(ts);
                }
                else
                {
                    throw new InvalidOperationException($"Value '{enumStringValue}' could not be parsed into {typeof(TEnum)}.");
                }
            });
        }

        public static implicit operator List<TEnum>(EnumFilter<TEnum> f) => f.EnumList;
    }
}
