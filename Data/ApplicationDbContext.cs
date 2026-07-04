using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using medicalapp.Models;

namespace medicalapp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalReportRequest> MedicalReportRequests { get; set; } // NEW
        public DbSet<MedicalRecord> MedicalRecords { get; set; } 
        public DbSet<Invoice> Invoices { get; set; }
                public DbSet<Referral> Referrals { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Fix cascade delete conflicts ---

            // Appointment - Patient
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment - Doctor
            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prescription - Patient
            builder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pat => pat.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription - Doctor
            builder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(doc => doc.Prescriptions)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription - Appointment
            builder.Entity<Prescription>()
                .HasOne(p => p.Appointment)
                .WithMany(a => a.Prescriptions)
                .HasForeignKey(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
            // =============================================
            // NEW: Referral cascade fixes
            // =============================================
            // Referral - FromDoctor
            builder.Entity<Referral>()
                .HasOne(r => r.FromDoctor)
                .WithMany()
                .HasForeignKey(r => r.FromDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Referral - ToDoctor
            builder.Entity<Referral>()
                .HasOne(r => r.ToDoctor)
                .WithMany()
                .HasForeignKey(r => r.ToDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Referral - Patient
            builder.Entity<Referral>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Referrals)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);


            // Seed roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Patient", NormalizedName = "PATIENT" },
                new IdentityRole { Id = "2", Name = "Doctor", NormalizedName = "DOCTOR" },
                new IdentityRole { Id = "3", Name = "Receptionist", NormalizedName = "RECEPTIONIST" },
                new IdentityRole { Id = "4", Name = "Admin", NormalizedName = "ADMIN" }
            );

            builder.Entity<Invoice>()
                .HasOne(i => i.Appointment)
                .WithOne(a => a.Invoice)
                .HasForeignKey<Invoice>(i => i.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}