namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;

using System;

public class Origin
{
    public Origin()
    {
    }

    public Origin(DateTimeOffset? timestamp, string organization)
    {
        Timestamp = timestamp;
        Organization = organization;
    }

    public DateTimeOffset? Timestamp { get; set; }
    public string Organization { get; set; }

    public override bool Equals(object obj)
    {
        return obj is Origin other && Equals(this, other);
    }

    public bool Equals(Origin x, Origin y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Timestamp == y.Timestamp
               && x.Organization == y.Organization;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Timestamp, Organization);
    }
}
