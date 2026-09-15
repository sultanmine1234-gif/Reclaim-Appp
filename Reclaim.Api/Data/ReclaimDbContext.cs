using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Reclaim.Api.Models;

namespace Reclaim.Api.Data;

public partial class ReclaimDbContext : DbContext
{
    public ReclaimDbContext(DbContextOptions<ReclaimDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Claimant> Claimants { get; set; }

    public virtual DbSet<FoundItem> FoundItems { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<ItemClaim> ItemClaims { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoundItem>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("Available");

            entity.HasOne(d => d.Category).WithMany(p => p.FoundItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FoundItems_ItemCategories");
        });

        modelBuilder.Entity<ItemClaim>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Claimant).WithMany(p => p.ItemClaims)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemClaims_Claimants");

            entity.HasOne(d => d.FoundItem).WithMany(p => p.ItemClaims)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemClaims_FoundItems");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
