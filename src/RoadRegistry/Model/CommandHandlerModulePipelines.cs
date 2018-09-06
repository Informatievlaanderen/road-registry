namespace RoadRegistry.Model
{
    using FluentValidation;
    using Framework;

    internal static class CommandHandlerModulePipelines
    {
        public static ICommandHandlerBuilder<TContext, TCommand> ValidateUsing<TContext, TCommand>(
            this ICommandHandlerBuilder<TContext, TCommand> builder, IValidator<TCommand> validator)
        {
            return builder.Pipe(next => async (context, message, ct) =>
            {
                await validator.ValidateAndThrowAsync(message.Body, cancellationToken: ct);
                await next(context, message, ct);
            });
        }
    }
}
