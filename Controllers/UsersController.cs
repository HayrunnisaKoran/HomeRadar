using Microsoft.AspNetCore.Mvc;
using HomeRadar.Models;
using HomeRadar.Attributes;
using HomeRadar.Services;

namespace HomeRadar.Controllers
{
    [AuthorizeRole("Admin")] // Sadece Admin erişebilir
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public UsersController(IUserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // GET: Users - READ işlemi
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();

            ViewBag.Message = "Kullanıcı Yönetimi";
            ViewBag.TotalUsers = users.Count();
            ViewBag.AdminCount = await _userService.GetAdminCountAsync();
            ViewBag.UserCount = await _userService.GetRegularUserCountAsync();

            return View(users);
        }

        // GET: Users/Details/5 - READ işlemi (detay)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserWithPredictionsAsync(id.Value);

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
                await _userService.CreateUserAsync(user);

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

            var user = await _userService.GetUserByIdAsync(id.Value);
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
                    await _userService.UpdateUserAsync(user);

                    TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi!";
                }
                catch
                {
                    if (!await _userService.UserExistsAsync(user.Id))
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

            var user = await _userService.GetUserByIdAsync(id.Value);

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
            await _userService.DeleteUserAsync(id);

            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi!";

            return RedirectToAction(nameof(Index));
        }
    }
}

