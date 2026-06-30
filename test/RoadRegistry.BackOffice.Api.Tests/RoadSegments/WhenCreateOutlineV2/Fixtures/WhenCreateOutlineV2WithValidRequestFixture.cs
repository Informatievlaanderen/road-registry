namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2.Fixtures;

using Abstractions.Fixtures;
using MediatR;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.BackOffice;
using RoadRegistry.Tests.BackOffice;

public class WhenCreateOutlineV2WithValidRequestFixture : WhenCreateOutlineV2Fixture
{
    public WhenCreateOutlineV2WithValidRequestFixture(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateOutlinedRoadSegmentV2Parameters CreateRequest()
    {
        const double length = 10;

        return new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Wegsegmentstatus = RoadSegmentStatusV2.Gepland.ToDutchString(),
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ],
            Wegverharding =
            [
                new WegsegmentWegverhardingAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString()
                }
            ],
            Toegang =
            [
                new WegsegmentToegangAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Toegang = RoadSegmentAccessRestrictionV2.OpenbareWeg.ToDutchString()
                }
            ],
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = WegsegmentKant.Beide,
                    VanPositie = 0,
                    TotPositie = length,
                    Identificator = "https://data.vlaanderen.be/id/straatnaam/71671"
                }
            ],
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = WegsegmentKant.Beide,
                    VanPositie = 0,
                    TotPositie = length,
                    Wegbeheerder = "AWV"
                }
            ],
            Wegcategorie =
            [
                new WegsegmentWegcategorieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Wegcategorie = RoadSegmentCategoryV2.RegionaleWeg.ToDutchString()
                }
            ],
            VerkeerstypeAuto =
            [
                new WegsegmentVerkeerstypeAutoAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.Forward.ToDutchString()
                }
            ],
            VerkeerstypeFiets =
            [
                new WegsegmentVerkeerstypeFietsAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.Both.ToDutchString()
                }
            ],
            VerkeerstypeVoetganger =
            [
                new WegsegmentVerkeerstypeVoetgangerAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.None.ToDutchString()
                }
            ]
        };
    }
}
