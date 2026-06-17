using Microsoft.EntityFrameworkCore;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RoleMaster> RoleMasters { get; set; }
        public DbSet<UserMaster> UserMasters { get; set; }
        public DbSet<VisitorMaster> VisitorMasters { get; set; }
        public DbSet<EmployeeMaster> EmployeeMasters { get; set; }
        public DbSet<DepartmentMaster> DepartmentMasters { get; set; }
        public DbSet<AppointmentMaster> AppointmentMasters { get; set; }
        public DbSet<VisitEntryMaster> VisitEntryMasters { get; set; }
        public DbSet<GatePassMaster> GatePassMasters { get; set; }
        public DbSet<ModuleMaster> ModuleMasters { get; set; }
        public DbSet<RoleRight> RoleRights { get; set; }
        public DbSet<EntryRequestMaster> EntryRequestMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure mapping to verify matching table and column case-sensitivity
            modelBuilder.Entity<RoleMaster>(entity =>
            {
                entity.ToTable("RoleMaster");
                entity.HasKey(e => e.RoleId);
                
                // One-to-Many relationship configuration
                entity.HasMany(e => e.Users)
                      .WithOne(e => e.Role)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent role deletion if users are assigned
            });

            modelBuilder.Entity<UserMaster>(entity =>
            {
                entity.ToTable("UserMaster");
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.Employee)
                      .WithOne()
                      .HasForeignKey<UserMaster>(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.EmployeeId)
                      .IsUnique();
            });

            modelBuilder.Entity<VisitorMaster>(entity =>
            {
                entity.ToTable("VisitorMaster");
                entity.HasKey(e => e.VisitorId);
            });

            modelBuilder.Entity<EmployeeMaster>(entity =>
            {
                entity.ToTable("EmployeeMaster");
                entity.HasKey(e => e.EmployeeId);
            });

            modelBuilder.Entity<DepartmentMaster>(entity =>
            {
                entity.ToTable("DepartmentMaster");
                entity.HasKey(e => e.DepartmentId);

                // Configure relationship with EmployeeMaster (1-to-many, restrict delete)
                entity.HasMany(e => e.Employees)
                      .WithOne(e => e.Department!)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AppointmentMaster>(entity =>
            {
                entity.ToTable("AppointmentMaster");
                entity.HasKey(e => e.AppointmentId);

                entity.HasOne(e => e.Visitor)
                      .WithMany()
                      .HasForeignKey(e => e.VisitorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VisitEntryMaster>(entity =>
            {
                entity.ToTable("VisitEntryMaster");
                entity.HasKey(e => e.VisitEntryId);

                entity.HasOne(e => e.Appointment)
                      .WithMany()
                      .HasForeignKey(e => e.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Visitor)
                      .WithMany()
                      .HasForeignKey(e => e.VisitorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ModuleMaster>(entity =>
            {
                entity.ToTable("ModuleMaster");
                entity.HasKey(e => e.ModuleId);
            });

            modelBuilder.Entity<RoleRight>(entity =>
            {
                entity.ToTable("RoleRights");
                entity.HasKey(e => e.RoleRightId);

                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Module)
                      .WithMany()
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.RoleId, e.ModuleId })
                      .IsUnique();
            });

            modelBuilder.Entity<EntryRequestMaster>(entity =>
            {
                entity.ToTable("EntryRequestMaster");
                entity.HasKey(e => e.EntryRequestId);

                entity.HasOne(e => e.Visitor)
                      .WithMany()
                      .HasForeignKey(e => e.VisitorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByEmployee)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedByEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Global Value Converter for UTC DateTime mapping to PostgreSQL timestamp with time zone (timestamptz)
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.Name == "AppointmentDate")
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }
}
