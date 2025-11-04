using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using galutine.Models;
using galutine.Models.ViewModels;

namespace galutine.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _um;
        private readonly SignInManager<ApplicationUser> _sm;
        public AccountController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm) { _um = um; _sm = sm; }

        [HttpGet]
        public IActionResult Login(string returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var result = await _sm.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded) return LocalRedirect(vm.ReturnUrl ?? "/");
            ModelState.AddModelError("", "Invalid login");
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
            var res = await _um.CreateAsync(user, vm.Password);
            if (!res.Succeeded) { foreach (var e in res.Errors) ModelState.AddModelError("", e.Description); return View(vm); }
            await _sm.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _sm.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
