namespace RoadRegistry.Structurizr;

using System;
using System.Collections.Generic;

public class EventInfo
{
    public string Description { get; set; }
    public string Name { get; set; }
    public List<string> Properties { get; set; }
    public Type Type { get; set; }
}