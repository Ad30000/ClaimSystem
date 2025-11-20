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

    public virtual DbSet<ClaimApproval> ClaimApprovals { get; set; }

    public virtual DbSet<HR> HR { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=lab7L95SR\\SQLEXPRESS;Initial Catalog=ClaimDatabase;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.ClaimId);

            entity.ToTable("Claim");

            entity.Property(e => e.AdditionalNotes).HasMaxLength(500);
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
                .HasComputedColumnSql("([HoursWorked]*[AmountPerHour])", false)
                .HasColumnType("decimal(21, 4)");
            entity.Property(e => e.ClaimNumber).HasMaxLength(20);
            entity.Property(e => e.LastUpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Lecturer)
                .WithMany(p => p.Claims)
                .HasForeignKey(d => d.LecturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claim_Lecturer");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.LecturerId);

            entity.ToTable("Lecturer");

            entity.HasIndex(e => e.UserId, "UQ_Lecturer_UserId").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);

            entity.HasOne(d => d.User)
                .WithOne(p => p.Lecturer)
                .HasForeignKey<Lecturer>(d => d.UserId)
                .HasConstraintName("FK_Lecturer_User");
        });

        modelBuilder.Entity<SupportingDocument>(entity =>
        {
            entity.HasKey(e => e.SupportingDocumentId);

            entity.ToTable("SupportingDocument");

            entity.Property(e => e.ContentType)
                .HasMaxLength(100)
                .HasDefaultValue("application/octet-stream");
            entity.Property(e => e.FileContent).HasColumnType("varbinary(max)");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileSize).HasColumnType("bigint");
            entity.Property(e => e.FileUploadDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Claim)
                .WithMany(p => p.SupportingDocuments)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK_SupportingDocument_Claim");
        });

        modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("User");

                entity.HasIndex(e => e.Username, "UQ_User_Username").IsUnique();
                entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(50);
            });



        modelBuilder.Entity<ClaimApproval>(entity =>
        {
            entity.HasKey(e => e.ClaimApprovalId);

            entity.ToTable("ClaimApproval");

            entity.Property(e => e.ApprovalDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Role).HasMaxLength(30);
            entity.Property(e => e.Action).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.Comments).HasMaxLength(500);

            entity.HasOne(d => d.Claim)
                .WithMany(p => p.ClaimApprovals)
                .HasForeignKey(d => d.ClaimId)
                .HasConstraintName("FK_ClaimApproval_Claim");

            entity.HasOne(d => d.Approver)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ClaimApproval_User");
        });

        modelBuilder.Entity<HR>(entity =>
        {
            entity.HasKey(e => e.HRId);

            entity.ToTable("HR");

            entity.HasIndex(e => e.UserId, "UQ_HR_UserId").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // ADD THIS FOR THE NEW PROPERTY
            entity.Property(e => e.DefaultHourlyRate)
                .HasColumnType("decimal(10, 2)")
                .HasDefaultValue(0);

            entity.HasOne(d => d.User)
                .WithOne(p => p.HR)
                .HasForeignKey<HR>(d => d.UserId)
                .HasConstraintName("FK_HR_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
