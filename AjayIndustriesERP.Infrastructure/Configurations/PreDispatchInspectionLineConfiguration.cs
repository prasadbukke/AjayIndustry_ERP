/*
============================================================
File: PreDispatchInspectionLineConfiguration.cs

Purpose:
Configures PreDispatchInspectionLine entity for EF Core.

Responsibilities:
- Configure primary key.
- Configure parent PDI relationship.
- Configure line sequence.
- Configure inspection parameter fields.
- Configure result and remarks.
- Configure child Observation relationship.
- Prevent duplicate Sequence Number within one PDI.

Important:
- Inspection Parameter and Specification are snapshots.
- One PDI can contain multiple Inspection Lines.
- One Inspection Line can contain multiple Observations.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PreDispatchInspectionLineConfiguration
        : IEntityTypeConfiguration<PreDispatchInspectionLine>
    {
        public void Configure(
            EntityTypeBuilder<PreDispatchInspectionLine> builder)
        {
            #region Table

            builder.ToTable(
                "PreDispatchInspectionLines");

            #endregion


            #region Primary Key

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Parent PDI

            builder.HasOne(
                    x => x.PreDispatchInspection)
                .WithMany(
                    x => x.Lines)
                .HasForeignKey(
                    x => x.PreDispatchInspectionId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Sequence

            builder.Property(
                    x => x.SequenceNumber)
                .IsRequired();


            builder.HasIndex(
                    x => new
                    {
                        x.PreDispatchInspectionId,
                        x.SequenceNumber
                    })
                .IsUnique();

            #endregion


            #region Inspection Parameter

            builder.Property(
                    x => x.Parameter)
                .IsRequired()
                .HasMaxLength(250);


            builder.Property(
                    x => x.Specification)
                .IsRequired()
                .HasMaxLength(500);

            #endregion


            #region Inspection Method

            builder.Property(
                    x => x.InspectionMethod)
                .HasMaxLength(250);

            #endregion


            #region Result

            builder.Property(
                    x => x.Result)
                .IsRequired();


            builder.Property(
                    x => x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Observations

            builder.HasMany(
                    x => x.Observations)
                .WithOne(
                    x => x.PreDispatchInspectionLine)
                .HasForeignKey(
                    x => x.PreDispatchInspectionLineId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion
        }
    }
}