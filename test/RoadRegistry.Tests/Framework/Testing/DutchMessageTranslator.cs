namespace RoadRegistry.Framework.Testing;

using System;
using System.Collections.Generic;
using System.Security.Claims;

public class DutchMessageTranslator : IMessageTranslator
{
    public string Translate(object message)
    {
        string result = null;
        switch (message)
        {
            case Exception exception:
                result = exception.Message;

                break;
            case KeyValuePair<string, object> header:
                switch (header.Key)
                {
                    case "Subject":
                        var subject = (ClaimsPrincipal)header.Value;
                        result = $"uitgevoerd door {subject.Identity.Name}";
                        break;
                }

                break;
        }

        return result;
    }
}
