namespace RoadRegistry.BackOffice.Exceptions
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class RoadRegistryException : DomainException
    {
        protected RoadRegistryException() { }

        protected RoadRegistryException(string message) : base(message) { }

        protected RoadRegistryException(string message, Exception inner) : base(message, inner) { }
    }
}
