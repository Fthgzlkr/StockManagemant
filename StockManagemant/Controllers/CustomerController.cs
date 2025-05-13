using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.Business.Managers;
using StockManagemant.Entities.DTO;

namespace StockManagemant.Controllers
{
    [Authorize] // 🔒 Öncelikle tüm controller için login zorunluluğu
    [Authorize(Roles = "Admin,Operator")] // 🔒 Admin ve Operator yetkililer görebilir
    public class CustomerController : Controller
    {
        private readonly ICustomerManager _customerManager;

        public CustomerController(ICustomerManager customerManager)
        {
            _customerManager = customerManager;
        }

        // 🔹 Ana Sayfa (Grid yüklemesi için View)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 🔹 Grid verisini çekme (JSON Data)
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerManager.GetAllCustomersAsync();
            return Json(new
            {
                rows = customers
            });
        }

        // 🔹 Yeni Müşteri Ekleme (Form gösterimi - Modal içinde açılacak View)
        [Authorize(Roles = "Admin,Operator")] // 🔒 Ekleme sadece Admin, Operator
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomersDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _customerManager.AddCustomerAsync(customerDto);
            return Json(new { success = true, message = "Müşteri başarıyla eklendi." });
        }

        // 🔹 Müşteri Düzenleme (Form gösterimi - Modal içinde açılacak View)
        [Authorize(Roles = "Admin,Operator")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerManager.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [Authorize(Roles = "Admin,Operator")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CustomersDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _customerManager.UpdateCustomerAsync(customerDto);
            return Json(new { success = true, message = "Müşteri başarıyla güncellendi." });
        }

        // 🔹 Silme İşlemi
        [Authorize(Roles = "Admin,Operator")] // 🔒 Silme sadece yetkili roller
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerManager.DeleteCustomerAsync(id);
            return Json(new { success = true, message = "Müşteri başarıyla silindi." });
        }
    }
}