namespace RoadRegistry.Framework
{
    using System;
    using System.Collections.Generic;

    public class Message<TBody>
    {
        public Message(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            Head = message.Head;
            Body = (TBody)message.Body;
        }

        public IDictionary<string, object> Head { get; }
        public TBody Body { get; }
    }
}