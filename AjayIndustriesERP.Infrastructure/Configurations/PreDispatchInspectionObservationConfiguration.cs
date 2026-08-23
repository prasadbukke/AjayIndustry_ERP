/*
============================================================
File: PreDispatchInspectionObservationConfiguration.cs

Purpose:
Configures PreDispatchInspectionObservation entity for EF Core.

Responsibilities:
- Configure primary key.
- Configure parent Inspection Line relationship.
- Configure observation sequence.
- Configure interval-reading flag.
- Configure observation value.
- Prevent duplicate sequence within the same reading group.

Important:
- Normal Observations and Interval Readings share the same table.
- Sequence Number is unique within:
  Inspection Line + Reading Type.
- Therefore:
  Observation 1 and Interval Reading 1 can both exist.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PreDispatchInspectionObservationConfiguration
        : IEntityTypeConfiguration<
            PreDispatchInspectionObservation>
    {
        public void Configure(
            EntityTypeBuilder<
                PreDispatchInspectionObservation> builder)
        {
            #region Table

            builder.ToTable(
                "PreDispatchInspectionObservations");

            #endregion


            #region Primary Key

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Parent Inspection Line

            builder.HasOne(
                    x => x.PreDispatchInspectionLine)
                .WithMany(
                    x => x.Observations)
                .HasForeignKey(
                    x => x.PreDispatchInspectionLineId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Sequence

            builder.Property(
                    x => x.SequenceNumber)
                .IsRequired();


            builder.Property(
                    x => x.IsIntervalReading)
                .IsRequired();


            builder.HasIndex(
                    x => new
                    {
                        x.PreDispatchInspectionLineId,
                        x.IsIntervalReading,
                        x.SequenceNumber
                    })
                .IsUnique();

            #endregion


            #region Observation Value

            builder.Property(
                    x => x.Value)
                .HasMaxLength(250);

            #endregion
        }
    }
}