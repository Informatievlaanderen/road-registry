namespace MartenPoc;

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

public interface IHasId
{
    Guid Id { get; }
}
public interface IHasGeometry
{
    string Geometry { get; }
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
    ) : IHasId, IWegsegmentEvent, ICreateEvent, IHasGeometry;

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
    string Attribuut8) : IHasId, IWegsegmentEvent, IHasGeometry;

public sealed record WegknoopWerdToegevoegd(Guid Id, string Geometry, string Type) : IHasId, IWegknoopEvent, ICreateEvent, IHasGeometry;

public sealed record WegknoopWerdGewijzigd(Guid Id, string Geometry, string Type) : IHasId, IWegknoopEvent, IHasGeometry;
