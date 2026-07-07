using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using medicalapp.Models;
using medicalapp.Models.ViewModels;
using medicalapp.Data;
using System.Threading.Tasks;
using System.Linq;

namespace medicalapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ICNumber = model.ICNumber,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    ProfileImageUrl = "", // Add this line - empty string instead of null
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign "Patient" role
                    if (!await _roleManager.RoleExistsAsync("Patient"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Patient"));
                    }
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Create Patient profile
                    var patient = new Patient
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.Now
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();

                    // Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Redirect to RegisterSuccess
                    return RedirectToAction("RegisterSuccess", "Account", new { email = user.Email, userId = user.Id, token = token });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Get user roles to redirect appropriately
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                        return RedirectToAction("Dashboard", "Admin");
                    else if (roles.Contains("Doctor"))
                        return RedirectToAction("Dashboard", "Doctor");
                    else if (roles.Contains("Receptionist"))
                        return RedirectToAction("Dashboard", "Receptionist");
                    else
                        return RedirectToAction("Dashboard", "Patient");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "You must confirm your email before logging in. Please check your inbox.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RegisterSuccess(string email, string userId, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;
            ViewBag.UserId = userId;
            ViewBag.Token = token;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Status = "Error";
                ViewBag.Message = "Invalid confirmation link.";
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Status = "Error";
                ViewBag.Message = "User not found.";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Status = "Success";
                ViewBag.Message = "Thank you for confirming your email. You can now log in to your account.";
            }
            else
            {
                ViewBag.Status = "Error";
                ViewBag.Message = "Email confirmation failed. The link might be expired or invalid.";
            }

            return View();
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ForgotPasswordConfirmation", new { email = model.Email, token = "" });
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Redirect to ForgotPasswordConfirmation with token for local development mock
                return RedirectToAction("ForgotPasswordConfirmation", new { email = user.Email, token = token });
            }

            return View(model);
        }

        // GET: Account/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation(string email, string token)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // GET: Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ICNumber = user.ICNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                UserRole = role
            };

            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (patient != null)
                {
                    model.BloodType = patient.BloodType;
                    model.Allergies = patient.Allergies;
                    model.ChronicConditions = patient.ChronicConditions;
                    model.CurrentMedications = patient.CurrentMedications;
                    model.EmergencyContactName = patient.EmergencyContactName;
                    model.EmergencyContactPhone = patient.EmergencyContactPhone;
                    model.EmergencyContactRelationship = patient.EmergencyContactRelationship;
                }
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor != null)
                {
                    model.ConsultationFee = doctor.ConsultationFee;
                    model.YearsOfExperience = doctor.YearsOfExperience;
                    model.Bio = doctor.Bio;
                    model.Qualifications = doctor.Qualifications;
                    model.ClinicName = doctor.ClinicName;
                    model.ClinicAddress = doctor.ClinicAddress;
                    model.ClinicPhone = doctor.ClinicPhone;
                }
            }

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Repopulate read-only fields
            model.ICNumber = user.ICNumber;
            model.Gender = user.Gender;
            model.DateOfBirth = user.DateOfBirth;
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";
            model.UserRole = role;

            // Handle Email Updates & Duplicate Check
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email address is already in use by another account.");
                }
            }

            // Remove ChangePassword validation if we are only saving profile info
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("ChangePasswordModel")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;

                if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = model.Email;
                    user.UserName = model.Email; // Keep UserName in sync for email-based login
                    user.NormalizedEmail = model.Email.ToUpper();
                    user.NormalizedUserName = model.Email.ToUpper();
                }

                var userResult = await _userManager.UpdateAsync(user);
                if (userResult.Succeeded)
                {
                    if (role == "Patient")
                    {
                        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                        if (patient != null)
                        {
                            patient.BloodType = model.BloodType;
                            patient.Allergies = model.Allergies;
                            patient.ChronicConditions = model.ChronicConditions;
                            patient.CurrentMedications = model.CurrentMedications;
                            patient.EmergencyContactName = model.EmergencyContactName;
                            patient.EmergencyContactPhone = model.EmergencyContactPhone;
                            patient.EmergencyContactRelationship = model.EmergencyContactRelationship;
                            
                            _context.Patients.Update(patient);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (role == "Doctor")
                    {
                        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                        if (doctor != null)
                        {
                            doctor.ConsultationFee = model.ConsultationFee;
                            doctor.YearsOfExperience = model.YearsOfExperience;
                            doctor.Bio = model.Bio;
                            doctor.Qualifications = model.Qualifications;
                            doctor.ClinicName = model.ClinicName;
                            doctor.ClinicAddress = model.ClinicAddress;
                            doctor.ClinicPhone = model.ClinicPhone;

                            _context.Doctors.Update(doctor);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Refresh sign in cookies to reflect Email/UserName updates immediately
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            // Create a full profile model to return in case of errors
            var profileModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ICNumber = user.ICNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                UserRole = role,
                ChangePasswordModel = model
            };

            // Remove generic profile field validations from state since we only care about password inputs here
            var profileKeys = ModelState.Keys.Where(k => !k.StartsWith("ChangePasswordModel")).ToList();
            foreach (var key in profileKeys)
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Mark password tab active in case of errors
            ViewBag.ActiveTab = "password";
            return View("Profile", profileModel);
        }
    }
}