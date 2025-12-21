using HomeRadar.Models;

namespace HomeRadar.Services
{
    /// <summary>
    /// Basit Authentication/Authorization servisi
    /// Session tabanlı rol yönetimi için
    /// </summary>
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetUser(User user)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("UserId", user.Id.ToString());
            _httpContextAccessor.HttpContext?.Session.SetString("UserEmail", user.Email);
            _httpContextAccessor.HttpContext?.Session.SetString("UserRole", user.Role);
            _httpContextAccessor.HttpContext?.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserRole");
        }

        public int? GetUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }

        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");
        }

        public string? GetUserName()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserName");
        }

        public bool IsAdmin()
        {
            return GetUserRole() == "Admin";
        }

        public bool IsAuthenticated()
        {
            return GetUserId().HasValue;
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }
    }
}

