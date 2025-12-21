using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using HomeRadar.Services;

namespace HomeRadar.Attributes
{
    /// <summary>
    /// Rol bazlı yetkilendirme attribute'u
    /// </summary>
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<AuthService>();
            
            if (authService == null || !authService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var userRole = authService.GetUserRole();
            
            if (userRole == null || !_allowedRoles.Contains(userRole))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

