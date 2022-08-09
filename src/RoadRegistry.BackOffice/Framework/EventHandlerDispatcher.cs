namespace RoadRegistry.BackOffice.Framework;

using System.Threading;
using System.Threading.Tasks;

public delegate Task EventHandlerDispatcher(Event @event, CancellationToken cancellationToken);
