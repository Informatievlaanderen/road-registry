namespace RoadRegistry.Model
{
    using System.Collections.Generic;

    public interface IRequestedChanges : IReadOnlyCollection<IRequestedChange>, IRequestedChangeIdentityTranslator
    {
    }
}
