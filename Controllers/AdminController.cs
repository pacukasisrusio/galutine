using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using galutine.Models;
using Microsoft.AspNetCore.Authorization;

namespace galutine.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _um;
        public AdminController(UserManager<ApplicationUser> um) { _um = um; }

        public IActionResult Index()
        {
            var users = _um.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlock(string id)
        {
            var user = await _um.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsBlocked = !user.IsBlocked;
            await _um.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await _um.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (await _um.IsInRoleAsync(user, "Admin"))
            {
                await _um.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _um.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction("Index");
        }
    }
}
