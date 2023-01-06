//namespace RoadRegistry.BackOffice.Commands;

//using System;
//using System.Collections.Generic;
//using Be.Vlaanderen.Basisregisters.Generators.Guid;
//using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//using Be.Vlaanderen.Basisregisters.Utilities;

//public sealed class LinkStreetNameCommand : IHasCommandProvenance
//{
//    private static readonly Guid Namespace = new("f5693e71-8a47-4fbd-b97c-12ff63a24e64");

//    public LinkStreetNameCommand(
//        RoadSegmentId roadSegmentId,
//        string leftStreetNameId,
//        string rightStreetNameId,
//        Provenance provenance)
//    {
//        RoadSegmentId = roadSegmentId;
//        LeftStreetNameId = leftStreetNameId;
//        RightStreetNameId = rightStreetNameId;
//        Provenance = provenance;
//    }

//    public RoadSegmentId RoadSegmentId { get; }
//    public string LeftStreetNameId { get; }
//    public string RightStreetNameId { get; }
//    public Provenance Provenance { get; }

//    public Guid CreateCommandId()
//    {
//        return Deterministic.Create(Namespace, $"LinkStreetName-{ToString()}");
//    }

//    private IEnumerable<object> IdentityFields()
//    {
//        yield return RoadSegmentId;
//        yield return LeftStreetNameId;
//        yield return RightStreetNameId;

//        foreach (var field in Provenance.GetIdentityFields())
//        {
//            yield return field;
//        }
//    }

//    public override string ToString()
//    {
//        return ToStringBuilder.ToString(IdentityFields());
//    }
//}
