namespace RoadRegistry.BackOffice.Framework;

using System.IO;
using Microsoft.Extensions.Logging.Abstractions;

public class RoadRegistryLambdaLoggerFormatter
{
    public void Write<TState>(in LogEntry<TState> logEntry, TextWriter textWriter)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        textWriter.Write(message);
    }

    public const string Name = "RoadRegistryLambdaFormatter";
}
