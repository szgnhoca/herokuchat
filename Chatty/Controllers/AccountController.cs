using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vektora.Authentication.Interfaces;

namespace Chatty.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMembershipProvider<IdentityUser, IdentityRole, KaleUstaPortalContext> _membershipProvider;

        public AccountController(IMembershipProvider<IdentityUser, IdentityRole, KaleUstaPortalContext> membershipProvider) => _membershipProvider = membershipProvider;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var signInResult = await _membershipProvider.SignIn(model.Email, model.Password, true, false);
            if (signInResult.Succeeded) return RedirectToAction("Index", "Home");
            return Json(signInResult);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> CreateUser(LoginViewModel model)
        {
            IdentityResult result = await _membershipProvider.CreateUser(new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email
            }, model.Password);
            TempData["SuccessMail"] = model.Email;
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            _membershipProvider.SignOut();
            return RedirectToAction("Login");
        }
    }

    public class LoginViewModel
    {
        [EmailAddress, Required]
        public string Email { get; set; }

        [MinLength(8), Required]
        public string Password { get; set; }
    }
}