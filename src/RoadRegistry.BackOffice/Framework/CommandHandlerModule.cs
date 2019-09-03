namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class CommandHandlerModule
    {
        private readonly List<CommandHandler> _handlers;

        protected CommandHandlerModule()
        {
            _handlers = new List<CommandHandler>();
        }

        public CommandHandler[] Handlers => _handlers.ToArray();

        protected void Handle<TCommand>(Func<Command<TCommand>, CancellationToken, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            _handlers.Add(
                new CommandHandler(
                    typeof(TCommand),
                    (message, ct) => handler(new Command<TCommand>(message), ct)
            ));
        }

        protected ICommandHandlerBuilder<TCommand> For<TCommand>()
        {
            return new CommandHandlerBuilder<TCommand>(handler =>
            {
                _handlers.Add(
                    new CommandHandler(
                        typeof(TCommand),
                        (message, ct) => handler(new Command<TCommand>(message), ct)
                ));
            });
        }
    }
}
