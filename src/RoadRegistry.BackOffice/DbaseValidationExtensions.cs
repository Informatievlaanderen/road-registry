namespace RoadRegistry.BackOffice
{
    using Extracts.Dbase.RoadSegments;
    using Uploads;

    public static class ValidationExtensions
    {
        public static ZipArchiveProblems Validate<T>(this IDbaseFileRecordProblemBuilder recordContext, T record)
            where T : IRoadSegmentDbaseRecord
        {
            recordContext = recordContext.WithIdentifier(nameof(record.WS_OIDN), record.WS_OIDN.Value);

            var problems = ZipArchiveProblems.None;

            if (!record.METHODE.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.METHODE.Field);
            }
            else if (!RoadSegmentGeometryDrawMethod.ByIdentifier.ContainsKey(record.METHODE.Value)
                     || !RoadSegmentGeometryDrawMethod.ByIdentifier[record.METHODE.Value].IsAllowed())
            {
                problems += recordContext.RoadSegmentGeometryDrawMethodMismatch(record.METHODE.Value);
            }

            if (!record.CATEGORIE.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.CATEGORIE.Field);
            }
            else if (!RoadSegmentCategory.ByIdentifier.ContainsKey(record.CATEGORIE.Value))
            {
                problems += recordContext.RoadSegmentCategoryMismatch(record.CATEGORIE.Value);
            }

            if (!record.TGBEP.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.TGBEP.Field);
            }
            else if (!RoadSegmentAccessRestriction.ByIdentifier.ContainsKey(record.TGBEP.Value))
            {
                problems += recordContext.RoadSegmentAccessRestrictionMismatch(record.TGBEP.Value);
            }

            if (record.LSTRNMID.Value.HasValue && !CrabStreetnameId.Accepts(record.LSTRNMID.Value.Value))
            {
                problems += recordContext.LeftStreetNameIdOutOfRange(record.LSTRNMID.Value.Value);
            }

            if (record.RSTRNMID.Value.HasValue && !CrabStreetnameId.Accepts(record.RSTRNMID.Value.Value))
            {
                problems += recordContext.RightStreetNameIdOutOfRange(record.RSTRNMID.Value.Value);
            }

            if (string.IsNullOrEmpty(record.BEHEERDER.Value))
            {
                problems += recordContext.RequiredFieldIsNull(record.BEHEERDER.Field);
            }

            if (record.METHODE.HasValue && RoadSegmentGeometryDrawMethod.ByIdentifier.TryGetValue(record.METHODE.Value, out var geometryDrawMethod) && geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                problems += ValidateRoadSegmentOutlined(recordContext, record);
            }
            else
            {
                problems += ValidateRoadSegmentMeasured(recordContext, record);
            }

            return problems;
        }

        private static ZipArchiveProblems ValidateRoadSegmentMeasured<T>(IDbaseFileRecordProblemBuilder recordContext, T record)
            where T : IRoadSegmentDbaseRecord
        {
            var problems = ZipArchiveProblems.None;

            if (!record.STATUS.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.STATUS.Field);
            }
            else if (!RoadSegmentStatus.ByIdentifier.ContainsKey(record.STATUS.Value))
            {
                problems += recordContext.RoadSegmentStatusMismatch(record.STATUS.Value);
            }

            if (!record.MORFOLOGIE.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.MORFOLOGIE.Field);
            }
            else if (!RoadSegmentMorphology.ByIdentifier.ContainsKey(record.MORFOLOGIE.Value))
            {
                problems += recordContext.RoadSegmentMorphologyMismatch(record.MORFOLOGIE.Value);
            }

            if (!record.B_WK_OIDN.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.B_WK_OIDN.Field);
            }
            else if (!RoadNodeId.Accepts(record.B_WK_OIDN.Value))
            {
                problems += recordContext.BeginRoadNodeIdOutOfRange(record.B_WK_OIDN.Value);
            }

            if (!record.E_WK_OIDN.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.E_WK_OIDN.Field);
            }
            else if (!RoadNodeId.Accepts(record.E_WK_OIDN.Value))
            {
                problems += recordContext.EndRoadNodeIdOutOfRange(record.E_WK_OIDN.Value);
            }

            if (record.B_WK_OIDN.HasValue && record.E_WK_OIDN.HasValue && record.B_WK_OIDN.Value.Equals(record.E_WK_OIDN.Value))
            {
                problems += recordContext.BeginRoadNodeIdEqualsEndRoadNode(record.B_WK_OIDN.Value, record.E_WK_OIDN.Value);
            }

            return problems;
        }

        private static ZipArchiveProblems ValidateRoadSegmentOutlined<T>(IDbaseFileRecordProblemBuilder recordContext, T record)
            where T : IRoadSegmentDbaseRecord
        {
            var problems = ZipArchiveProblems.None;

            if (!record.STATUS.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.STATUS.Field);
            }
            else if (!RoadSegmentStatus.ByIdentifier.TryGetValue(record.STATUS.Value, out var status) || !status.IsValidForEdit())
            {
                problems += recordContext.RoadSegmentStatusMismatch(record.STATUS.Value, true);
            }

            if (!record.MORFOLOGIE.HasValue)
            {
                problems += recordContext.RequiredFieldIsNull(record.MORFOLOGIE.Field);
            }
            else if (!RoadSegmentMorphology.ByIdentifier.TryGetValue(record.MORFOLOGIE.Value, out var morphology) || !morphology.IsValidForEdit())
            {
                problems += recordContext.RoadSegmentMorphologyMismatch(record.MORFOLOGIE.Value, true);
            }

            if (record.B_WK_OIDN.HasValue && !record.B_WK_OIDN.Value.IsValidStartRoadNodeIdForRoadSegmentOutline())
            {
                problems += recordContext.BeginRoadNodeIdOutOfRange(record.B_WK_OIDN.Value);
            }

            if (record.E_WK_OIDN.HasValue && !record.E_WK_OIDN.Value.IsValidEndRoadNodeIdForRoadSegmentOutline())
            {
                problems += recordContext.EndRoadNodeIdOutOfRange(record.E_WK_OIDN.Value);
            }

            return problems;
        }
    }
}
