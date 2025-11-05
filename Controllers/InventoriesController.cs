using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using galutine.Data;
using galutine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace galutine.Controllers
{
    [Authorize]
    public class InventoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public InventoriesController(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _um = um;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Inventories
                .Include(i => i.Owner)
                .Include(i => i.Items)
                .ToListAsync();
            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var inv = await _db.Inventories
                .Include(i => i.Fields)
                .Include(i => i.Items)
                    .ThenInclude(it => it.Likes)
                .Include(i => i.DiscussionPosts)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inv == null) return NotFound();
            return View(inv);
        }

        // CREATE INVENTORY
        public IActionResult Create()
        {
            return View(new Inventory());
        }

        // GET: /Inventories/AddItem?inventoryId=1
        public async Task<IActionResult> AddItem(int inventoryId)
        {
            var inv = await _db.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inv == null) return NotFound();

            // Optionally you could check permissions here (owner / access / public)
            var model = new InventoryItem { InventoryId = inventoryId };
            ViewData["Inventory"] = inv;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(InventoryItem model)
        {
            var inv = await _db.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == model.InventoryId);

            if (inv == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Inventory"] = inv;
                return View(model);
            }

            model.CreatedById = _um.GetUserId(User)!;

            // Collect field values from the posted form inputs named field_{id}
            foreach (var f in inv.Fields)
            {
                var value = Request.Form[$"field_{f.Id}"];
                model.FieldValues.Add(new FieldValue { InventoryFieldId = f.Id, Value = value });
            }

            _db.InventoryItems.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.InventoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.OwnerId = _um.GetUserId(User)!;
            // Ensure RowVersion is not null (SQLite column is NOT NULL)
            if (model.RowVersion == null)
            {
                model.RowVersion = Array.Empty<byte>();
            }

            _db.Inventories.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // EDIT INVENTORY 
        public async Task<IActionResult> Edit(int id)
        {
            var inv = await _db.Inventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();
            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inventory model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var inv = await _db.Inventories.FindAsync(model.Id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();

            inv.Title = model.Title;
            inv.Description = model.Description;
            inv.Category = model.Category;
            inv.ImageUrl = model.ImageUrl;
            inv.IsPublic = model.IsPublic;
            inv.Tags = model.Tags;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = inv.Id });
        }

        // DELETE INVENTORY 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inv = await _db.Inventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.OwnerId != _um.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();

            _db.Inventories.Remove(inv);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
