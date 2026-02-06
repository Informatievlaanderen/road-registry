namespace RoadRegistry.Extracts.Uploads;

public static class ProjectionFormatFileProblems
{
    public static FileError ProjectionFormatNotLambert72(this IFileProblemBuilder builder)
    {
        return builder.Error(nameof(ProjectionFormatNotLambert72)).Build();
    }

    public static FileError ProjectionFormatNotLambert08(this IFileProblemBuilder builder)
    {
        return builder.Error(nameof(ProjectionFormatNotLambert08)).Build();
    }
}
