namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Schema;

    public class BackOfficeProjection : ConnectedProjection<BackOfficeContext>
    {
        public BackOfficeProjection(params ConnectedProjection<BackOfficeContext>[] projections)
        {
            if (projections == null) throw new ArgumentNullException(nameof(projections));

            //HACK: Don't try this at home
            var allHandlers = (List<ConnectedProjectionHandler<BackOfficeContext>>)
                typeof(ConnectedProjection<BackOfficeContext>)
                .GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);

            allHandlers.AddRange(
                from @group
                    in projections
                        .SelectMany(projection => projection.Handlers)
                        .GroupBy(handler => handler.Message)
                let handlers = @group.ToArray()
                select new ConnectedProjectionHandler<BackOfficeContext>(@group.Key, async (context, envelope, token) =>
                {
                    foreach (var handler in handlers)
                    {
                        await handler.Handler(context, envelope, token);
                    }
                }));
        }
    }
}
