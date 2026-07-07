using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.Models;
using medicalapp.Models.ViewModels;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient (Redirect to Dashboard)
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: Patient Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new PatientDashboardViewModel
            {
                PatientId = patient.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                UpcomingAppointments = patient.Appointments
                    .Where(a => a.AppointmentDate >= DateTime.Today && a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .Take(3)
                    .ToList(),
                PastAppointments = patient.Appointments
                    .Where(a => a.AppointmentDate < DateTime.Today || a.Status == "Completed")
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(3)
                    .ToList(),
                UpcomingCount = patient.Appointments
                    .Count(a => a.AppointmentDate >= DateTime.Today && a.Status != "Cancelled"),
                PastCount = patient.Appointments
                    .Count(a => a.AppointmentDate < DateTime.Today || a.Status == "Completed"),
                ActivePrescriptions = await _context.Prescriptions
                    .Where(p => p.PatientId == patient.Id && p.Status == "Active")
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Book Appointment
        public async Task<IActionResult> BookAppointment(int? doctorId)
        {
            var viewModel = new BookAppointmentViewModel
            {
                DoctorId = doctorId ?? 0,
                Doctors = await _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.IsVerified)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization}"
                    })
                    .ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Book Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            try
            {
                Console.WriteLine("=== BOOKING APPOINTMENT ===");
                Console.WriteLine($"DoctorId: {model.DoctorId}");
                Console.WriteLine($"AppointmentDate: {model.AppointmentDate}");
                Console.WriteLine($"ReasonForVisit: {model.ReasonForVisit}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid!");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Error: {error.ErrorMessage}");
                    }

                    model.Doctors = await _context.Doctors
                        .Include(d => d.User)
                        .Where(d => d.IsVerified)
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization}"
                        })
                        .ToListAsync();
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please login again.";
                    return RedirectToAction("Login", "Account");
                }
                Console.WriteLine($"User: {user.Email}");

                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found. Please contact support.";
                    return RedirectToAction("Dashboard");
                }
                Console.WriteLine($"PatientId: {patient.Id}");

                var doctor = await _context.Doctors.FindAsync(model.DoctorId);
                if (doctor == null)
                {
                    ModelState.AddModelError("", "Selected doctor not found.");
                    model.Doctors = await _context.Doctors
                        .Include(d => d.User)
                        .Where(d => d.IsVerified)
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization}"
                        })
                        .ToListAsync();
                    return View(model);
                }
                Console.WriteLine($"DoctorId: {doctor.Id}, Fee: {doctor.ConsultationFee}");

                var startTime = model.AppointmentDate.TimeOfDay;
                var endTime = startTime.Add(TimeSpan.FromMinutes(30));

                var dayOfWeek = model.AppointmentDate.DayOfWeek;
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.DoctorId == model.DoctorId && 
                                              s.DayOfWeek == dayOfWeek && 
                                              s.IsAvailable &&
                                              s.StartTime <= startTime && 
                                              endTime <= s.EndTime);

                if (schedule == null)
                {
                    ModelState.AddModelError("AppointmentDate", "The selected time is outside of the doctor's scheduled hours.");
                    model.Doctors = await _context.Doctors
                        .Include(d => d.User)
                        .Where(d => d.IsVerified)
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization}"
                        })
                        .ToListAsync();
                    return View(model);
                }

                var isOverlapping = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == model.DoctorId && 
                                   a.AppointmentDate.Date == model.AppointmentDate.Date && 
                                   a.StartTime == startTime && 
                                   a.Status != "Cancelled");

                if (isOverlapping)
                {
                    ModelState.AddModelError("AppointmentDate", "This time slot has already been booked. Please choose another time.");
                    model.Doctors = await _context.Doctors
                        .Include(d => d.User)
                        .Where(d => d.IsVerified)
                        .Select(d => new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization}"
                        })
                        .ToListAsync();
                    return View(model);
                }

                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = model.DoctorId,
                    AppointmentDate = model.AppointmentDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = "Pending",
                    Type = "In-Person",
                    ReasonForVisit = model.ReasonForVisit ?? string.Empty,
                    Symptoms = model.Symptoms ?? string.Empty,
                    DoctorNotes = string.Empty,
                    ConsultationFee = doctor.ConsultationFee,
                    TaxAmount = doctor.ConsultationFee * 0.06m,
                    TotalAmount = doctor.ConsultationFee * 1.06m,
                    IsPaid = false,
                    PaymentMethod = string.Empty,
                    PaymentReference = string.Empty,
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.Id,
                    UpdatedBy = string.Empty
                };

                Console.WriteLine("Adding appointment to context...");
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                Console.WriteLine("Appointment saved successfully!");

                TempData["Success"] = "Appointment booked successfully! Please wait for doctor confirmation.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"STACK: {ex.StackTrace}");
                TempData["Error"] = $"Failed to book appointment: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        // GET: My Appointments
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(patient.Appointments ?? new List<Appointment>());
        }

        // GET: Request Medical Report
        public async Task<IActionResult> RequestMedicalReport()
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var doctorList = patient.Appointments
                .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                .Select(a => a.Doctor)
                .Distinct()
                .ToList();

            if (!doctorList.Any())
            {
                TempData["Error"] = "You don't have any completed or confirmed appointments yet.";
                return RedirectToAction("Dashboard");
            }

            var viewModel = new MedicalReportRequestViewModel
            {
                Doctors = doctorList.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization} ({d.Department})"
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Request Medical Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMedicalReport(MedicalReportRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var patient = await _context.Patients
                    .Include(p => p.Appointments)
                        .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (patient != null)
                {
                    var doctorList = patient.Appointments
                        .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                        .Select(a => a.Doctor)
                        .Distinct()
                        .ToList();

                    model.Doctors = doctorList.Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialization} ({d.Department})"
                    }).ToList();
                }
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            if (currentPatient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var request = new MedicalReportRequest
            {
                PatientId = currentPatient.Id,
                DoctorId = model.DoctorId,
                Reason = model.Reason,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                Status = "Pending",
                RequestDate = DateTime.Now
            };

            _context.MedicalReportRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your medical report request has been submitted to your selected doctor.";
            return RedirectToAction("Dashboard");
        }

        // POST: Cancel Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Dashboard");
            }

            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                TempData["Error"] = "This appointment cannot be cancelled.";
                return RedirectToAction("Dashboard");
            }

            appointment.Status = "Cancelled";
            appointment.UpdatedBy = user.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment cancelled successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorFee(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound();
            }
            return Json(new { fee = doctor.ConsultationFee });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string date)
        {
            if (doctorId <= 0 || string.IsNullOrEmpty(date))
            {
                return BadRequest("Invalid doctor or date.");
            }

            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            var dayOfWeek = parsedDate.DayOfWeek;

            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek && s.IsAvailable)
                .ToListAsync();

            var availableSlots = new List<string>();

            if (!schedules.Any())
            {
                return Json(availableSlots);
            }

            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                            a.AppointmentDate.Date == parsedDate.Date && 
                            a.Status != "Cancelled")
                .Select(a => a.StartTime)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                var current = schedule.StartTime;
                while (current < schedule.EndTime)
                {
                    if (!existingAppointments.Contains(current))
                    {
                        availableSlots.Add(current.ToString(@"hh\:mm"));
                    }
                    current = current.Add(TimeSpan.FromMinutes(schedule.SlotDuration));
                }
            }

            var result = availableSlots.Distinct().OrderBy(s => s).ToList();
            return Json(result);
        }
    }
}