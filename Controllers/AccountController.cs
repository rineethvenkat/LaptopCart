using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LaptopCart.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
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
                return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            /*
             * The Login action method is responsible for handling user login.
             * It takes a LoginViewModel as input, which contains the user's email, password, and an optional return URL.
             * If the model state is valid, it attempts to sign in the user using the SignInManager.
             * If login is successful, the user is redirected to the specified return URL or the home page.
             * If there are any errors during login, an error message is added to the ModelState and the view is returned with error messages.
             */
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("ProductView", "Product");
                else
                    return Redirect(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            /*
             * The Logout action method is responsible for handling user logout.
             * It signs out the currently logged-in user using the SignInManager and redirects them to the home page.
             */
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
