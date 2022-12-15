// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentDbaseSchema
{
    public RoadSegmentDbaseSchema()
    {
        Fields = Fields.Concat(new[]
        {
            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_UIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_GIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLSTATUS)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLMORF)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLWEGCAT)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LSTRNM)),
                    new DbaseFieldLength(80)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(RSTRNM)),
                    new DbaseFieldLength(80)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLBEHEER)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLMETHOD)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(OPNDATUM)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINTIJD)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLBGNORG)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTGBEP)),
                    new DbaseFieldLength(64))
        }).ToArray();
    }

    public DbaseField BEGINORG => this.GetField();
    public DbaseField BEGINTIJD => this.GetField();
    public DbaseField LBLBEHEER => this.GetField();
    public DbaseField LBLBGNORG => this.GetField();
    public DbaseField LBLMETHOD => this.GetField();
    public DbaseField LBLMORF => this.GetField();
    public DbaseField LBLSTATUS => this.GetField();
    public DbaseField LBLTGBEP => this.GetField();
    public DbaseField LBLWEGCAT => this.GetField();
    public DbaseField LSTRNM => this.GetField();
    public DbaseField OPNDATUM => this.GetField();
    public DbaseField RSTRNM => this.GetField();
    public DbaseField WS_GIDN => this.GetField();
    public DbaseField WS_UIDN => this.GetField();
}
