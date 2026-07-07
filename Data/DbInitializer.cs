using medicalapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace medicalapp.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed roles if they don't exist
            string[] roleNames = { "Admin", "Doctor", "Receptionist", "Patient" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Ensure all existing seeded users have EmailConfirmed = true to support local testing transition
            var unconfirmedUsers = await context.Users.Where(u => !u.EmailConfirmed).ToListAsync();
            if (unconfirmedUsers.Any())
            {
                foreach (var u in unconfirmedUsers)
                {
                    u.EmailConfirmed = true;
                }
                await context.SaveChangesAsync();
            }

            // Seed default users
            await SeedUser(userManager, roleManager, "admin@medicloud.com", "Admin123!", "Admin", "User", new[] { "Admin" });
            await SeedUser(userManager, roleManager, "doctor@medicloud.com", "Doctor123!", "Dr.", "Siti", new[] { "Doctor" });
            await SeedUser(userManager, roleManager, "patient@medicloud.com", "Patient123!", "Patient", "User", new[] { "Patient" });

            var receptionistEmail = "receptionist@medicloud.com";
            var receptionistUser = await userManager.FindByEmailAsync(receptionistEmail);
            if (receptionistUser == null)
            {
                receptionistUser = new ApplicationUser
                {
                    UserName = receptionistEmail,
                    Email = receptionistEmail,
                    FirstName = "Receptionist",
                    LastName = "User",
                    ICNumber = "000101-10-5678",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(receptionistUser, "Receptionist123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(receptionistUser, "Receptionist");
                }
            }


            // Seed Doctor profile for doctor@medicloud.com
            var doctorUser = await userManager.FindByEmailAsync("doctor@medicloud.com");
            if (doctorUser != null)
            {
                var existingDoctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);
                if (existingDoctor == null)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = doctorUser.Id,
                        Specialization = "Cardiology",
                        Department = "Cardiology Department",
                        LicenseNumber = "MED-2025-001",
                        LicenseDocumentUrl = null,
                        IsVerified = true,
                        VerifiedAt = DateTime.Now,
                        ConsultationFee = 150.00m,
                        YearsOfExperience = 12,
                        Bio = "Experienced cardiologist with expertise in heart disease prevention and treatment.",
                        Qualifications = "MD, MBBS, Fellowship in Cardiology",
                        ClinicName = "MediCloud Hospital",
                        ClinicAddress = "123 Jalan Medik, 47100 Puchong",
                        ClinicPhone = "03-1234 5678"
                    });
                }
            }

            // Seed Patient profile for patient@medicloud.com
            var patientUser = await userManager.FindByEmailAsync("patient@medicloud.com");
            if (patientUser != null)
            {
                var existingPatient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == patientUser.Id);
                if (existingPatient == null)
                {
                    context.Patients.Add(new Patient
                    {
                        UserId = patientUser.Id,
                        BloodType = "O+",
                        Allergies = "None",
                        ChronicConditions = "Hypertension",
                        CurrentMedications = "Amlodipine 5mg daily",
                        EmergencyContactName = "Emergency Contact",
                        EmergencyContactPhone = "012-3456789",
                        EmergencyContactRelationship = "Spouse",
                        CreatedAt = DateTime.Now
                    });
                    await context.SaveChangesAsync();
                }
            }

            // Seed a sample schedule for the doctor
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);
            if (doctor != null)
            {
                var existingSchedules = await context.Schedules.AnyAsync(s => s.DoctorId == doctor.Id);
                if (!existingSchedules)
                {
                    var schedules = new[]
                    {
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true },
                        new Schedule { DoctorId = doctor.Id, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDuration = 30, MaxPatientsPerDay = 6, IsAvailable = true }
                    };
                    await context.Schedules.AddRangeAsync(schedules);
                    await context.SaveChangesAsync();
                }
            }

            var doctorData = new List<(string email, string firstName, string lastName, string specialization, string department, decimal fee, int experience)>
            {
                ("doctor.cardiologist@medicloud.com", "Dr.", "Aminah", "Cardiology", "Cardiology Department", 180.00m, 15),
                ("doctor.pediatrician@medicloud.com", "Dr.", "Ravi", "Pediatrics", "Pediatrics Department", 130.00m, 10),
                ("doctor.orthopedic@medicloud.com", "Dr.", "Siti", "Orthopedics", "Orthopedics Department", 160.00m, 12),
                ("doctor.neurologist@medicloud.com", "Dr.", "Ahmad", "Neurology", "Neurology Department", 190.00m, 18),
                ("doctor.dermatologist@medicloud.com", "Dr.", "Mei Ling", "Dermatology", "Dermatology Department", 140.00m, 8),
                ("doctor.ophthalmologist@medicloud.com", "Dr.", "Tan", "Ophthalmology", "Ophthalmology Department", 150.00m, 11),
                ("doctor.ent@medicloud.com", "Dr.", "Kumar", "ENT", "ENT Department", 135.00m, 9),
                ("doctor.psychiatrist@medicloud.com", "Dr.", "Chong", "Psychiatry", "Psychiatry Department", 170.00m, 14)
            };

            foreach (var doc in doctorData)
            {
                var existingUser = await userManager.FindByEmailAsync(doc.email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = doc.email,
                        Email = doc.email,
                        FirstName = doc.firstName,
                        LastName = doc.lastName,
                        ICNumber = "000101-10-5678",
                        Gender = "Male",
                        DateOfBirth = new DateTime(1975, 1, 1),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, "Doctor123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Doctor");
                        context.Doctors.Add(new Doctor
                        {
                            UserId = user.Id,
                            Specialization = doc.specialization,
                            Department = doc.department,
                            LicenseNumber = $"MED-{DateTime.Now.Year}-{new Random().Next(100, 999).ToString("D3")}",
                            IsVerified = true,
                            VerifiedAt = DateTime.Now,
                            ConsultationFee = doc.fee,
                            YearsOfExperience = doc.experience,
                            Bio = $"Specializes in {doc.specialization} with {doc.experience} years of experience.",
                            Qualifications = "MD, MBBS",
                            ClinicName = "MediCloud Hospital",
                            ClinicAddress = "123 Jalan Medik, 47100 Puchong",
                            ClinicPhone = "03-1234 5678"
                        });
                    }
                }
            }
            await context.SaveChangesAsync();


            // =============================================
            // SEED 20+ PATIENTS WITH REALISTIC DATA
            // =============================================

            var patientData = new List<(string firstName, string lastName, string ic, string gender, DateTime dob, string bloodType, string allergies, string condition, string phone, string emergencyContact, string emergencyPhone, string emergencyRelation)>
            {
                ("Ahmad", "Abdullah", "800101-10-1234", "Male", new DateTime(1980, 1, 15), "O+", "Penicillin", "Hypertension", "012-3456789", "Siti Abdullah", "019-8765432", "Spouse"),
                ("Siti", "Aminah", "830205-08-5678", "Female", new DateTime(1983, 2, 5), "A+", "None", "None", "013-9876543", "Rahim Aminah", "016-5432109", "Spouse"),
                ("Tan", "Wei Ming", "751112-14-9012", "Male", new DateTime(1975, 11, 12), "B+", "Peanuts", "Diabetes Type 2", "012-5678901", "Tan Mei Ling", "019-4321098", "Spouse"),
                ("Kumar", "Raj", "901231-08-3456", "Male", new DateTime(1990, 12, 31), "O-", "None", "None", "014-6789012", "Saraswati Raj", "017-3210987", "Mother"),
                ("Lee", "Mei Ling", "850617-10-7890", "Female", new DateTime(1985, 6, 17), "AB+", "Shellfish", "None", "012-7890123", "Lee Chong Wei", "019-2109876", "Father"),
                ("Muthu", "Krishnan", "770330-08-1234", "Male", new DateTime(1977, 3, 30), "B-", "None", "Heart Disease", "013-8901234", "Lakshmi Krishnan", "016-1098765", "Spouse"),
                ("Chong", "Wei", "921112-10-5678", "Male", new DateTime(1992, 11, 12), "A-", "Dust", "Asthma", "012-9012345", "Chong Kim Lee", "019-0987654", "Spouse"),
                ("Rina", "Hassan", "880705-14-9012", "Female", new DateTime(1988, 7, 5), "O+", "None", "None", "013-0123456", "Rashid Hassan", "016-9876543", "Spouse"),
                ("James", "Wong", "691115-08-3456", "Male", new DateTime(1969, 11, 15), "A+", "Pollen", "Hyperlipidemia", "012-1234567", "Wong Siew Ling", "019-8765432", "Spouse"),
                ("Nurul", "Izzati", "951212-10-7890", "Female", new DateTime(1995, 12, 12), "B+", "None", "None", "014-2345678", "Nurul Izzah", "017-7654321", "Sister"),
                ("Raj", "Kumar", "820409-08-1234", "Male", new DateTime(1982, 4, 9), "O-", "Penicillin", "None", "012-3456789", "Priya Kumar", "019-6543210", "Spouse"),
                ("Chin", "Mei", "901028-14-5678", "Female", new DateTime(1990, 10, 28), "AB-", "None", "None", "013-4567890", "Chin Teck Seng", "016-5432109", "Father"),
                ("Shanti", "Devi", "770612-08-9012", "Female", new DateTime(1977, 6, 12), "O+", "Shellfish", "Diabetes Type 2", "012-5678901", "Suresh Kumar", "019-4321098", "Spouse"),
                ("Muhammad", "Faiz", "940315-10-3456", "Male", new DateTime(1994, 3, 15), "A+", "None", "None", "014-6789012", "Muhammad Faisal", "017-3210987", "Brother"),
                ("Sandra", "Fernandez", "860529-14-7890", "Female", new DateTime(1986, 5, 29), "B-", "Peanuts", "Hypertension", "012-7890123", "Carlos Fernandez", "019-2109876", "Father"),
                ("Vincent", "Tan", "710827-08-1234", "Male", new DateTime(1971, 8, 27), "A-", "None", "Heart Disease", "013-8901234", "Tan Siok Eng", "016-1098765", "Spouse"),
                ("Elena", "Ivanova", "930506-10-5678", "Female", new DateTime(1993, 5, 6), "O+", "None", "None", "012-9012345", "Dmitri Ivanov", "019-0987654", "Spouse"),
                ("Tengku", "Syahmi", "890111-14-9012", "Male", new DateTime(1989, 1, 11), "AB+", "Dust", "Asthma", "013-0123456", "Tengku Putri", "016-9876543", "Spouse"),
                ("Goh", "Boon Heng", "751119-08-3456", "Male", new DateTime(1975, 11, 19), "B+", "None", "None", "012-1234567", "Goh Ai Leng", "019-8765432", "Spouse"),
                ("Priscilla", "Chan", "900528-10-7890", "Female", new DateTime(1990, 5, 28), "O-", "Penicillin", "None", "014-2345678", "Chan Wai Lun", "017-7654321", "Brother"),
                ("Hafiz", "Rahman", "940731-08-1234", "Male", new DateTime(1994, 7, 31), "A+", "Pollen", "None", "012-3456789", "Aisyah Rahman", "019-6543210", "Spouse"),
                ("Deepa", "Muthu", "850315-14-5678", "Female", new DateTime(1985, 3, 15), "B+", "None", "None", "013-4567890", "Karthik Muthu", "016-5432109", "Spouse")
            };

            foreach (var p in patientData)
            {
                var email = $"{p.firstName.ToLower()}.{p.lastName.ToLower()}@patient.com";
                var existingUser = await userManager.FindByEmailAsync(email);
                
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = p.firstName,
                        LastName = p.lastName,
                        ICNumber = p.ic,
                        Gender = p.gender,
                        DateOfBirth = p.dob,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Patient123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Patient");

                        var patient = new Patient
                        {
                            UserId = user.Id,
                            BloodType = p.bloodType,
                            Allergies = p.allergies,
                            ChronicConditions = p.condition,
                            CurrentMedications = "None",
                            EmergencyContactName = p.emergencyContact,
                            EmergencyContactPhone = p.emergencyPhone,
                            EmergencyContactRelationship = p.emergencyRelation,
                            CreatedAt = DateTime.Now
                        };
                        context.Patients.Add(patient);
                    }
                }
            }

            await context.SaveChangesAsync();


            // =============================================
            // SEED RICH DATA FOR ADMIN ANALYTICS (ONLY ONCE)
            // =============================================

            var existingAppointmentCount = await context.Appointments.CountAsync();
            if (existingAppointmentCount < 10)
            {
                // Get existing patients and doctors
                var existingPatients = await context.Patients.ToListAsync();
                var existingDoctors = await context.Doctors.ToListAsync();

                if (existingPatients.Any() && existingDoctors.Any())
                {
                    var rng = new Random(); // ✅ RENAMED from 'random' to 'rng'
                    var statuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
                    var reasons = new[]
                    {
                        "Routine Checkup", "Chest Pain", "Fever", "Headache", "High Blood Pressure",
                        "Diabetes Monitoring", "Pregnancy Check", "Follow-up", "Vaccination",
                        "Health Screening", "Heart Palpitations", "Shortness of Breath",
                        "Back Pain", "Joint Pain", "Skin Rash", "Eye Examination",
                        "Dental Check", "Ear Infection", "Stomach Pain", "Allergy Test"
                    };
                    var appointmentTypes = new[] { "In-Person", "Telehealth", "Follow-up" };
                    var doctorIds = existingDoctors.Select(d => d.Id).ToList();
                    var patientIds = existingPatients.Select(p => p.Id).ToList();

                    // Generate 50 appointments
                    var appointments = new List<Appointment>();
                    var startDate = DateTime.Now.AddDays(-60);
                    var endDate = DateTime.Now.AddDays(60);

                    for (int i = 0; i < 50; i++)
                    {
                        var patientId = patientIds[rng.Next(patientIds.Count)];
                        var doctorId = doctorIds[rng.Next(doctorIds.Count)];
                        var status = statuses[rng.Next(statuses.Length)];
                        var appointmentDate = startDate.AddDays(rng.Next((endDate - startDate).Days));
                        var startTime = new TimeSpan(rng.Next(8, 17), rng.Next(0, 60), 0);
                        var endTime = startTime.Add(TimeSpan.FromMinutes(30));

                        var selectedDoctor = existingDoctors.First(d => d.Id == doctorId);
                        var fee = selectedDoctor.ConsultationFee;

                        appointments.Add(new Appointment
                        {
                            PatientId = patientId,
                            DoctorId = doctorId,
                            AppointmentDate = appointmentDate,
                            StartTime = startTime,
                            EndTime = endTime,
                            Status = status,
                            Type = appointmentTypes[rng.Next(appointmentTypes.Length)],
                            ReasonForVisit = reasons[rng.Next(reasons.Length)],
                            Symptoms = "Various symptoms",
                            ConsultationFee = fee,
                            TaxAmount = fee * 0.06m,
                            TotalAmount = fee * 1.06m,
                            IsPaid = status == "Completed" || status == "Confirmed",
                            PaymentMethod = rng.Next(0, 2) == 0 ? "Online Banking" : "Credit Card",
                            PaymentReference = $"PAY-{DateTime.Now.Year}-{rng.Next(10000, 99999)}",
                            PaymentDate = status == "Completed" || status == "Confirmed" ? appointmentDate.AddDays(rng.Next(0, 3)) : null,
                            CreatedAt = appointmentDate,
                            CreatedBy = "system"
                        });
                    }

                    await context.Appointments.AddRangeAsync(appointments);
                    await context.SaveChangesAsync();

                    // =============================================
                    // SEED PRESCRIPTIONS
                    // =============================================

                    var medications = new[]
                    {
                        ("Amoxicillin", "500mg", "3 times daily", "7 days"),
                        ("Paracetamol", "500mg", "4 times daily", "5 days"),
                        ("Amlodipine", "5mg", "Once daily", "30 days"),
                        ("Metformin", "850mg", "Twice daily", "30 days"),
                        ("Omeprazole", "20mg", "Once daily", "14 days"),
                        ("Losartan", "50mg", "Once daily", "30 days"),
                        ("Atorvastatin", "20mg", "Once daily", "30 days"),
                        ("Salbutamol", "100mcg", "As needed", "30 days"),
                        ("Cetirizine", "10mg", "Once daily", "14 days"),
                        ("Ibuprofen", "400mg", "3 times daily", "7 days"),
                        ("Diazepam", "2mg", "3 times daily", "14 days"),
                        ("Ciprofloxacin", "500mg", "Twice daily", "7 days"),
                        ("Cough Syrup", "10ml", "3 times daily", "5 days"),
                        ("Vitamin C", "1000mg", "Once daily", "30 days"),
                        ("Vitamin D", "1000IU", "Once daily", "30 days")
                    };

                    var completedAppointments = await context.Appointments
                        .Where(a => a.Status == "Completed" && a.AppointmentDate < DateTime.Now)
                        .Take(25)
                        .ToListAsync();

                    var prescriptions = new List<Prescription>();
                    var prescriptionStatuses = new[] { "Active", "Completed", "Cancelled" };

                    foreach (var appt in completedAppointments)
                    {
                        var patient = await context.Patients.FindAsync(appt.PatientId);
                        if (patient != null)
                        {
                            var numPrescriptions = rng.Next(1, 3);
                            for (int i = 0; i < numPrescriptions; i++)
                            {
                                var med = medications[rng.Next(medications.Length)];
                                var status = prescriptionStatuses[rng.Next(prescriptionStatuses.Length)];
                                
                                prescriptions.Add(new Prescription
                                {
                                    PatientId = appt.PatientId,
                                    DoctorId = appt.DoctorId,
                                    AppointmentId = appt.Id,
                                    MedicationName = med.Item1,
                                    Dosage = med.Item2,
                                    Frequency = med.Item3,
                                    Duration = med.Item4,
                                    Instructions = $"Take as directed. {med.Item3} for {med.Item4}.",
                                    Quantity = rng.Next(10, 60),
                                    IsRefillable = rng.Next(0, 2) == 0,
                                    RefillCount = rng.Next(0, 3),
                                    PrescribedDate = appt.AppointmentDate,
                                    ExpiryDate = appt.AppointmentDate.AddMonths(6),
                                    Status = status
                                });
                            }
                        }
                    }

                    await context.Prescriptions.AddRangeAsync(prescriptions);
                    await context.SaveChangesAsync();

                    // =============================================
                    // SEED MEDICAL RECORDS
                    // =============================================

                    var recordTypes = new[] { "Lab Result", "X-Ray", "Blood Test", "MRI", "ECG", "Ultrasound" };
                    var medicalRecords = new List<MedicalRecord>();

                    foreach (var appt in completedAppointments.Take(15))
                    {
                        var patient = await context.Patients.FindAsync(appt.PatientId);
                        if (patient != null)
                        {
                            medicalRecords.Add(new MedicalRecord
                            {
                                PatientId = appt.PatientId,
                                DoctorId = appt.DoctorId,
                                AppointmentId = appt.Id,
                                RecordType = recordTypes[rng.Next(recordTypes.Length)],
                                Title = $"{recordTypes[rng.Next(recordTypes.Length)]} - {appt.AppointmentDate:dd MMM yyyy}",
                                Description = $"Test results and findings from consultation.",
                                RecordDate = appt.AppointmentDate,
                                UploadedAt = appt.AppointmentDate,
                                UploadedBy = "doctor@medicloud.com",
                                Status = "Active",
                                IsConfidential = false
                            });
                        }
                    }

                    await context.MedicalRecords.AddRangeAsync(medicalRecords);
                    await context.SaveChangesAsync();
                }
            }


            // =============================================
            // SEED INVOICES FOR COMPLETED APPOINTMENTS
            // =============================================

            var completedApptsList = await context.Appointments
                .Where(a => a.Status == "Completed")
                .ToListAsync();

            if (completedApptsList.Any())
            {
                var invoiceCounter = await context.Invoices.CountAsync();
                var newInvoices = new List<Invoice>();

                foreach (var appointment in completedApptsList)
                {
                    var existingInvoice = await context.Invoices
                        .FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id);

                    if (existingInvoice == null)
                    {
                        invoiceCounter++;
                        var invoiceNumber = $"INV-{DateTime.Now.Year}-{invoiceCounter.ToString("D4")}";

                        var docInfo = await context.Doctors
                            .Include(d => d.User)
                            .FirstOrDefaultAsync(d => d.Id == appointment.DoctorId);
                        var docFullName = docInfo != null ? $"Dr. {docInfo.User.FirstName} {docInfo.User.LastName}" : "Unknown Doctor";

                        newInvoices.Add(new Invoice
                        {
                            AppointmentId = appointment.Id,
                            PatientId = appointment.PatientId,
                            InvoiceNumber = invoiceNumber,
                            IssueDate = appointment.AppointmentDate,
                            DueDate = appointment.AppointmentDate.AddDays(14),
                            SubTotal = appointment.ConsultationFee,
                            TaxAmount = appointment.TaxAmount,
                            TotalAmount = appointment.TotalAmount,
                            Status = appointment.IsPaid ? "Paid" : "Unpaid",
                            PaymentDate = appointment.IsPaid ? appointment.AppointmentDate.AddDays(1) : null,
                            PaymentMethod = appointment.IsPaid ? appointment.PaymentMethod : null,
                            PaymentReference = appointment.IsPaid ? appointment.PaymentReference : null,
                            CreatedBy = "system",
                            Notes = $"Consultation with {docFullName}"
                        });
                    }
                }

                if (newInvoices.Any())
                {
                    await context.Invoices.AddRangeAsync(newInvoices);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {newInvoices.Count} invoices seeded successfully!");
                }
            }
            
        }

        private static async Task SeedUser(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            string email,
            string password,
            string firstName,
            string lastName,
            string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    ICNumber = "000101-10-1234",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1980, 1, 1),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    foreach (var role in roles)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }
        }
    }
}