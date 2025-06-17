namespace MartenPoc.Projections;

using System.Collections.Generic;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;

public interface IRoadNetworkChangeProjection
{
    Task Project(ICollection<IEvent> events, IDocumentSession session);
}
