namespace RoadRegistry.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class CommandHandlerModule<TContext>
    {
        private readonly List<CommandHandler<TContext>> _handlers;

        protected CommandHandlerModule()
        {
            _handlers = new List<CommandHandler<TContext>>();
        }

        public CommandHandler<TContext>[] Handlers => _handlers.ToArray();

        protected void Handle<TCommand>(Func<TContext, Message<TCommand>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            _handlers.Add(
                new CommandHandler<TContext>(
                    typeof(TCommand),
                    (context, message, ct) => handler(context, new Message<TCommand>(message), ct)
            ));
        }

        protected ICommandHandlerBuilder<TContext, TCommand> For<TCommand>()
        {
            return new CommandHandlerBuilder<TContext, TCommand>(handler =>
            {
                _handlers.Add(
                    new CommandHandler<TContext>(
                        typeof(TCommand),
                        (context, message, ct) => handler(context, new Message<TCommand>(message), ct)
                ));
            });
        }
    }
}
