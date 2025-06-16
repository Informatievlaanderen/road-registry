namespace MartenPoc;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed record ScopedWegennetwerk
{
    private readonly IList<Wegknoop> _wegknopen;
    private readonly IList<Wegsegment> _wegsegmenten;

    public Dictionary<Guid, List<object>> UncommittedEvents { get; private set; } = [];

    public static ScopedWegennetwerk Empty => new ScopedWegennetwerk([], []);

    public IEnumerable<Wegknoop> Wegknopen => _wegknopen;
    public IEnumerable<Wegsegment> Wegsegmenten => _wegsegmenten;

    public ScopedWegennetwerk(
        IEnumerable<Wegknoop> wegknopen,
        IEnumerable<Wegsegment> wegsegmenten)
    {
        _wegknopen = wegknopen.ToList();
        _wegsegmenten = wegsegmenten.ToList();
    }

    public Wegsegment VoegWegsegmentToe(
        Guid wegsegmentId,
        string geometry,
        Guid beginknoopId,
        Guid eindknoopId,
        string beheerder,
        string attribuut1,
        string attribuut2,
        string attribuut3,
        string attribuut4,
        string attribuut5,
        string attribuut6,
        string attribuut7,
        string attribuut8)
    {
        if (_wegsegmenten.Any(x => x.Id == wegsegmentId))
        {
            throw new Exception("Wegsegment bestaat al");
        }

        AddEvent(wegsegmentId, new WegsegmentWerdToegevoegd(
            wegsegmentId,
            geometry,
            beginknoopId,
            eindknoopId,
            beheerder,
            attribuut1,
            attribuut2,
            attribuut3,
            attribuut4,
            attribuut5,
            attribuut6,
            attribuut7,
            attribuut8));

        return _wegsegmenten.Single(x => x.Id == wegsegmentId);
    }

    public void WijzigWegsegment(
        Guid wegsegmentId,
        string geometry,
        Guid beginknoopId,
        Guid eindknoopId,
        string beheerder,
        string attribuut1,
        string attribuut2,
        string attribuut3,
        string attribuut4,
        string attribuut5,
        string attribuut6,
        string attribuut7,
        string attribuut8)
    {
        if (_wegsegmenten.All(x => x.Id != wegsegmentId))
        {
            throw new Exception("Wegsegment bestaat niet");
        }

        AddEvent(wegsegmentId, new WegsegmentWerdGewijzigd(
            wegsegmentId,
            geometry,
            beginknoopId,
            eindknoopId,
            beheerder,
            attribuut1,
            attribuut2,
            attribuut3,
            attribuut4,
            attribuut5,
            attribuut6,
            attribuut7,
            attribuut8));
    }

    public void VoegWegknoopToe(
        Guid wegknoopId,
        string geometry,
        string type)
    {
        if (_wegknopen.Any(x => x.Id == wegknoopId))
        {
            throw new Exception("Wegknoop bestaat al");
        }

        AddEvent(wegknoopId, new WegknoopWerdToegevoegd(
            wegknoopId,
            geometry,
            type));
    }

    public void WijzigWegknoop(
        Guid wegknoopId,
        string geometry,
        string type)
    {
        if (_wegknopen.All(x => x.Id != wegknoopId))
        {
            throw new Exception("Wegknoop bestaat niet");
        }

        AddEvent(wegknoopId, new WegknoopWerdGewijzigd(
            wegknoopId,
            geometry,
            type));
    }

    private void AddEvent(Guid aggregateId, object @event)
    {
        Apply(aggregateId, @event);
        UncommittedEvents.TryAdd(aggregateId, []);
        UncommittedEvents[aggregateId].Add(@event);
    }

    private void Apply(Guid aggregateId, dynamic @event)
    {
        switch (@event)
        {
            case IWegknoopEvent and WegknoopWerdToegevoegd wegknoopWerdToegevoegd:
                _wegknopen.Add(Wegknoop.Create(wegknoopWerdToegevoegd));
                break;
            case IWegknoopEvent wegknoopEvent:
            {
                var wegknoop = _wegknopen.Single(x => x.Id == aggregateId);
                wegknoop.Apply(@event);
                break;
            }
            case IWegsegmentEvent and WegsegmentWerdToegevoegd wegsegmentWerdToegevoegd:
                _wegsegmenten.Add(Wegsegment.Create(wegsegmentWerdToegevoegd));
                break;
            case IWegsegmentEvent wegsegmentEvent:
            {
                var wegsegment = _wegsegmenten.Single(x => x.Id == aggregateId);
                wegsegment.Apply(@event);
                break;
            }
        }
    }
}
