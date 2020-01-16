namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Func<Command, CancellationToken, Task> CommandHandlerResolver(Command command);
}
