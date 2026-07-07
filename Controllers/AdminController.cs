using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.Models;
using medicalapp.Models.ViewModels;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin (Redirect to Dashboard)
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingVerifications = await _context.Doctors.CountAsync(d => !d.IsVerified),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == DateTime.Today),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Pending"),
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: User Management
        public async Task<IActionResult> Users(string searchTerm = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(searchTerm) ||
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm));
            }

            var users = await query
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.SearchTerm = searchTerm;

            return View(users);
        }

        // GET: User Details
        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                CurrentRoles = roles.ToList(),
                AllRoles = allRoles.Select(r => r.Name).ToList()
            };

            return View(viewModel);
        }

        // POST: Update User Roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (roles != null && roles.Any())
            {
                await _userManager.AddToRolesAsync(user, roles);
            }

            TempData["Success"] = "User roles updated successfully!";
            return RedirectToAction("UserDetails", new { id = userId });
        }

        // GET: Pending Doctors (Verification)
        public async Task<IActionResult> PendingDoctors()
        {
            var pendingDoctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => !d.IsVerified)
                .ToListAsync();

            return View(pendingDoctors);
        }

        // POST: Verify Doctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            doctor.IsVerified = true;
            doctor.VerifiedAt = DateTime.Now;
            doctor.VerifiedBy = User.Identity.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Doctor verified successfully!";
            return RedirectToAction("PendingDoctors");
        }

        // POST: Reject Doctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Doctor rejected and removed from the system.";
            return RedirectToAction("PendingDoctors");
        }

        // GET: Doctor Details for Admin
        public async Task<IActionResult> DoctorDetails(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: System Settings
        public IActionResult Settings()
        {
            // You can fetch settings from a Settings table or use default values
            var settings = new SystemSettingsViewModel
            {
                ClinicName = "MediCloud Hospital",
                ClinicAddress = "123 Jalan Medik, 47100 Puchong",
                ClinicPhone = "03-1234 5678",
                ClinicEmail = "info@medicloud.com"
            };
            return View(settings);
        }

        // POST: Update Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SystemSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save settings logic here (you'd need a Settings table)
                TempData["Success"] = "Settings updated successfully!";
                return RedirectToAction("Settings");
            }
            return View(model);
        }

        // GET: Manage Departments
        public async Task<IActionResult> Departments()
        {
            var departments = await _context.Doctors
                .Select(d => d.Department)
                .Distinct()
                .Where(d => d != null)
                .ToListAsync();
            return View(departments);
        }

        // POST: Add Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartment(string departmentName)
        {
            if (!string.IsNullOrEmpty(departmentName))
            {
                // Add logic to create a new department
                // You can add to a Departments table if you have one
                TempData["Success"] = $"Department '{departmentName}' added successfully!";
            }
            return RedirectToAction("Departments");
        }

        // POST: Delete Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(string departmentName)
        {
            if (!string.IsNullOrEmpty(departmentName))
            {
                // Check if any doctors are assigned to this department
                var hasDoctors = await _context.Doctors.AnyAsync(d => d.Department == departmentName);
                if (hasDoctors)
                {
                    TempData["Error"] = $"Cannot delete '{departmentName}' - it has doctors assigned to it.";
                    return RedirectToAction("Departments");
                }
                // Delete department logic
                TempData["Success"] = $"Department '{departmentName}' deleted successfully!";
            }
            return RedirectToAction("Departments");
        }


        // GET: Create User (Admin)
        public IActionResult CreateUser()
        {
            var viewModel = new AdminCreateUserViewModel();
            return View(viewModel);
        }

        // POST: Create User (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            // Conditional validation for Doctor role
            if (model.Role == "Doctor")
            {
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "The Specialization field is required for Doctor role.");
                }
                if (string.IsNullOrWhiteSpace(model.Department))
                {
                    ModelState.AddModelError("Department", "The Department field is required for Doctor role.");
                }
                if (string.IsNullOrWhiteSpace(model.LicenseNumber))
                {
                    ModelState.AddModelError("LicenseNumber", "The License Number field is required for Doctor role.");
                }
            }

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

            // Create the user
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
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, model.Role);

            // If Doctor, create Doctor profile
            if (model.Role == "Doctor")
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = model.Specialization ?? "General",
                    Department = model.Department ?? "General",
                    LicenseNumber = model.LicenseNumber ?? "PENDING",
                    IsVerified = true, // Admin creates verified doctors
                    VerifiedAt = DateTime.Now,
                    VerifiedBy = User.Identity.Name,
                    ConsultationFee = model.ConsultationFee ?? 100.00m,
                    YearsOfExperience = model.YearsOfExperience ?? 0,
                    Bio = model.Bio ?? string.Empty,
                    ClinicName = "MediCloud Hospital",
                    ClinicAddress = "123 Jalan Medik, 47100 Puchong",
                    ClinicPhone = "03-1234 5678"
                };
                _context.Doctors.Add(doctor);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{model.Role} created successfully!";
            return RedirectToAction("Users");
        }




    }
}