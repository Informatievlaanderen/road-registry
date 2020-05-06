namespace RoadRegistry.Framework.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    internal class DbaseRecordComparer<TDbaseRecord> : IEqualityComparer<TDbaseRecord>
        where TDbaseRecord :DbaseRecord
    {
        public bool Equals(TDbaseRecord x, TDbaseRecord y)
        {
            if (null == x && null == y)
                return true;

            if (null == x || null == y || x.GetType() != y.GetType())
                return false;

            return x.IsDeleted == y.IsDeleted && Equal(x.Values, y.Values);
        }

        private bool Equal(DbaseFieldValue[] xValues, DbaseFieldValue[] yValues)
        {
            if (null == xValues && null == yValues)
                return true;

            if (xValues?.Length != yValues?.Length)
                return false;

            return xValues
                .Select<DbaseFieldValue, Func<bool>>((_, i) => { return () => Equals(xValues[i], yValues[i]); })
                .All(areEqual => areEqual());
        }

        private static bool Equals(DbaseFieldValue x, DbaseFieldValue y)
        {
            if (null == x && null == y)
                return false;

            if (
                null == x
                || null == y
                || x.GetType() != y.GetType()
                || false == x.Field.Equals(y.Field))
                return false;

            if (x is DbaseDateTime xDateTime && y is DbaseDateTime yDateTime)
                return Equals(xDateTime.Value, yDateTime.Value);

            if (x is DbaseInt32 xInt32 && y is DbaseInt32 yInt32)
                return Equals(xInt32.Value, yInt32.Value);

            if (x is DbaseSingle xSingle && y is DbaseSingle ySingle)
                return Equals(xSingle.Value, ySingle.Value);

            if (x is DbaseDouble xDouble && y is DbaseDouble yDouble)
                return Equals(xDouble.Value, yDouble.Value);

            if (x is DbaseString xString && y is DbaseString yString)
                return Equals(xString.Value, yString.Value);

            throw new NotImplementedException($"No equality impelemented for {x.GetType().FullName}");
        }

        public int GetHashCode(TDbaseRecord obj)
        {
            unchecked
            {
                return
                    typeof(TDbaseRecord).Name.GetHashCode()
                    ^ (obj?.IsDeleted.GetHashCode() ?? 0 * 397)
                    ^ GetValuesHash(obj?.Values);
            }
        }

        private static int GetValuesHash(DbaseFieldValue[] values)
        {
            if (null == values || 0 == values.Length)
                return 0;

            return values
                .Select(GetHashCode)
                .Aggregate((i, j) => { unchecked { return i ^ j; } });
        }

        private static int GetHashCode(DbaseFieldValue fieldValue)
        {
            if (null == fieldValue)
                return 0;

            int CreateFieldHashForValue(object value)
            {
                unchecked
                {
                    return (fieldValue.Field?.GetHashCode() ?? 0) ^ 397 ^ value.GetHashCode();
                }
            }

            if (fieldValue is DbaseDateTime dbaseDateTime)
                return CreateFieldHashForValue(dbaseDateTime.Value);

            if (fieldValue is DbaseInt32 dbaseInt32)
                return CreateFieldHashForValue(dbaseInt32.Value);

            if (fieldValue is DbaseSingle dbaseSingle)
                return CreateFieldHashForValue(dbaseSingle.Value);

            if (fieldValue is DbaseDouble dbaseDouble)
                return CreateFieldHashForValue(dbaseDouble.Value);

            if (fieldValue is DbaseString dbaseString)
                return CreateFieldHashForValue(dbaseString.Value);

            throw new NotImplementedException($"No GetHashCode implementation for {fieldValue.GetType().FullName}");
        }
    }
}
