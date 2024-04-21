using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CarrotSystem.Models.PO;

namespace CarrotSystem.Models.Context
{
    public partial class POContext : DbContext
    {
        public POContext()
        {
        }

        public POContext(DbContextOptions<POContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Poform> Poforms { get; set; } = null!;
        public virtual DbSet<PoformFile> PoformFiles { get; set; } = null!;
        public virtual DbSet<PoformLog> PoformLogs { get; set; } = null!;
        public virtual DbSet<PoformTemplate> PoformTemplates { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=MercProductionService;Integrated Security=True");
                //optionsBuilder.UseSqlServer("Server=103.28.51.32;Database=MercProductionService;user id=mpsDBadmin;pwd=t5xiwD2rcjg?02BVv;TrustServerCertificate=True;MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Poform>(entity =>
            {
                entity.HasKey(e => e.FormId)
                    .HasName("PK_POForm");

                entity.ToTable("POForms");

                entity.Property(e => e.FormId).HasColumnName("FormID");

                entity.Property(e => e.CheckedBy).HasMaxLength(20);

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.DateChecked).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateVerified).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.IssuedBy).HasMaxLength(50);

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.Ponumber)
                    .HasMaxLength(50)
                    .HasColumnName("PONumber");

                entity.Property(e => e.Price).HasColumnType("decimal(16, 2)");

                entity.Property(e => e.RequiredBy).HasMaxLength(50);

                entity.Property(e => e.SentBy).HasMaxLength(50);

                entity.Property(e => e.SentOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.Supplier).HasMaxLength(50);

                entity.Property(e => e.VerificationBy).HasMaxLength(20);
            });

            modelBuilder.Entity<PoformFile>(entity =>
            {
                entity.HasKey(e => e.Aid);

                entity.ToTable("POFormFiles");

                entity.Property(e => e.Aid).HasColumnName("AId");

                entity.Property(e => e.AttachedFileAddress).HasMaxLength(2000);

                entity.Property(e => e.AttachedFileName).HasMaxLength(500);

                entity.Property(e => e.DateUploaded).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PoformLog>(entity =>
            {
                entity.HasKey(e => e.Pk)
                    .HasName("PK_POFormsLog");

                entity.ToTable("POFormLogs");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TimeLabelBgColor).HasMaxLength(20);

                entity.Property(e => e.TimeLabelBody).HasMaxLength(500);

                entity.Property(e => e.TimeLabelHeader)
                    .HasMaxLength(100)
                    .HasColumnName("TImeLabelHeader");
            });

            modelBuilder.Entity<PoformTemplate>(entity =>
            {
                entity.HasKey(e => e.FormId);

                entity.ToTable("POFormTemplates");

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.IsActivated)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IssuedBy).HasMaxLength(50);

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(16, 2)");

                entity.Property(e => e.RequiredBy).HasMaxLength(50);

                entity.Property(e => e.Supplier).HasMaxLength(50);

                entity.Property(e => e.TemplateName).HasMaxLength(100);

                entity.Property(e => e.TemplateOrder).HasDefaultValueSql("((1))");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
