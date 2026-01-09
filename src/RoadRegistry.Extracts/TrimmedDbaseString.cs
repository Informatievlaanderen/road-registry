namespace RoadRegistry.Extracts
{
    using System.IO;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class TrimmedDbaseString: DbaseString
    {
        public TrimmedDbaseString(DbaseField field, string value = null)
            : base(field, value)
        {
        }

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);

            CleanValue();
        }

        public override void Write(BinaryWriter writer)
        {
            CleanValue();

            base.Write(writer);
        }

        private void CleanValue()
        {
            if (Value is not null)
            {
                Value = Value.Trim();
            }
        }
    }
}
