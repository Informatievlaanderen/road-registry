namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using ValueObjects;

//TODO-pr add unit test
public class ReusableAttributeIdGenerator
{
    private readonly IRoadNetworkIdGenerator _idGenerator;
    private readonly Func<IRoadNetworkIdGenerator, AttributeId> _getNewId;
    private readonly Queue<IRoadSegmentDynamicAttribute> _attributes;

    public ReusableAttributeIdGenerator(IRoadNetworkIdGenerator idGenerator, Func<IRoadNetworkIdGenerator, AttributeId> getNewId, IEnumerable<IRoadSegmentDynamicAttribute> attributes)
    {
        _idGenerator = idGenerator;
        _getNewId = getNewId;
        _attributes = new Queue<IRoadSegmentDynamicAttribute>(attributes);
    }

    public AttributeId GetNextId()
    {
        if (_attributes.Any())
        {
            return _attributes.Dequeue().Id;
        }

        return _getNewId(_idGenerator);
    }
}
