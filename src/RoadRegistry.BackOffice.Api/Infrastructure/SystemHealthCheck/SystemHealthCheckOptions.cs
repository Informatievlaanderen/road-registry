﻿namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck;

using System;
using System.Collections.Generic;

public sealed class SystemHealthCheckOptions
{
    public required ICollection<Type> HealthCheckTypes { get; init; }
}