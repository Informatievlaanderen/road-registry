namespace RoadRegistry.Framework
{
    public delegate CommandHandler<TContext> CommandHandlerResolver<TContext>(Message message);
}