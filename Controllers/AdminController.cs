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
        private readonly RoleManager<IdentityRole> _rm;

        public AdminController(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
        {
            _um = um;
            _rm = rm;
        }

        public async Task<IActionResult> Index()
        {
            var users = _um.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
                userRoles[user.Id] = await _um.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlock(string id)
        {
            var user = await _um.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBlocked = !user.IsBlocked;
            await _um.UpdateAsync(user);

            TempData["Message"] = $"{user.Email} has been {(user.IsBlocked ? "blocked" : "unblocked")}.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await _um.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Ensure Admin role exists
            if (!await _rm.RoleExistsAsync("Admin"))
                await _rm.CreateAsync(new IdentityRole("Admin"));

            if (await _um.IsInRoleAsync(user, "Admin"))
            {
                await _um.RemoveFromRoleAsync(user, "Admin");
                TempData["Message"] = $"{user.Email} has been removed from Admin role.";
            }
            else
            {
                await _um.AddToRoleAsync(user, "Admin");
                TempData["Message"] = $"{user.Email} has been granted Admin role.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _um.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _um.DeleteAsync(user);
            TempData["Message"] = $"{user.Email} has been deleted.";

            return RedirectToAction("Index");
        }
    }
}
