namespace StreetNameRegistry.Consumer.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Microsoft.Extensions.Logging;

    public class CommandHandler
    {
        private readonly ILifetimeScope _container;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(ILifetimeScope container, ILogger<CommandHandler> logger)
        {
            _container = container;
            _logger = logger;
        }

        public virtual async Task Handle<T>(T command, CancellationToken cancellationToken)
            where T : class
        {
            _logger.LogDebug($"Handling {command.GetType().FullName}");

            await using var scope = _container.BeginLifetimeScope();

            var resolver = scope.Resolve<ICommandHandlerResolver>();
            _ = await resolver.Dispatch(command.CreateCommandId(), command, cancellationToken:cancellationToken);




            _logger.LogDebug($"Handled {command.GetType().FullName}");
        }
    }
}
