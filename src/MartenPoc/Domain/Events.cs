namespace MartenPoc.Domain;

using System;

public interface ICreateEvent
{
}

public interface IWegsegmentEvent
{
}

public interface IWegknoopEvent
{
}

public sealed record WegsegmentWerdToegevoegd(
    Guid Id,
    string Geometry,
    Guid StartNodeId,
    Guid EndNodeId,
    string Beheerder,
    string Attribuut1,
    string Attribuut2,
    string Attribuut3,
    string Attribuut4,
    string Attribuut5,
    string Attribuut6,
    string Attribuut7,
    string Attribuut8
    ) : IWegsegmentEvent, ICreateEvent;

public sealed record WegsegmentWerdGewijzigd(
    Guid Id,
    string Geometry,
    Guid StartNodeId,
    Guid EndNodeId,
    string Beheerder,
    string Attribuut1,
    string Attribuut2,
    string Attribuut3,
    string Attribuut4,
    string Attribuut5,
    string Attribuut6,
    string Attribuut7,
    string Attribuut8) : IWegsegmentEvent;

public sealed record WegknoopWerdToegevoegd(Guid Id, string Geometry, string Type) : IWegknoopEvent, ICreateEvent;

public sealed record WegknoopWerdGewijzigd(Guid Id, string Geometry, string Type) : IWegknoopEvent;
