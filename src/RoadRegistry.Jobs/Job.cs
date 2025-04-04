namespace RoadRegistry.Jobs
{
    using System;
    using BackOffice.Abstractions.Jobs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class Job
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset LastChanged { get; private set; }
        public JobStatus Status { get; private set; }
        public Guid TicketId { get; set; }
        public UploadType UploadType { get; set; }
        public Guid? DownloadId { get; set; }
        public string? OperatorName { get; set; }

        public string UploadBlobName => $"upload_{Id:D}";
        public string ReceivedBlobName => $"received/{Id:D}";
        private Job() { }

        public Job(DateTimeOffset created, JobStatus status, UploadType uploadType, Guid ticketId)
        {
            Created = created;
            LastChanged = created;
            Status = status;
            UploadType = uploadType;
            TicketId = ticketId;
        }

        public void UpdateStatus(JobStatus status)
        {
            Status = status;
            LastChanged = DateTimeOffset.Now;
        }

        public bool IsExpired(TimeSpan expiration) => Created.Add(expiration) < DateTimeOffset.Now;
    }

    public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder
                .ToTable("Jobs", JobsContext.Schema)
                .HasKey(x => x.Id);

            builder
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Created);
            builder.Property(x => x.LastChanged);
            builder.Property(x => x.Status);
            builder.Property(x => x.TicketId);
            builder.Property(x => x.OperatorName).HasMaxLength(20);

            builder.HasIndex(x => x.Status);
        }
    }
}
