namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;

internal static class CreateOutlineV2Parameters
{
    // The length of GeometryTranslatorTestCases.ValidGmlLineStringLambert08.
    public const double GeometryLength = 10;

    // A request that passes validation, with every attribute value stated over the whole segment without positions.
    public static CreateOutlinedRoadSegmentV2Parameters Valid()
    {
        return new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Wegsegmentstatus = RoadSegmentStatusV2.Gepland.ToDutchString(),
            Morfologie = [new MorfologieParameters { Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString() }],
            Wegverharding = [new WegverhardingParameters { Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }],
            Toegang = [new ToegangParameters { Toegang = RoadSegmentAccessRestrictionV2.OpenbareWeg.ToDutchString() }],
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                    Identificator = "https://data.vlaanderen.be/id/straatnaam/71671"
                }
            ],
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Beide.ToDutchString(),
                    Wegbeheerder = "AGIV"
                }
            ],
            Wegcategorie = [new WegcategorieParameters { Wegcategorie = RoadSegmentCategoryV2.RegionaleWeg.ToDutchString() }],
            VerkeerstypeAuto = [new VerkeerstypeParameters { Richting = RoadSegmentTrafficDirection.Forward.ToDutchString() }],
            VerkeerstypeFiets = [new VerkeerstypeParameters { Richting = RoadSegmentTrafficDirection.Both.ToDutchString() }],
            VerkeerstypeVoetganger = [new VerkeerstypeVoetgangerParameters { Richting = RoadSegmentPedestrianTrafficDirection.Both.ToDutchString() }]
        };
    }
}
