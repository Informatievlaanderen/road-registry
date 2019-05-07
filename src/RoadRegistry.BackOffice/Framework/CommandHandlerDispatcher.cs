namespace RoadRegistry.BackOffice.Framework
{
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task CommandHandlerDispatcher(Message message, CancellationToken cancellationToken);
}