using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.BusinessLogic.Managers.Interfaces;
using StockManagemant.Entities.DTO;
using AutoMapper;

namespace StockManagemant.Web.Controllers
{
    
    public class WarehouseLocationController : Controller
    {
        private readonly IWareHouseLocationManager _locationManager;
        private readonly IMapper _mapper;

        public WarehouseLocationController(IWareHouseLocationManager locationManager, IMapper mapper)
        {
            _locationManager = locationManager;
            _mapper = mapper;
        }

        // Ana sayfa - Yönetim arayüzü açılacak
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Tüm lokasyonları jqGrid’e uygun formatta getir
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var locations = await _locationManager.GetAllAsync();

                var jsonData = new
                {
                    total = 1,
                    page = 1,
                    records = locations.Count(),
                    rows = locations.Select(l => new
                    {
                        id = l.Id,
                        warehouseId = l.WarehouseId,
                        corridor = l.Corridor,
                        shelf = l.Shelf,
                        bin = l.Bin
                    })
                };

                return Json(jsonData);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lokasyonlar getirilirken hata oluştu: " + ex.Message });
            }
        }

        // ✅ Yeni lokasyon ekleme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] WarehouseLocationDto locationDto)
        {
            if (!ModelState.IsValid || locationDto == null || string.IsNullOrEmpty(locationDto.Corridor))
            {
                return Json(new { success = false, message = "Geçersiz lokasyon bilgisi." });
            }

            try
            {
                await _locationManager.AddAsync(locationDto);
                return Json(new { success = true, message = "Lokasyon başarıyla eklendi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon eklenirken hata oluştu: " + ex.Message });
            }
        }

        // ✅ Lokasyon güncelleme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditLocation([FromBody] WarehouseLocationDto locationDto)
        {
            if (!ModelState.IsValid || locationDto == null || locationDto.Id <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz lokasyon verisi." });
            }

            try
            {
                await _locationManager.UpdateAsync(locationDto);
                return Json(new { success = true, message = "Lokasyon başarıyla güncellendi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon güncellenirken hata oluştu: " + ex.Message });
            }
        }

        // ✅ Lokasyon silme (soft delete)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Geçersiz lokasyon ID." });
            }

            try
            {
                await _locationManager.DeleteAsync(id);
                return Json(new { success = true, message = "Lokasyon silindi." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon silinirken hata oluştu: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetLocationsByWarehouseId(int warehouseId)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            var locations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
            return Json(locations);
        }

        // ✅ Dinamik: Koridorları getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetCorridors(int warehouseId)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            var corridors = await _locationManager.GetCorridorsByWarehouseIdAsync(warehouseId);
            return Json(corridors);
        }

        // ✅ Dinamik: Rafları getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetShelves(int warehouseId, string corridor)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            var shelves = await _locationManager.GetShelvesByWarehouseAsync(warehouseId, corridor);
            return Json(shelves);
        }

        // ✅ Dinamik: Binleri getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetBins(int warehouseId, string corridor, string shelf)
        {
            if (User.IsInRole("BasicUser"))
            {
                var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }

            var bins = await _locationManager.GetBinsByWarehouseAsync(warehouseId, corridor, shelf);
            return Json(bins);
        }
    }
}