namespace Shaperon
{
    using System.Collections.Generic;

    public class DbaseFieldValueEqualityComparer : IEqualityComparer<DbaseFieldValue>
    {
        public bool Equals(DbaseFieldValue left, DbaseFieldValue right)
        {
            if(left == null && right == null) return true;
            if(left == null || right == null) return false;
            var leftInspector = new ValueInspector();
            var rightInspector = new ValueInspector();
            left.Inspect(leftInspector);
            right.Inspect(rightInspector);
            return left.Field.Equals(right.Field)
                && Equals(leftInspector.Value, rightInspector.Value);
        }

        public int GetHashCode(DbaseFieldValue instance)
        {
            var inspector = new HashCodeInspector();
            instance.Inspect(inspector);
            return instance.Field.GetHashCode() ^ inspector.HashCode;
        }

        private class ValueInspector : IDbaseFieldValueInspector
        {
            public object Value { get; private set; }

            public void Inspect(DbaseDateTime value)
            {
                Value = value.Value;
            }

            public void Inspect(DbaseDouble value)
            {
                Value = value.Value;
            }

            public void Inspect(DbaseSingle value)
            {
                Value = value.Value;
            }

            public void Inspect(DbaseInt32 value)
            {
                Value = value.Value;
            }

            public void Inspect(DbaseString value)
            {
                Value = value.Value;
            }
        }

        private class HashCodeInspector : IDbaseFieldValueInspector
        {
            public int HashCode { get; private set; }
            public void Inspect(DbaseDateTime value)
            {
                HashCode = value.Value.HasValue
                    ? value.Value.Value.GetHashCode()
                    : 0;
            }

            public void Inspect(DbaseDouble value)
            {
                HashCode = value.Value.HasValue
                    ? value.Value.Value.GetHashCode()
                    : 0;
            }
            
            public void Inspect(DbaseSingle value)
            {
                HashCode = value.Value.HasValue
                    ? value.Value.Value.GetHashCode()
                    : 0;
            }

            public void Inspect(DbaseInt32 value)
            {
                HashCode = value.Value.HasValue
                    ? value.Value.Value.GetHashCode()
                    : 0;
            }

            public void Inspect(DbaseString value)
            {
                HashCode = value.Value != null
                    ? value.Value.GetHashCode()
                    : 0;
            }
        }
    }
}
