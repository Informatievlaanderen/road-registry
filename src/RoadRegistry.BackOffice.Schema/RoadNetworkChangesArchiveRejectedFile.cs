namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveRejectedFile
    {
        public string File { get; set; }
        public string[] Problems { get; set; }
    }

//    public class RoadNetworkChangesArchiveRejectedFileConfiguration : IEntityTypeConfiguration<RoadNetworkChangesArchiveRejectedFile>
//    {
//        public void Configure(EntityTypeBuilder<RoadNetworkChangesArchiveRejectedFile> b)
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
