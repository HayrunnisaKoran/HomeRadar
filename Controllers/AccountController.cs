using Microsoft.AspNetCore.Mvc;
using HomeRadar.Data;
using HomeRadar.Models;
using HomeRadar.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmlakContext _context;
        private readonly AuthService _authService;

        public AccountController(EmlakContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Eğer zaten giriş yapılmışsa ana sayfaya yönlendir
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Giriş Yap";
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email ve şifre gereklidir.";
                return View();
            }

            // Basit authentication (Production'da hash kontrolü yapılmalı)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user != null)
            {
                // Basit şifre kontrolü (Production'da hash karşılaştırması yapılmalı)
                if (user.PasswordHash == password || user.PasswordHash.Contains(password))
                {
                    _authService.SetUser(user);
                    
                    // TempData ile başarı mesajı
                    TempData["SuccessMessage"] = $"Hoş geldiniz, {user.FirstName} {user.LastName}!";
                    
                    // Rol bazlı yönlendirme
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Users");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Listings");
                    }
                }
            }

            ViewBag.ErrorMessage = "Email veya şifre hatalı.";
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }
    }
}

