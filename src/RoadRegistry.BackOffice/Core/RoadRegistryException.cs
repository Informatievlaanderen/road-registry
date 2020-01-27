namespace RoadRegistry.BackOffice.Core
{
    using System;

    public abstract class RoadRegistryException : Exception
    {
        protected RoadRegistryException() { }

        protected RoadRegistryException(string message) : base(message) { }

        protected RoadRegistryException(string message, Exception inner) : base(message, inner) { }
    }
}
