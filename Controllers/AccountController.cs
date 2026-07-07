using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    }
}