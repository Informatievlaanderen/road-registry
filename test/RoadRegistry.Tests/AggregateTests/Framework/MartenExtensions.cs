namespace RoadRegistry.Tests.AggregateTests.Framework;

using System.Text;
using Marten;

public static class MartenExtensions
{
    public static T FromJson<T>(this ISerializer serializer, string json)
    {
        var byteArray = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(byteArray);
        return serializer.FromJson<T>(stream);
    }

    public static object FromJson(this ISerializer serializer, Type type, string json)
    {
        var byteArray = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(byteArray);
        return serializer.FromJson(type, stream);
    }
}
