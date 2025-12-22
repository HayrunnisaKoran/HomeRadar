using Microsoft.AspNetCore.Mvc;
using HomeRadar.Models;
using HomeRadar.Services;
using BCrypt.Net;

namespace HomeRadar.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public AccountController(IUserService userService, AuthService authService)
        {
            _userService = userService;
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

            var user = await _userService.GetUserByEmailAsync(email);

            if (user != null)
            {
                // BCrypt ile şifre kontrolü
                bool isPasswordValid = false;
                try
                {
                    // BCrypt hash kontrolü
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
                catch
                {
                    // Eğer hash BCrypt formatında değilse (eski kayıtlar için), eski kontrolü yap
                    isPasswordValid = user.PasswordHash == password || user.PasswordHash.Contains(password);
                }

                if (isPasswordValid)
                {
                    _authService.SetUser(user);
                    
                    // TempData ile başarı mesajı
                    TempData["SuccessMessage"] = $"Hoş geldiniz, {user.FirstName} {user.LastName}!";
                    
                    // Giriş yaptıktan sonra herkesi ana sayfaya yönlendir
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.ErrorMessage = "Email veya şifre hatalı.";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            // Eğer zaten giriş yapılmışsa ana sayfaya yönlendir
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Kayıt Ol";
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string confirmPassword)
        {
            // Validasyon kontrolleri
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Tüm alanlar gereklidir.";
                ViewBag.Message = "Kayıt Ol";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Şifreler eşleşmiyor.";
                ViewBag.Message = "Kayıt Ol";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.ErrorMessage = "Şifre en az 6 karakter olmalıdır.";
                ViewBag.Message = "Kayıt Ol";
                return View();
            }

            // Email kontrolü - aynı email ile kayıt olunamaz
            var existingUser = await _userService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Bu email adresi zaten kullanılıyor.";
                ViewBag.Message = "Kayıt Ol";
                return View();
            }

            // Yeni kullanıcı oluştur
            var newUser = new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // BCrypt ile şifre hash'leme
                Role = "User", // Yeni kayıtlar varsayılan olarak User rolünde
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                await _userService.CreateUserAsync(newUser);
                
                TempData["SuccessMessage"] = $"Kayıt başarılı! Hoş geldiniz, {newUser.FirstName} {newUser.LastName}!";
                
                // Otomatik giriş yap
                _authService.SetUser(newUser);
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Kayıt sırasında bir hata oluştu: {ex.Message}";
                ViewBag.Message = "Kayıt Ol";
                return View();
            }
        }

        // GET: Account/MyAccount
        public async Task<IActionResult> MyAccount()
        {
            // Giriş kontrolü
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _authService.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Message = "Hesabım";
            return View(user);
        }

        // POST: Account/MyAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyAccount(string firstName, string lastName, string email, string currentPassword, string newPassword, string confirmPassword)
        {
            // Giriş kontrolü
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = _authService.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validasyon kontrolleri
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Ad, Soyad ve Email alanları gereklidir.";
                ViewBag.Message = "Hesabım";
                return View(user);
            }

            // Email kontrolü - eğer email değiştiyse, yeni email'in başka bir kullanıcıda olup olmadığını kontrol et
            if (user.Email != email.Trim().ToLower())
            {
                var existingUser = await _userService.GetUserByEmailAsync(email.Trim().ToLower());
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ViewBag.ErrorMessage = "Bu email adresi başka bir kullanıcı tarafından kullanılıyor.";
                    ViewBag.Message = "Hesabım";
                    return View(user);
                }
            }

            // Şifre değişikliği kontrolü
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 6)
                {
                    ViewBag.ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.";
                    ViewBag.Message = "Hesabım";
                    return View(user);
                }

                if (newPassword != confirmPassword)
                {
                    ViewBag.ErrorMessage = "Yeni şifreler eşleşmiyor.";
                    ViewBag.Message = "Hesabım";
                    return View(user);
                }

                // Mevcut şifre kontrolü
                if (string.IsNullOrEmpty(currentPassword))
                {
                    ViewBag.ErrorMessage = "Şifre değiştirmek için mevcut şifrenizi girmelisiniz.";
                    ViewBag.Message = "Hesabım";
                    return View(user);
                }

                // Mevcut şifre doğrulama
                bool isCurrentPasswordValid = false;
                try
                {
                    isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
                }
                catch
                {
                    isCurrentPasswordValid = user.PasswordHash == currentPassword || user.PasswordHash.Contains(currentPassword);
                }

                if (!isCurrentPasswordValid)
                {
                    ViewBag.ErrorMessage = "Mevcut şifre hatalı.";
                    ViewBag.Message = "Hesabım";
                    return View(user);
                }

                // Yeni şifreyi hash'le
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            // Kullanıcı bilgilerini güncelle
            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.Email = email.Trim().ToLower();

            try
            {
                await _userService.UpdateUserAsync(user);
                
                // Session'daki kullanıcı adını güncelle
                _authService.SetUser(user);
                
                TempData["SuccessMessage"] = "Hesap bilgileriniz başarıyla güncellendi!";
                return RedirectToAction("MyAccount", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Güncelleme sırasında bir hata oluştu: {ex.Message}";
                ViewBag.Message = "Hesabım";
                return View(user);
            }
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

