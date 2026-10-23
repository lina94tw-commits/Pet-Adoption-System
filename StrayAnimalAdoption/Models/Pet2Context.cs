using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StrayAnimalAdoption.Models;

public partial class Pet2Context : DbContext
{
    public Pet2Context()
    {
    }

    public Pet2Context(DbContextOptions<Pet2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AdoptionApplication> AdoptionApplications { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<HealthRecord> HealthRecords { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
       
        => optionsBuilder.UseSqlServer("Server=5CG5123H54\\SQLEXPRESS;Database=Pet2;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdoptionApplication>(entity =>
        {
            entity.HasKey(e => e.Aid).HasName("PK__Adoption__C69007C89C2D3B05");

            entity.ToTable("Adoption_Application");

            entity.Property(e => e.Aid)
              
                .HasColumnName("AID");
            entity.Property(e => e.Adate).HasColumnName("ADate");
            entity.Property(e => e.Astatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Mid).HasColumnName("MID");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.AdoptionApplications)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK__Adoption_Ap__MID__3B75D760");

            entity.HasMany(d => d.Eids).WithMany(p => p.Aids)
                .UsingEntity<Dictionary<string, object>>(
                    "Review",
                    r => r.HasOne<Employee>().WithMany()
                        .HasForeignKey("Eid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Review__EID__4316F928"),
                    l => l.HasOne<AdoptionApplication>().WithMany()
                        .HasForeignKey("Aid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Review__AID__4222D4EF"),
                    j =>
                    {
                        j.HasKey("Aid", "Eid").HasName("PK__Review__6A8906B8C3CA5CF9");
                        j.ToTable("Review");
                        j.IndexerProperty<int>("Aid").HasColumnName("AID");
                        j.IndexerProperty<int>("Eid").HasColumnName("EID");
                    });
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Eid).HasName("PK__Employee__C190170BFB3E43D1");

            entity.ToTable("Employee");

            entity.Property(e => e.Eid)
                .ValueGeneratedNever()
                .HasColumnName("EID");
            entity.Property(e => e.Ename)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EName");
        });

        modelBuilder.Entity<HealthRecord>(entity =>
        {
            entity.HasKey(e => new { e.Pid, e.Rid }).HasName("PK__HealthRe__F9D8A13386860DB3");

            entity.ToTable("HealthRecord");

            entity.Property(e => e.Pid).HasColumnName("PID");
            entity.Property(e => e.Rid).HasColumnName("RID");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Tdate).HasColumnName("TDate");
            entity.Property(e => e.Ttype)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TType");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.HealthRecords)
                .HasForeignKey(d => d.Pid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HealthRecor__PID__45F365D3");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Mid).HasName("PK__Member__C797348A16DB4AD9");

            entity.ToTable("Member");

            entity.Property(e => e.Mid)
             
                .HasColumnName("MID");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Mname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MName");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PK__Pet__C577552068098D27");

            entity.ToTable("Pet");

            entity.Property(e => e.Pid)
         
                .HasColumnName("PID");
            entity.Property(e => e.Adoption)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Aid).HasColumnName("AID");
            entity.Property(e => e.Breed)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.Pname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PName");
            entity.Property(e => e.Species)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.AidNavigation).WithMany(p => p.Pets)
                .HasForeignKey(d => d.Aid)
                .HasConstraintName("FK__Pet__AID__3E52440B");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Pets)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK__Pet__MID__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
