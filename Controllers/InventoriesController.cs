using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using galutine.Data;
using galutine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using System.Text.Json;

namespace galutine.Controllers
{
    [Authorize]
    public class InventoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        public InventoriesController(ApplicationDbContext db, UserManager<ApplicationUser> um) { _db = db; _um = um; }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Inventories.Include(i => i.Owner).ToListAsync();
            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var inv = await _db.Inventories.Include(i => i.Fields).Include(i => i.Items).ThenInclude(it => it.Likes).FirstOrDefaultAsync(i => i.Id == id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        public IActionResult Create() => View(new Inventory());

        [HttpPost]
        public async Task<IActionResult> Create(Inventory model)
        {
            model.OwnerId = _um.GetUserId(User)!;
            _db.Inventories.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = model.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inv = await _db.Inventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();
            return View(inv);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Inventory model)
        {
            var inv = await _db.Inventories.FindAsync(model.Id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();
            inv.Title = model.Title;
            inv.Description = model.Description;
            inv.IsPublic = model.IsPublic;
            inv.Tags = model.Tags;
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = inv.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var inv = await _db.Inventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();
            _db.Inventories.Remove(inv);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Add item
        [HttpGet]
        public async Task<IActionResult> AddItem(int inventoryId)
        {
            var inv = await _db.Inventories.Include(i => i.Fields).FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inv == null) return NotFound();
            return View(new InventoryItem { InventoryId = inventoryId });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(InventoryItem vm)
        {
            var inv = await _db.Inventories.Include(i => i.Fields).FirstOrDefaultAsync(i => i.Id == vm.InventoryId);
            if (inv == null) return NotFound();

            // generate a simple custom id if empty
            if (string.IsNullOrWhiteSpace(vm.CustomId))
                vm.CustomId = $"I{inv.Id}-{Guid.NewGuid().ToString("N").Substring(0,8).ToUpper()}";

            vm.CreatedById = _um.GetUserId(User)!;
            vm.CreatedAt = DateTime.UtcNow;
            _db.InventoryItems.Add(vm);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Custom ID duplicate. Please choose another.");
                return View(vm);
            }
            return RedirectToAction("Details", new { id = vm.InventoryId });
        }
    }
}
