using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BSM311.Models;
using BSM311.Constants;
using Microsoft.AspNetCore.Authorization;

namespace BSM311.Controllers
{
    // Controllers/AccountController.cs
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
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
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign default role as Customer
                    await _userManager.AddToRoleAsync(user, UserRoles.Customer);

                    // Automatically sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null && await _userManager.IsInRoleAsync(user, UserRoles.Admin))
                    {
                        return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });
                    }

                    // If returnUrl is valid, redirect to it
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // If returnUrl is not valid, redirect to Customer Home page
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
