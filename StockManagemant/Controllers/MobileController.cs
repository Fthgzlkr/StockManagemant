using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;

namespace StockManagemant.Controllers
{
    public class MobileController : Controller
    {
        private readonly IAppUserManager _userManager;

        public MobileController(IAppUserManager userManager)
        {
            _userManager = userManager;
        }

        // Ana sayfa - Mobile/Index
        [Route("Mobile")]
        [Route("Mobile/Index")]
        public IActionResult Index()
        {
            // Eğer giriş yapmamışsa login'e yönlendir
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            
            // Eğer role uygun değilse erişim engelle
            if (!User.IsInRole("Admin") && !User.IsInRole("Operator"))
            {
                return RedirectToAction("AccessDenied");
            }
            
            return View();
        }
        
        // Login sayfası - Mobile/Login (GET)
        [Route("Mobile/Login")]
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            
            return View();
        }
        
        // Login POST işlemi
        [HttpPost]
        [Route("Mobile/Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre gerekli";
                return View();
            }

            try
            {
                // Web'deki AuthController ile aynı authentication logic
                var user = await _userManager.AuthenticateAsync(username, password);
                if (user == null)
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre yanlış.";
                    return View();
                }

                // Mobil app sadece Admin ve Operator'lere açık
                if (user.Role != "Admin" && user.Role != "Operator")
                {
                    ViewBag.Error = "Bu uygulamayı kullanma yetkiniz yok. Sadece Admin ve Operator erişebilir.";
                    return View();
                }

                // Claims oluştur (Web ile aynı)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

                if (user.AssignedWarehouseId.HasValue)
                {
                    claims.Add(new Claim("AssignedWarehouseId", user.AssignedWarehouseId.Value.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Mobil ana sayfaya yönlendir
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Giriş sırasında hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Mobil logout
        [Route("Mobile/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        
        // Erişim engellendi sayfası
        [Route("Mobile/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        // Ürünler sayfası
        [Authorize(Roles = "Admin,Operator")]
        [Route("Mobile/Products")]
        public IActionResult Products()
        {
            return View();
        }
        
        // Depo sayfası
        [Authorize(Roles = "Admin,Operator")]
        [Route("Mobile/Warehouse")]
        public IActionResult Warehouse()
        {
            return View();
        }
    }
}