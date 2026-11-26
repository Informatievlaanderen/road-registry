namespace RoadRegistry.Extracts.Infrastructure.Dbase;

using System;
using System.Collections.Generic;

public static class DbaseFields
{
    // Written into the files and read back by nobody: an upload is judged on its identifiers, its geometry and its
    // attributes. Reading them meant refusing a value that is not in the format they are written in, and that
    // refusal stopped the reader on the record it hit, leaving every record behind it unread - which is how an
    // extract came to be read only halfway (the road segments it never read were never matched and never removed).
    public static readonly IReadOnlySet<string> NotRead =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CREATIE",
            "VERSIE"
        };
}
