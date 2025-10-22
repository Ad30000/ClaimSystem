using System;
using System.Collections.Generic;
using ClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimSystem.DB;

public partial class ClaimDatabaseContext : DbContext
{
    public ClaimDatabaseContext()
    {
    }

    public ClaimDatabaseContext(DbContextOptions<ClaimDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Claim> Claims { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<SupportingDocument> SupportingDocuments { get; set; }

    public virtual DbSet<User> Users { get; set; }

   

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=lab7L95SR\\SQLEXPRESS;Initial Catalog=ClaimDatabase;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.ClaimId).HasName("PK__Claim__EF2E139BC542D98F");

            entity.ToTable("Claim");

            entity.Property(e => e.AdditionalNotes).HasMaxLength(255);
            entity.Property(e => e.AmountPerHour).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DateSubmitted)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HoursWorked).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MonthClaim).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("([HoursWorked]*[AmountPerHour])", true)
                .HasColumnType("decimal(21, 4)");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Claims)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK_Claim_Lecturer");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.LecturerId).HasName("PK__Lecturer__5A78B93D23C3DCC3");

            entity.ToTable("Lecturer");

            entity.HasIndex(e => e.UserId, "UQ__Lecturer__1788CC4DC72B8E1D").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);

            entity.HasOne(d => d.User).WithOne(p => p.Lecturer)
                .HasForeignKey<Lecturer>(d => d.UserId)
                .HasConstraintName("FK_Lecturer_User");
        });

        modelBuilder.Entity<SupportingDocument>(entity =>
        {
            entity.HasKey(e => e.SupportingDocumentId).HasName("PK__Supporti__9DD3C21C647019FF");

            entity.ToTable("SupportingDocument");

            entity.Property(e => e.ContentType)
                .HasMaxLength(100)
                .HasDefaultValue("application/octet-stream");
            entity.Property(e => e.FileContent)
                .IsRequired(false)
              .HasColumnType("varbinary(max)");
            entity.Property(e => e.FileName).HasMaxLength(100);
            entity.Property(e => e.FileUploadDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Claim).WithMany(p => p.SupportingDocuments)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK_SupportingDocument_Claim");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4CB3B45EFA");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E47BB91CFB").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534F9976CCD").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
