using LaptopCartDAL.Data;
using LaptopCartModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LaptopCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            /*
             * The Register action method is responsible for handling user registration.
             * It takes a RegisterViewModel as input, which contains the user's email and password.
             * If the model state is valid, it creates a new ApplicationUser and attempts to register the user using the UserManager.
             * If registration is successful, the user is assigned the "User" role and signed in.
             * If there are any errors during registration, they are added to the ModelState and the view is returned with error messages.
             */
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("ProductView", "Product");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            /*
             * The Login action method is responsible for handling user login.
             * It takes a LoginViewModel as input, which contains the user's email, password, and an optional return URL.
             * If the model state is valid, it attempts to sign in the user using the SignInManager.
             * If login is successful, the user is redirected to the specified return URL or the home page.
             * If there are any errors during login, an error message is added to the ModelState and the view is returned with error messages.
             */
            //if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("ProductView", "Product");
                else
                    return Redirect(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Account locked out.");
                return View(model);
            }
            if (result.IsNotAllowed)
            {
                _logger.LogWarning("User not allowed to sign in.");
                ModelState.AddModelError(string.Empty, "You are not allowed to sign in.");
                return View(model);
            }
            else
            {
                _logger.LogWarning("Invalid login attempt.");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            /*
             * The Logout action method is responsible for handling user logout.
             * It signs out the currently logged-in user using the SignInManager and redirects them to the home page.
             */
            await _signInManager.SignOutAsync();
            return RedirectToAction("ProductView", "Product");
        }
    }
}
