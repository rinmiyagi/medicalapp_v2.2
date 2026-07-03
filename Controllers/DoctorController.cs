using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.Models;
using medicalapp.Models.ViewModels;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctor Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var today = DateTime.Today;
            var viewModel = new DoctorDashboardViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = $"Dr. {user.FirstName} {user.LastName}",
                Specialization = doctor.Specialization,
                TodayAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .ToListAsync(),
                PendingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Where(a => a.DoctorId == doctor.Id && a.Status == "Pending")
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync(),
                TotalPatients = await _context.Appointments
                    .Where(a => a.DoctorId == doctor.Id && a.Status == "Completed")
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync(),
                TodayCount = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today && a.Status != "Cancelled"),
                PendingCount = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Pending")
            };

            return View(viewModel);
        }

        // GET: My Appointments
        public async Task<IActionResult> Appointments(string status = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id);

            if (status != "all")
            {
                query = query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(appointments);
        }

        // GET: Appointment Details
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Prescriptions)  // ← ADD THIS LINE
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }
        // POST: Accept Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Confirmed";
            appointment.UpdatedAt = DateTime.Now;
            appointment.UpdatedBy = user.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment confirmed successfully!";
            return RedirectToAction("AppointmentDetails", new { id = id });
        }

        // POST: Reject Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTime.Now;
            appointment.UpdatedBy = user.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment rejected.";
            return RedirectToAction("AppointmentDetails", new { id = id });
        }

        // GET: Patient History
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(p => p.Prescriptions)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Complete Appointment (mark as completed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (doctor == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Completed";
            appointment.UpdatedAt = DateTime.Now;
            appointment.UpdatedBy = user.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment marked as completed.";
            return RedirectToAction("AppointmentDetails", new { id = id });
        }

    // POST: Update Clinical Notes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateClinicalNotes(int id, string diagnosis, string treatmentPlan, string clinicalNotes)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);
        
        if (appointment == null)
        {
            return NotFound();
        }
        
        appointment.Diagnosis = diagnosis ?? string.Empty;
        appointment.TreatmentPlan = treatmentPlan ?? string.Empty;
        appointment.ClinicalNotes = clinicalNotes ?? string.Empty;
        appointment.UpdatedAt = DateTime.Now;
        appointment.UpdatedBy = user.Id;
        
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Clinical notes saved successfully!";
        return RedirectToAction("AppointmentDetails", new { id = id });
    }

    // =============================================
    // PRESCRIPTIONS
    // =============================================

    // GET: Prescribe
    public async Task<IActionResult> Prescribe(int appointmentId)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);
        
        if (appointment == null)
        {
            return NotFound();
        }
        
        return View(appointment);
    }

    // POST: Create Prescription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrescription(int appointmentId, string medicationName, string dosage, 
        string frequency, string duration, int quantity, string instructions, bool isRefillable)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);
        
        if (appointment == null)
        {
            return NotFound();
        }
        
        var prescription = new Prescription
        {
            PatientId = appointment.PatientId,
            DoctorId = doctor.Id,
            AppointmentId = appointmentId,
            MedicationName = medicationName,
            Dosage = dosage,
            Frequency = frequency,
            Duration = duration,
            Quantity = quantity,
            Instructions = instructions ?? string.Empty,
            IsRefillable = isRefillable,
            RefillCount = isRefillable ? 1 : 0,
            PrescribedDate = DateTime.Now,
            Status = "Active"
        };
        
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Prescription created successfully!";
        return RedirectToAction("AppointmentDetails", new { id = appointmentId });
    }


    

    // GET: Report Requests (Doctor)
    public async Task<IActionResult> ReportRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        // Get requests for patients under this doctor
        var requests = await _context.MedicalReportRequests
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Where(r => r.DoctorId == doctor.Id || r.DoctorId == null)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
        
        return View(requests);
    }

    // GET: Generate Report
    public async Task<IActionResult> GenerateReport(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var request = await _context.MedicalReportRequests
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (request == null)
        {
            return NotFound();
        }
        
        // Get patient's appointments and prescriptions for the report
        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.PatientId == request.PatientId && a.Status == "Completed")
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
        
        var prescriptions = await _context.Prescriptions
            .Include(p => p.Doctor)
                .ThenInclude(d => d.User)
            .Where(p => p.PatientId == request.PatientId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();
        
        var viewModel = new GenerateReportViewModel
        {
            Request = request,
            Patient = request.Patient,
            Appointments = appointments,
            Prescriptions = prescriptions,
            DoctorName = $"Dr. {user.FirstName} {user.LastName}",
            GeneratedDate = DateTime.Now
        };
        
        return View(viewModel);
    }

    // POST: Generate Report
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateReport(int id, string reportContent, string summary)
    {
        var user = await _userManager.GetUserAsync(User);
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
        
        if (doctor == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var request = await _context.MedicalReportRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }
        
        request.Status = "Approved";
        request.ResponseDate = DateTime.Now;
        request.ApprovedBy = user.Id;
        request.ReportContent = reportContent;
        request.ReportSummary = summary;
        
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Medical report generated and sent to patient!";
        return RedirectToAction("ReportRequests");
    }

    // GET: Reject Report Request (shows the form)
            public async Task<IActionResult> RejectReportRequest(int id)
            {
                var user = await _userManager.GetUserAsync(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                
                if (doctor == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                var request = await _context.MedicalReportRequests
                    .Include(r => r.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(r => r.Id == id);
                
                if (request == null)
                {
                    return NotFound();
                }
                
                var viewModel = new RejectReportRequestViewModel
                {
                    RequestId = request.Id,
                    PatientName = $"{request.Patient.User.FirstName} {request.Patient.User.LastName}",
                    Reason = request.Reason
                };
                
                return View(viewModel);
            }

            // POST: Reject Report Request (processes the rejection)
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> RejectReportRequest(int id, string rejectionReason)
            {
                var user = await _userManager.GetUserAsync(User);
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                
                if (doctor == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                
                var request = await _context.MedicalReportRequests.FindAsync(id);
                if (request == null)
                {
                    return NotFound();
                }
                
                request.Status = "Rejected";
                request.ResponseDate = DateTime.Now;
                request.ApprovedBy = user.Id;
                request.RejectionReason = rejectionReason ?? "No reason provided.";
                
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Report request rejected successfully.";
                return RedirectToAction("ReportRequests");
            }




    }


}