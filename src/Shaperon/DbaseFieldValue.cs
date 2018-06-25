using System;
using System.IO;

namespace Shaperon
{
    public abstract class DbaseFieldValue
    {
        protected DbaseFieldValue(DbaseField field)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public DbaseField Field { get; }
        public abstract void Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);
        public abstract void Inspect(IDbaseFieldValueInspector inspector);

        public override bool Equals(object obj)
        {
            return obj is DbaseFieldValue fieldValue
                   && Equals(fieldValue);
        }

        protected bool Equals(DbaseFieldValue other)
        {
            return Equals(Field, other.Field);
        }

        public override int GetHashCode()
        {
            return (Field != null ? Field.GetHashCode() : 0);
        }
    }
}
