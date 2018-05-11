namespace Shaperon
{
    public interface IDbaseValueInspector
    {
        void Inspect(DbaseDateTime value);
        void Inspect(DbaseDouble value);
        void Inspect(DbaseInt32 value);
        void Inspect(DbaseString value);
    }
}