/*
============================================================
File: MachineConfiguration.cs

Purpose:
Configures the Machine entity for Entity Framework Core.

Responsibilities:
- Map Machine to the Machines table.
- Configure required fields and maximum lengths.
- Configure unique Machine Code.
- Configure optional Serial Number uniqueness.
- Configure Machine Status.
- Configure indexes used by Machine Master queries.

Important:
- Machine Code is always unique.
- Serial Number is optional.
- Multiple Machines may have the same Manufacturer / Model /
  Machine Type.
- Production relationships will be introduced later.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class MachineConfiguration
        : IEntityTypeConfiguration<Machine>
    {
        public void Configure(
            EntityTypeBuilder<Machine> builder)
        {
            #region Table

            builder.ToTable(
                "Machines");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Machine Code

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

            #endregion


            #region Machine Information

            builder.Property(x =>
                    x.MachineName)
                .IsRequired()
                .HasMaxLength(200);


            builder.Property(x =>
                    x.MachineType)
                .IsRequired()
                .HasMaxLength(100);

            #endregion


            #region Manufacturer Information

            builder.Property(x =>
                    x.Manufacturer)
                .HasMaxLength(150);


            builder.Property(x =>
                    x.Model)
                .HasMaxLength(150);


            builder.Property(x =>
                    x.SerialNumber)
                .HasMaxLength(100);


            builder.HasIndex(x =>
                    x.SerialNumber)
                .IsUnique()
                .HasFilter(
                    "[SerialNumber] IS NOT NULL AND [IsDeleted] = 0");

            #endregion


            #region Capacity And Location

            builder.Property(x =>
                    x.Capacity)
                .HasMaxLength(250);


            builder.Property(x =>
                    x.Location)
                .HasMaxLength(150);

            #endregion


            #region Operational Status

            builder.Property(x =>
                    x.Status)
                .IsRequired();

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.MachineName);


            builder.HasIndex(x =>
                x.MachineType);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                x.IsActive);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}