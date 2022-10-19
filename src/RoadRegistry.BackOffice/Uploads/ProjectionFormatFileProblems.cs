namespace RoadRegistry.BackOffice.Uploads;

public static class ProjectionFormatFileProblems
{
    public static FileError ProjectionFormatInvalid(this IFileProblemBuilder builder)
    {
        return builder.ThisError(nameof(ProjectionFormatInvalid)).Build();
    }
}
