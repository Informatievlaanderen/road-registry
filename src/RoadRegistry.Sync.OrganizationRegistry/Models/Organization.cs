namespace RoadRegistry.Sync.OrganizationRegistry.Models;

using System;
using System.Collections.Generic;

public class Organization
{
    public int ChangeId { get; set; }
    public string Name { get; set; }
    public string OvoNumber { get; set; }
    public string? KboNumber { get; set; }
    public IEnumerable<Key>? Keys { get; set; }
}

public class Key
{
    public string? KeyTypeName { get; set; }
    public string? Value { get; set; }
    public Validity? Validity { get; set; }
}

public class Validity
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}
