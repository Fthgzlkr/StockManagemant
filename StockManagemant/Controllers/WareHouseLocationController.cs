using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.BusinessLogic.Managers.Interfaces;
using StockManagemant.Entities.Enums;
using StockManagemant.Entities.DTO;
using AutoMapper;

namespace StockManagemant.Web.Controllers
{ 
       [Authorize]
    public class WarehouseLocationController : Controller
    {
        private readonly IWareHouseLocationManager _locationManager;
        private readonly IMapper _mapper;

        public WarehouseLocationController(IWareHouseLocationManager locationManager, IMapper mapper)
        {
            _locationManager = locationManager;
            _mapper = mapper;
        }

        // Ana sayfa - Dinamik lokasyon yönetimi arayüzü
        [Authorize(Roles = "Admin")]
        public IActionResult Index(int? warehouseId)
        {
            ViewBag.WarehouseId = warehouseId ?? 1; // Default warehouse ID
            return View();
        }

        // Tüm lokasyonları jqGrid'e uygun formatta getir
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
                        name = l.Name,
                        level = l.Level.ToString(),
                        parentId = l.ParentId,
                        storageType = l.StorageType.ToString()
                    })
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyonlar getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Belirli bir depoya ait lokasyonları getir (Dinamik yapı için)
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetLocationsByWarehouseId(int warehouseId)
        {
            try
            {
                // BasicUser role kontrolü
                if (User.IsInRole("BasicUser"))
                {
                    var assignedWarehouseId = User.FindFirst("AssignedWarehouseId")?.Value;
                    if (assignedWarehouseId == null || assignedWarehouseId != warehouseId.ToString())
                    {
                        return Json(new { success = false, message = "Bu depoya erişim yetkiniz yok." });
                    }
                }

                var locations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
                
                // Lokasyonları frontend için uygun formatta döndür
                var result = locations.Select(l => new
                {
                    id = l.Id,
                    name = l.Name,
                    level = l.Level,
                    parentId = l.ParentId,
                    warehouseId = l.WarehouseId,
                    storageType = (int)l.StorageType,
                    hasChildren = locations.Any(child => child.ParentId == l.Id)
                }).OrderBy(l => l.level).ThenBy(l => l.name);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyonlar getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Yeni lokasyon ekleme (Dinamik yapı için geliştirildi)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] WarehouseLocationDto locationDto)
        {
            if (!ModelState.IsValid || locationDto == null || string.IsNullOrWhiteSpace(locationDto.Name))
            {
                return Json(new { success = false, message = "Geçersiz lokasyon bilgisi." });
            }

            try
            {
                // Parent varsa kontrol et
                if (locationDto.ParentId.HasValue)
                {
                    var parent = await _locationManager.GetByIdAsync(locationDto.ParentId.Value);
                    if (parent == null)
                    {
                        return Json(new { success = false, message = "Geçersiz üst lokasyon ID." });
                    }

                    // Hiyerarşi kuralı: Alt seviye, üst seviyeden büyük olmalı
                    if ((int)locationDto.Level <= (int)parent.Level)
                    {
                        return Json(new { success = false, message = "Alt seviye, üst seviyeden büyük olmalıdır." });
                    }

                    // Maksimum seviye kontrolü (5 seviye)
                    if ((int)locationDto.Level > 5)
                    {
                        return Json(new { success = false, message = "Maksimum 5 seviye desteklenmektedir." });
                    }

                    // Aynı parent altında aynı isimde lokasyon kontrolü
                    var existingLocations = await _locationManager.GetLocationsByWarehouseIdAsync(locationDto.WarehouseId);
                    if (existingLocations.Any(l => l.ParentId == locationDto.ParentId && 
                                                  l.Name.Equals(locationDto.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { success = false, message = "Bu üst lokasyon altında aynı isimde lokasyon zaten mevcut." });
                    }
                }
                else
                {
                    // Kök seviye kontrolü (Parent olmayan)
                    if ((int)locationDto.Level != 1)
                    {
                        return Json(new { success = false, message = "Ana lokasyonlar (parent olmayan) seviye 1 olmalıdır." });
                    }

                    // Aynı depoda aynı isimde kök lokasyon kontrolü
                    var existingLocations = await _locationManager.GetLocationsByWarehouseIdAsync(locationDto.WarehouseId);
                    if (existingLocations.Any(l => l.ParentId == null && 
                                                  l.Name.Equals(locationDto.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { success = false, message = "Bu depoda aynı isimde ana lokasyon zaten mevcut." });
                    }
                }

                // StorageType kontrolü
                if (!Enum.IsDefined(typeof(StorageType), locationDto.StorageType))
                {
                    return Json(new { success = false, message = "Geçersiz depolama türü." });
                }

                await _locationManager.AddAsync(locationDto);
                return Json(new { success = true, message = "Lokasyon başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon eklenirken hata oluştu: " + ex.Message });
            }
        }

        // Lokasyon güncelleme (Dinamik yapı için geliştirildi)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditLocation([FromBody] WarehouseLocationDto locationDto)
        {
            if (!ModelState.IsValid || locationDto == null || locationDto.Id <= 0)
            {
                return Json(new { success = false, message = "Geçersiz lokasyon verisi." });
            }

            try
            {
                // Mevcut lokasyonu kontrol et
                var existingLocation = await _locationManager.GetByIdAsync(locationDto.Id);
                if (existingLocation == null)
                {
                    return Json(new { success = false, message = "Güncellenecek lokasyon bulunamadı." });
                }

                // Parent değişikliği kontrolü
                if (locationDto.ParentId.HasValue)
                {
                    var parent = await _locationManager.GetByIdAsync(locationDto.ParentId.Value);
                    if (parent == null)
                    {
                        return Json(new { success = false, message = "Üst lokasyon bulunamadı." });
                    }

                    // Kendisini parent yapma kontrolü
                    if (locationDto.ParentId.Value == locationDto.Id)
                    {
                        return Json(new { success = false, message = "Lokasyon kendi üst lokasyonu olamaz." });
                    }

                    // Döngüsel referans kontrolü (child'larından birini parent yapma)
                    var children = await GetAllChildrenRecursive(locationDto.Id);
                    if (children.Contains(locationDto.ParentId.Value))
                    {
                        return Json(new { success = false, message = "Alt lokasyonlardan biri üst lokasyon olarak seçilemez (döngüsel referans)." });
                    }

                    // Seviye kontrolü
                    if ((int)locationDto.Level <= (int)parent.Level)
                    {
                        return Json(new { success = false, message = "Alt seviye, üst seviyeden büyük olmalıdır." });
                    }
                }

                // StorageType kontrolü
                if (!Enum.IsDefined(typeof(StorageType), locationDto.StorageType))
                {
                    return Json(new { success = false, message = "Geçersiz depolama türü." });
                }

                // İsim değişikliği kontrolü (aynı seviyede aynı isim olmamalı)
                if (!existingLocation.Name.Equals(locationDto.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var existingLocations = await _locationManager.GetLocationsByWarehouseIdAsync(locationDto.WarehouseId);
                    if (existingLocations.Any(l => l.Id != locationDto.Id && 
                                                  l.ParentId == locationDto.ParentId && 
                                                  l.Name.Equals(locationDto.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { success = false, message = "Bu seviyede aynı isimde lokasyon zaten mevcut." });
                    }
                }

                await _locationManager.UpdateAsync(locationDto);
                return Json(new { success = true, message = "Lokasyon başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon güncellenirken hata oluştu: " + ex.Message });
            }
        }

        // Lokasyon silme (Cascade delete ile alt lokasyonları da sil)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Geçersiz lokasyon ID." });
            }

            try
            {
                var location = await _locationManager.GetByIdAsync(id);
                if (location == null)
                {
                    return Json(new { success = false, message = "Silinecek lokasyon bulunamadı." });
                }

                // Alt lokasyonları kontrol et
                var children = await GetAllChildrenRecursive(id);
                if (children.Any())
                {
                    // Alt lokasyonları da silecek misin kontrolü (frontend'de yapılmalı)
                    // Şimdilik cascade delete yapıyoruz
                    foreach (var childId in children.OrderByDescending(x => x)) // Derinlikten başla
                    {
                        await _locationManager.DeleteAsync(childId);
                    }
                }

                await _locationManager.DeleteAsync(id);
                return Json(new { success = true, message = "Lokasyon ve alt lokasyonları başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon silinirken hata oluştu: " + ex.Message });
            }
        }

        // Belirli bir lokasyonun alt lokasyonlarını getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetChildren(int parentId)
        {
            try
            {
                var children = await _locationManager.GetChildrenAsync(parentId);
                
                var result = children.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    level = c.Level,
                    parentId = c.ParentId,
                    storageType = (int)c.StorageType,
                    hasChildren = children.Any(child => child.ParentId == c.Id)
                }).OrderBy(c => c.name);

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Alt lokasyonlar getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Lokasyon hiyerarşisini breadcrumb formatında getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetLocationPath(int locationId)
        {
            try
            {
                var path = new List<object>();
                var currentLocation = await _locationManager.GetByIdAsync(locationId);
                
                while (currentLocation != null)
                {
                    path.Insert(0, new
                    {
                        id = currentLocation.Id,
                        name = currentLocation.Name,
                        level = currentLocation.Level
                    });
                    
                    if (currentLocation.ParentId.HasValue)
                    {
                        currentLocation = await _locationManager.GetByIdAsync(currentLocation.ParentId.Value);
                    }
                    else
                    {
                        break;
                    }
                }

                return Json(path);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon yolu getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Belirli seviyedeki tüm lokasyonları getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetLocationsByLevel(int warehouseId, int level)
        {
            try
            {
                var allLocations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
                var filteredLocations = allLocations.Where(l => (int)l.Level == level)
                                                   .Select(l => new
                                                   {
                                                       id = l.Id,
                                                       name = l.Name,
                                                       level = l.Level,
                                                       parentId = l.ParentId,
                                                       storageType = (int)l.StorageType
                                                   })
                                                   .OrderBy(l => l.name);

                return Json(filteredLocations);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Seviye lokasyonları getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Depolama türüne göre lokasyonları getir
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> GetLocationsByStorageType(int warehouseId, StorageType storageType)
        {
            try
            {
                var allLocations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
                var filteredLocations = allLocations.Where(l => l.StorageType == storageType)
                                                   .Select(l => new
                                                   {
                                                       id = l.Id,
                                                       name = l.Name,
                                                       level = l.Level,
                                                       parentId = l.ParentId,
                                                       storageType = (int)l.StorageType
                                                   })
                                                   .OrderBy(l => l.level).ThenBy(l => l.name);

                return Json(filteredLocations);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Depolama türü lokasyonları getirilirken hata oluştu: " + ex.Message });
            }
        }

        // Lokasyon arama
        [Authorize(Roles = "Admin,Operator,BasicUser")]
        [HttpGet]
        public async Task<IActionResult> SearchLocations(int warehouseId, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new List<object>());
                }

                var allLocations = await _locationManager.GetLocationsByWarehouseIdAsync(warehouseId);
                var filteredLocations = allLocations.Where(l => l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                                                   .Select(l => new
                                                   {
                                                       id = l.Id,
                                                       name = l.Name,
                                                       level = l.Level,
                                                       parentId = l.ParentId,
                                                       storageType = (int)l.StorageType
                                                   })
                                                   .OrderBy(l => l.level).ThenBy(l => l.name);

                return Json(filteredLocations);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lokasyon arama sırasında hata oluştu: " + ex.Message });
            }
        }

        #region Private Helper Methods

        // Belirli bir lokasyonun tüm alt lokasyonlarını recursive olarak getir
        private async Task<List<int>> GetAllChildrenRecursive(int parentId)
        {
            var allChildren = new List<int>();
            var directChildren = await _locationManager.GetChildrenAsync(parentId);
            
            foreach (var child in directChildren)
            {
                allChildren.Add(child.Id);
                var grandChildren = await GetAllChildrenRecursive(child.Id);
                allChildren.AddRange(grandChildren);
            }
            
            return allChildren;
        }

        #endregion
    }
}