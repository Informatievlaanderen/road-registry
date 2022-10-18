namespace RoadRegistry.BackOffice.Framework;

public static class StreamNameConversions
{
    public static StreamNameConverter PassThru =>
        name => name;

    public static StreamNameConverter WithoutPrefix(string prefix)
    {
        return name => name.WithoutPrefix(prefix);
    }

    public static StreamNameConverter WithoutSuffix(string suffix)
    {
        return name => name.WithoutSuffix(suffix);
    }

    public static StreamNameConverter WithPrefix(string prefix)
    {
        return name => name.WithPrefix(prefix);
    }

    public static StreamNameConverter WithSuffix(string suffix)
    {
        return name => name.WithSuffix(suffix);
    }
}