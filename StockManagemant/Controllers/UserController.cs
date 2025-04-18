using Microsoft.AspNetCore.Mvc;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace StockManagemant.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IAppUserManager _userManager;

        public UserController(IAppUserManager userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.GetAllAsync();
            return Json(new { data = users });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AppUserCreateDto dto)
        {
            await _userManager.AddAsync(dto);
            return Ok(new { success = true, message = "Kullanıcı başarıyla oluşturuldu." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AppUserCreateDto dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var targetUser = await _userManager.GetByIdAsync(id);

            if (targetUser == null)
                return NotFound();

            if (currentUserRole == "Admin" ||
               (currentUserRole == "Operator" && (targetUser.Id == currentUserId || targetUser.Role == "BasicUser")) ||
               (currentUserRole == "BasicUser" && currentUserId == id))
            {
                await _userManager.UpdateAsync(id, dto);
                return Ok(new { success = true, message = "Kullanıcı güncellendi." });
            }

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var targetUser = await _userManager.GetByIdAsync(id);

            if (targetUser == null)
                return NotFound();

            if (currentUserRole == "Admin" ||
               (currentUserRole == "Operator" && targetUser.Role == "BasicUser"))
            {
                await _userManager.DeleteAsync(id);
                return Ok(new { success = true, message = "Kullanıcı silindi." });
            }

            return Forbid();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RestoreUser(int id)
        {
            await _userManager.RestoreAsync(id);
            return Ok(new { success = true, message = "Kullanıcı geri yüklendi." });
        }
    }
}
