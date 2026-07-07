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
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReceptionistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Receptionist (Redirect to Dashboard)
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: Receptionist Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var todayAppointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var pendingCount = await _context.Appointments
                .CountAsync(a => a.Status == "Pending");

            var todayCount = todayAppointments.Count;

            ViewBag.TodayCount = todayCount;
            ViewBag.PendingCount = pendingCount;

            return View(todayAppointments);
        }

        // GET: Book Appointment
        public async Task<IActionResult> BookAppointment()
        {
            var viewModel = new ReceptionistBookAppointmentViewModel
            {
                Patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.User.FirstName} {p.User.LastName} ({p.User.Email})"
                    })
                    .ToListAsync(),
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
        public async Task<IActionResult> BookAppointment(ReceptionistBookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.User.FirstName} {p.User.LastName} ({p.User.Email})"
                    })
                    .ToListAsync();
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

            var doctor = await _context.Doctors.FindAsync(model.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError("", "Selected doctor not found.");
                return View(model);
            }

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
                model.Patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.User.FirstName} {p.User.LastName} ({p.User.Email})"
                    })
                    .ToListAsync();
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
                model.Patients = await _context.Patients
                    .Include(p => p.User)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.User.FirstName} {p.User.LastName} ({p.User.Email})"
                    })
                    .ToListAsync();
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
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Pending",
                Type = "In-Person",
                ReasonForVisit = model.ReasonForVisit ?? string.Empty,
                Symptoms = model.Symptoms ?? string.Empty,
                ConsultationFee = doctor.ConsultationFee,
                TaxAmount = doctor.ConsultationFee * 0.06m,
                TotalAmount = doctor.ConsultationFee * 1.06m,
                IsPaid = false,
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("Dashboard");
        }

        // GET: All Appointments
        public async Task<IActionResult> AllAppointments(string status = "all")
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User);

            if (status != "all")
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Appointment, ApplicationUser>)query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(appointments);
        }

        // GET: Patient Registration
        public IActionResult RegisterPatient()
        {
            return View();
        }

        // POST: Patient Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(ReceptionistRegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ICNumber = model.ICNumber,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    UserId = user.Id,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    EmergencyContactRelationship = model.EmergencyContactRelationship,
                    CreatedAt = DateTime.Now
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Patient registered successfully!";
                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // POST: Check-in Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "InProgress";
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Patient checked in successfully!";
            return RedirectToAction("Dashboard");
        }

        // POST: Check-out Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Completed";
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Patient checked out successfully!";
            return RedirectToAction("Dashboard");
        }

        // GET: Appointment Details (Receptionist view)
        public async Task<IActionResult> AppointmentDetails(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Cancel Appointment (Receptionist)
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Cancel Appointment (Receptionist)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string cancellationReason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTime.Now;
            appointment.UpdatedBy = User.Identity.Name;
            appointment.DoctorNotes = $"Cancelled by receptionist. Reason: {cancellationReason ?? "Not specified"}";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment cancelled successfully!";
            return RedirectToAction("AllAppointments");
        }

        // GET: Reschedule Appointment (Receptionist)
        public async Task<IActionResult> RescheduleAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new RescheduleAppointmentViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorName = $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                CurrentDate = appointment.AppointmentDate,
                CurrentStartTime = appointment.StartTime,
                CurrentEndTime = appointment.EndTime,
                NewAppointmentDate = appointment.AppointmentDate,
                NewStartTime = appointment.StartTime,
                NewEndTime = appointment.EndTime
            };

            return View(viewModel);
        }

        // POST: Reschedule Appointment (Receptionist)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleAppointment(RescheduleAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment == null)
            {
                return NotFound();
            }

            // Check if the new slot is available (optional)
            var conflictingAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == appointment.DoctorId 
                            && a.Id != appointment.Id
                            && a.AppointmentDate.Date == model.NewAppointmentDate.Date
                            && a.Status != "Cancelled"
                            && ((model.NewStartTime >= a.StartTime && model.NewStartTime < a.EndTime)
                                || (model.NewEndTime > a.StartTime && model.NewEndTime <= a.EndTime)
                                || (model.NewStartTime <= a.StartTime && model.NewEndTime >= a.EndTime)));

            if (conflictingAppointment)
            {
                ModelState.AddModelError("", "The selected time slot is already booked. Please choose a different time.");
                return View(model);
            }

            // Update appointment
            appointment.AppointmentDate = model.NewAppointmentDate;
            appointment.StartTime = model.NewStartTime;
            appointment.EndTime = model.NewEndTime;
            appointment.UpdatedAt = DateTime.Now;
            appointment.UpdatedBy = User.Identity.Name;
            appointment.Status = "Pending"; // Reset status to pending for doctor re-approval

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment rescheduled successfully!";
            return RedirectToAction("AllAppointments");
        }

        // GET: Book Appointment for Referred Patient
        public async Task<IActionResult> BookReferredAppointment(int patientId, int doctorId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (patient == null || doctor == null)
            {
                return NotFound();
            }

            var viewModel = new ReceptionistBookAppointmentViewModel
            {
                PatientId = patientId,
                DoctorId = doctorId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                DoctorName = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}",
                AppointmentDate = DateTime.Now.AddDays(1), // Default to tomorrow
                ReasonForVisit = "Referred by another doctor"
            };

            return View(viewModel);
        }

        // POST: Book Appointment for Referred Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookReferredAppointment(ReceptionistBookAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FindAsync(model.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError("", "Selected doctor not found.");
                return View(model);
            }

            var startTime = model.AppointmentDate.TimeOfDay;
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));

            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Pending",
                Type = "In-Person",
                ReasonForVisit = model.ReasonForVisit ?? "Referred by another doctor",
                Symptoms = string.Empty,
                DoctorNotes = string.Empty,
                ConsultationFee = doctor.ConsultationFee,
                TaxAmount = doctor.ConsultationFee * 0.06m,
                TotalAmount = doctor.ConsultationFee * 1.06m,
                IsPaid = false,
                CreatedAt = DateTime.Now,
                CreatedBy = user?.Id ?? "system",
                UpdatedBy = string.Empty
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Appointment booked successfully with Dr. {doctor.User.FirstName} {doctor.User.LastName}!";
            return RedirectToAction("AllAppointments");
        }




    }
}