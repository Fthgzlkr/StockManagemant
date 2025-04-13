using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            return View();
        }

        // ✅ Tüm lokasyonları jqGrid’e uygun formatta getir
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


        [HttpGet]
        public async Task<IActionResult> GetLocationsByWarehouseId(int warehouseId)
        {
            var locations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
            return Json(locations);
        }
                        

        // ✅ Dinamik: Koridorları getir
        [HttpGet]
        public async Task<IActionResult> GetCorridors(int warehouseId)
        {
            var corridors = await _locationManager.GetCorridorsByWarehouseIdAsync(warehouseId);
            return Json(corridors);
        }

        // ✅ Dinamik: Rafları getir
        [HttpGet]
        public async Task<IActionResult> GetShelves(int warehouseId, string corridor)
        {
            var shelves = await _locationManager.GetShelvesByWarehouseAsync(warehouseId, corridor);
            return Json(shelves);
        }

        // ✅ Dinamik: Binleri getir
        [HttpGet]
        public async Task<IActionResult> GetBins(int warehouseId, string corridor, string shelf)
        {
            var bins = await _locationManager.GetBinsByWarehouseAsync(warehouseId, corridor, shelf);
            return Json(bins);
        }
    }
}