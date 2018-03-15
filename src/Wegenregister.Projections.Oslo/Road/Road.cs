namespace Wegenregister.Projections.Oslo.Road
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ValueObjects;

    public class Road
    {
        public Guid? RoadId { get; set; }

        public Language? PrimaryLanguage { get; set; }
        public Language? SecondaryLanguage { get; set; }

        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }
    }

    public class RoadConfiguration : IEntityTypeConfiguration<Road>
    {
        private const string TableName = "Roads";

        public void Configure(EntityTypeBuilder<Road> b)
        {
            b.ToTable(TableName, Schema.Oslo)
                .HasKey(p => p.RoadId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.PrimaryLanguage);
            b.Property(p => p.SecondaryLanguage);

            b.Property(p => p.NameDutch);
            b.Property(p => p.NameFrench);
            b.Property(p => p.NameGerman);
            b.Property(p => p.NameEnglish);

            b.HasIndex(p => p.NameDutch).ForSqlServerIsClustered();
        }
    }
}
