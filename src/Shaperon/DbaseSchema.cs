using System;

namespace Shaperon
{
    public abstract class DbaseSchema
    {
        public const int MaximumFieldCount = 128;

        private DbaseField[] _fields = Array.Empty<DbaseField>();

        protected DbaseSchema() { }

        public DbaseField[] Fields
        {
            get => _fields;
            protected set {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value.Length > MaximumFieldCount)
                {
                    throw new ArgumentException($"The value with ({value.Length}) fields exceeds the maximum number of fields ({MaximumFieldCount}).", nameof(value));
                }
                _fields = value;
            }
        }

        //public RecordLength RecordLength => Fields.Aggregate(new RecordLength(0), (length, field) => length.Plus(field.Length));
    }
}
