namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Collections.Generic;

public record ListOverlappingResponse(List<Guid> DownloadIds);
