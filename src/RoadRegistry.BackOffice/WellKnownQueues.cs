namespace RoadRegistry.BackOffice;

public static class WellKnownQueues
{
    public const string AdminQueue = "Admin";
    public const string BackOfficeQueue = "BackOffice";
    public const string SnapshotQueue = "Snapshot";

    public const string EventQueue = "roadnetworkarchive-event-queue";
    public const string ExtractQueue = "roadnetworkextract-event-queue";
    public const string CommandQueue = "roadnetwork-command-queue";
    public const string ExtractCommandQueue = "roadnetworkextract-command-queue";
    public const string OrganizationCommandQueue = "organization-command-queue";
}
