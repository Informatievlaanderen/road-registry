using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.StreetNameConsumer.Schema.TypeConfigurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public class StreetNameConsumerItemEntityTypeConfiguration: IEntityTypeConfiguration<StreetNameConsumerItem>
    {
        public void Configure(EntityTypeBuilder<StreetNameConsumerItem> builder)
        {
            builder.Property<string>("StreetNameId")
                .HasColumnType("nvarchar(450)")
                .IsRequired();

            builder.Property<string>("DutchHomonymAddition")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("DutchName")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("DutchNameWithHomonymAddition")
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .HasComputedColumnSql("COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED");

            builder.Property<string>("EnglishHomonymAddition")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("EnglishName")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("EnglishNameWithHomonymAddition")
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .HasComputedColumnSql("COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED");

            builder.Property<string>("FrenchHomonymAddition")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("FrenchName")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("FrenchNameWithHomonymAddition")
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .HasComputedColumnSql("COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED");

            builder.Property<string>("GermanHomonymAddition")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("GermanName")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("GermanNameWithHomonymAddition")
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .HasComputedColumnSql("COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED");

            builder.Property<string>("HomonymAddition")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("MunicipalityId")
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property<string>("Name")
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property<string>("NameWithHomonymAddition")
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("nvarchar(max)")
                .IsRequired(false)
                .HasComputedColumnSql("COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED");

            builder.Property<string>("NisCode")
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property<int?>("PersistentLocalId")
                .HasColumnType("int");

            builder.Property<StreetNameStatus?>("StreetNameStatus")
                .HasColumnType("int")
                .IsRequired(false)
                .HasConversion(new EnumToNumberConverter<StreetNameStatus, int>());

            builder.HasKey("StreetNameId");

            builder.HasIndex("PersistentLocalId");

            builder.HasIndex("StreetNameId");

            SqlServerIndexBuilderExtensions.IsClustered(builder.HasIndex("StreetNameId"), false);

            builder.ToTable("StreetName", "RoadRegistryStreetNameConsumer");
        }
    }
}
