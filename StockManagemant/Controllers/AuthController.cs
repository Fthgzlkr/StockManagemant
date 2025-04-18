using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace StockManagemant.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAppUserManager _userManager;

        public AuthController(IAppUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AppUserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var user = await _userManager.AuthenticateAsync(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre yanlış.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };
            

            if (user.AssignedWarehouseId.HasValue)
            {
                claims.Add(new Claim("AssignedWarehouseId", user.AssignedWarehouseId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Rol bazlı yönlendirme
            if (user.Role == "BasicUser")
            {
                return Redirect($"/WarehouseProduct/WarehouseProducts?warehouseId={user.AssignedWarehouseId}");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}