namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

using System;
using System.Collections.Generic;

public class RoadNetworkExtractDownloadBecameAvailableEntry
{
    public ArchiveInfo Archive { get; set; }
    public ICollection<Guid> OverlapsWithDownloadIds { get; set; }
}
