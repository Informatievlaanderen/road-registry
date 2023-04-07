namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using System;
using BackOffice.Extracts.Dbase.RoadSegments;
using Core.ProblemCodes;
using Editor.Projections;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;

public class RoadSegmentOutlinedIdValidator : AbstractValidator<int>
{
    public RoadSegmentOutlinedIdValidator(EditorContext editorContext, RecyclableMemoryStreamManager recyclableMemoryStreamManager, FileEncoding fileEncoding)
    {
        ArgumentNullException.ThrowIfNull(editorContext);

        RuleFor(x => x).Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.IsValid)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            .MustAsync((id, cancellationToken) =>
            {
                return editorContext.RoadSegments.AnyAsync(x => x.Id == id, cancellationToken);
            })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.NotFound)
            .MustAsync(async (id, cancellationToken) =>
            {
                var roadSegment = await editorContext.RoadSegments.FindAsync(new object[]{ id }, cancellationToken);
                var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment!.DbaseRecord, recyclableMemoryStreamManager, fileEncoding);
                return dbaseRecord.METHODE.Value == RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
            })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotOutlined);
    }
}
