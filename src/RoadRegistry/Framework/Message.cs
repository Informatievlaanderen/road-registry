namespace RoadRegistry.Framework
{
    using System;
    using System.Collections.Generic;

    public class Message
    {
        public Message(IDictionary<string, object> head, object body)
        {
            Head = head ?? throw new ArgumentNullException(nameof(head));
            Body = body ?? throw new ArgumentNullException(nameof(body)); ;
        }

        public IDictionary<string, object> Head { get; }
        public object Body { get; }
    }
}
