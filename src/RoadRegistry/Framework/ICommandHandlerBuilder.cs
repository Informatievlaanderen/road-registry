namespace RoadRegistry.Framework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandHandlerBuilder<TCommand>
    {
        ICommandHandlerBuilder<TCommand> Pipe(
          Func<
            Func<Message<TCommand>, CancellationToken, Task>,
            Func<Message<TCommand>, CancellationToken, Task>> pipe);

        void Handle(Func<Message<TCommand>, CancellationToken, Task> handler);
    }
}
