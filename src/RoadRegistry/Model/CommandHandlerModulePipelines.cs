namespace RoadRegistry.Model
{
    using FluentValidation;
    using Framework;

    internal static class CommandHandlerModulePipelines
    {
        public static ICommandHandlerBuilder<TCommand> ValidateUsing<TCommand>(
            this ICommandHandlerBuilder<TCommand> builder, IValidator<TCommand> validator)
        {
            return builder.Pipe(next => async (message, ct) =>
            {
                await validator.ValidateAndThrowAsync(message.Body, cancellationToken: ct);
                await next(message, ct);
            });
        }
    }
}
