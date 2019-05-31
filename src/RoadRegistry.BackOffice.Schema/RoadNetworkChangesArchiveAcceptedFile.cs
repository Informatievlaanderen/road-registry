namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChangesArchiveAcceptedFile
    {
        public string File { get; set; }

        public string[] Problems
        {
            get => AllProblems.Split('|');
            set => AllProblems = string.Join("|", value);
        }

        public string AllProblems { get; set; }
    }

//    public class RoadNetworkChangesArchiveAcceptedFileConfiguration : IEntityTypeConfiguration<RoadNetworkChangesArchiveAcceptedFile>
//    {
//        public void Configure(EntityTypeBuilder<RoadNetworkChangesArchiveAcceptedFile> b)
//        {
//            b.Property(p => p.Id).ValueGeneratedOnAdd().UseSqlServerIdentityColumn();
//            b.Property(p => p.File);
//            b.Property(p => p.Problems)
//                .HasConversion(
//                    v => v == null ? null : string.Join("|", v),
//                    v => v == null ? null : v.Split('|'));
//        }
//    }
}
