namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class StreetName : EventSourcedEntity
{
    public static readonly Func<StreetName> Factory = () => new StreetName();

    private StreetName()
    {
        On<StreetNameCreated>(e =>
        {
            //TODO-rik
        });
        On<StreetNameModified>(e =>
        {
            //TODO-rik
        });
        On<StreetNameRemoved>(e =>
        {
            IsRemoved = true;
        });
    }

    public StreetNameId Id { get; private set; }
    public bool IsRemoved { get; private set; }
    
    public void Modify()
    {
        Apply(new StreetNameModified
        {
            //Id = Id, //TODO-rik
        });
    }

    public void Delete()
    {
        Apply(new StreetNameRemoved
        {
            //Id = Id, //TODO-rik
        });
    }
}
