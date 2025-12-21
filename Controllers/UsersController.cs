using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Models;
using HomeRadar.Attributes;
using HomeRadar.Services;

namespace HomeRadar.Controllers
{
    [AuthorizeRole("Admin")] // Sadece Admin erişebilir
    public class UsersController : Controller
    {
        private readonly EmlakContext _context;
        private readonly AuthService _authService;

        public UsersController(EmlakContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: Users - READ işlemi
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Message = "Kullanıcı Yönetimi";
            ViewBag.TotalUsers = users.Count;
            ViewBag.AdminCount = users.Count(u => u.Role == "Admin");
            ViewBag.UserCount = users.Count(u => u.Role == "User");

            return View(users);
        }

        // GET: Users/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Predictions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Kullanıcı Detayı";
            ViewData["PredictionCount"] = user.Predictions?.Count ?? 0;

            return View(user);
        }

        // GET: Users/Create - CREATE işlemi (form)
        public IActionResult Create()
        {
            ViewBag.Message = "Yeni Kullanıcı Ekle";
            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                new List<string> { "Admin", "User" });
            return View();
        }

        // POST: Users/Create - CREATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,PasswordHash,FirstName,LastName,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                new List<string> { "Admin", "User" }, user.Role);
            return View(user);
        }

        // GET: Users/Edit/5 - UPDATE işlemi (form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                new List<string> { "Admin", "User" }, user.Role);
            ViewBag.Message = "Kullanıcı Düzenle";
            return View(user);
        }

        // POST: Users/Edit/5 - UPDATE işlemi (kaydet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PasswordHash,FirstName,LastName,Role,IsActive,CreatedAt")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                new List<string> { "Admin", "User" }, user.Role);
            return View(user);
        }

        // GET: Users/Delete/5 - DELETE işlemi (onay)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5 - DELETE işlemi (sil)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Soft delete
                user.IsActive = false;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

