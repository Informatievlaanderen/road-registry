namespace RoadRegistry.Extracts.Infrastructure.Extensions;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Be.Vlaanderen.Basisregisters.Shaperon;

public static class DbaseSchemaExtensions
{
    public static DbaseField GetField(this DbaseSchema schema, [CallerMemberName] string callerName = "")
    {
        var field = schema.Fields.SingleOrDefault(x => x.Name == callerName);
        if (field == null)
        {
            throw new InvalidOperationException($"No field with name '{callerName}' exists in schema '{schema}'");
        }

        return field;
    }
}
