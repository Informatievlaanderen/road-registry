namespace Shaperon
{
    public class DbaseFieldWriteProperties
    {
        public DbaseFieldWriteProperties(DbaseField field, char padding, DbaseFieldPadding paddingSide)
            : this(field.Name, field.Length, padding, paddingSide)
        {}

        public DbaseFieldWriteProperties(string name, int length, char padding, DbaseFieldPadding pad)
        {
            Name = name;
            Length = length;
            Padding = padding;
            Pad = pad;
        }

        public string Name { get; }
        public int Length { get; }
        public char Padding { get; }
        public DbaseFieldPadding Pad { get; }
    }

    public enum DbaseFieldPadding {
        Left,
        Right,
    }
}
