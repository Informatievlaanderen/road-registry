namespace Shaperon
{
    public interface IDbaseFieldValueInspector
    {
        void Inspect(DbaseDateTime value);
        void Inspect(DbaseDouble value);
        void Inspect(DbaseInt32 value);
        void Inspect(DbaseString value);
    }
}