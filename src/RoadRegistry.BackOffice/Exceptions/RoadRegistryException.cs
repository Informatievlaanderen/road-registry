namespace RoadRegistry.BackOffice.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public abstract class RoadRegistryException : DomainException
    {
        protected RoadRegistryException() { }

        protected RoadRegistryException(string message) : base(message) { }

        protected RoadRegistryException(string message, Exception inner) : base(message, inner) { }

        protected RoadRegistryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
