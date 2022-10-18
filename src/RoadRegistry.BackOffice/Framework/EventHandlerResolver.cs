namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

public delegate Func<Event, CancellationToken, Task>[] EventHandlerResolver(Event @event);