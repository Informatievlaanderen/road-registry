namespace Wegenregister.Projections.Oslo.RoadList
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ValueObjects;

    public class RoadListItem
    {
        public Guid? RoadId { get; set; }

        public Language? PrimaryLanguage { get; set; }
        public Language? SecondaryLanguage { get; set; }

        public string DefaultName { get; set; }
        public string NameDutch { get; set; }
        public string NameFrench { get; set; }
        public string NameGerman { get; set; }
        public string NameEnglish { get; set; }
    }

    public class RoadListConfiguration : IEntityTypeConfiguration<RoadListItem>
    {
        private const string TableName = "RoadList";

        public void Configure(EntityTypeBuilder<RoadListItem> b)
        {
            b.ToTable(TableName, Schema.Oslo)
                .HasKey(p => p.RoadId)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.PrimaryLanguage);
            b.Property(p => p.SecondaryLanguage);

            b.Property(p => p.DefaultName);
            b.Property(p => p.NameDutch);
            b.Property(p => p.NameFrench);
            b.Property(p => p.NameGerman);
            b.Property(p => p.NameEnglish);

            b.HasIndex(p => p.DefaultName).ForSqlServerIsClustered();
        }
    }
}
