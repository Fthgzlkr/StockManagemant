// Mobile App Global Variables
let currentProduct = null;

// Tab değiştirme
function showTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    
    document.getElementById(tabName + '-tab').classList.add('active');
    document.querySelector(`[onclick="showTab('${tabName}')"]`).classList.add('active');
}

// Barkod tarama (şimdilik simüle)
function openScanner(target) {
    const fakeBarcode = prompt('Barkod girin (test):');
    if (fakeBarcode) {
        if (target === 'search') {
            document.getElementById('search-barcode').value = fakeBarcode;
        } else if (target === 'add') {
            document.getElementById('new-barcode').value = fakeBarcode;
        }
    }
}

// Kategorileri yükle
async function loadCategories() {
    try {
        const response = await fetch('/Product/GetCategoryDropdown');
        if (response.ok) {
            const categories = await response.json();
            const select = document.getElementById('new-category');
            
            if (select) {
                Object.entries(categories).forEach(([id, name]) => {
                    const option = document.createElement('option');
                    option.value = id;
                    option.textContent = name;
                    select.appendChild(option);
                });
            }
        }
    } catch (error) {
        console.error('Kategoriler yüklenemedi:', error);
    }
}

// Storage Type'ları yükle
async function loadStorageTypes() {
    try {
        const response = await fetch('/Product/GetStorageTypeOptions');
        if (response.ok) {
            const storageTypes = await response.json();
            const select = document.getElementById('new-storage-type');
            
            if (select) {
                Object.entries(storageTypes).forEach(([id, name]) => {
                    const option = document.createElement('option');
                    option.value = id;
                    option.textContent = name;
                    select.appendChild(option);
                });
            }
        }
    } catch (error) {
        console.error('Depolama tipleri yüklenemedi:', error);
    }
}

// Ürün arama
async function searchProduct() {
    const barcode = document.getElementById('search-barcode').value.trim();
    if (!barcode) {
        showMessage('Lütfen barkod girin', 'error');
        return;
    }
    
    showLoading(true);
    try {
        const response = await fetch(`/Product/GetProductByBarcode?barcode=${encodeURIComponent(barcode)}`);
        
        if (response.ok) {
            const product = await response.json();
            displayProduct(product);
            currentProduct = product;
            showMessage('Ürün bulundu!', 'success');
        } else if (response.status === 404) {
            showMessage('Ürün bulunamadı', 'error');
            hideProductResult();
        } else {
            showMessage('Arama hatası: ' + response.status, 'error');
            hideProductResult();
        }
    } catch (error) {
        showMessage('Ağ hatası: ' + error.message, 'error');
        hideProductResult();
    }
    showLoading(false);
}

// Ürün detaylarını göster
function displayProduct(product) {
    const resultDiv = document.getElementById('product-result');
    if (!resultDiv) return;
    
    resultDiv.innerHTML = `
        <div class="product-info">
            <h4>📦 ${product.name || 'İsimsiz Ürün'}</h4>
            <div class="product-details">
                <p><strong>🏷️ Kategori:</strong> ${product.categoryName || 'Belirtilmemiş'}</p>
                <p><strong>📊 Barkod:</strong> ${product.barcode}</p>
                <p><strong>💰 Fiyat:</strong> ${product.price || 0} ${product.currency || 'TRY'}</p>
                <p><strong>📦 Depolama:</strong> ${product.storageTypeName || 'Standart'}</p>
                ${product.imageUrl ? `<img src="${product.imageUrl}" alt="Ürün resmi" class="product-image">` : ''}
                ${product.description ? `<p><strong>📄 Açıklama:</strong> ${product.description}</p>` : ''}
            </div>
            <div class="product-actions">
                <button class="btn btn-warning" onclick="editProduct()">✏️ Düzenle</button>
            </div>
        </div>
    `;
    resultDiv.classList.remove('hidden');
}

// Ürün düzenleme
function editProduct() {
    if (!currentProduct) return;
    alert('Ürün düzenleme özelliği yakında eklenecek!');
}

// Ürün sonucunu gizle
function hideProductResult() {
    const resultDiv = document.getElementById('product-result');
    if (resultDiv) {
        resultDiv.classList.add('hidden');
    }
    currentProduct = null;
}

// Yeni ürün ekleme form handler
// Yeni ürün ekleme form handler - DÜZELTİLDİ
function initializeProductForm() {
    const form = document.getElementById('add-product-form');
    if (!form) return;
    
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Currency ve StorageType sayı olarak gönderilmeli
        const productData = {
            barcode: document.getElementById('new-barcode').value.trim(),
            name: document.getElementById('new-name').value.trim(),
            categoryId: parseInt(document.getElementById('new-category').value),
            price: parseFloat(document.getElementById('new-price').value),
            currency: parseInt(document.getElementById('new-currency').value), // Sayı olarak
            storageType: parseInt(document.getElementById('new-storage-type').value),
            imageUrl: document.getElementById('new-image-url').value.trim() || null,
            description: document.getElementById('new-description').value.trim() || null
        };
        
        // Validation
        if (!productData.barcode || !productData.name || !productData.categoryId || 
            isNaN(productData.price) || isNaN(productData.currency)) {
            showMessage('Lütfen tüm zorunlu alanları doldurun', 'error');
            return;
        }
        
        console.log('Gönderilen veri:', productData); // Debug için
        
        showLoading(true);
        try {
            const response = await fetch('/Product/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(productData)
            });
            
            const result = await response.json();
            console.log('API yanıtı:', result); // Debug için
            
            if (result.success) {
                showMessage('Ürün başarıyla eklendi!', 'success');
                form.reset();
            } else {
                const errorMessage = result.message || 
                                   (result.errors ? 
                                    Object.entries(result.errors)
                                          .map(([key, value]) => `${key}: ${value}`)
                                          .join(', ') : 'Bilinmeyen hata');
                showMessage('Ekleme hatası: ' + errorMessage, 'error');
            }
        } catch (error) {
            console.error('Ağ hatası:', error);
            showMessage('Ağ hatası: ' + error.message, 'error');
        }
        showLoading(false);
    });
}

// Yardımcı fonksiyonlar
function showLoading(show) {
    const loading = document.getElementById('loading');
    if (loading) {
        if (show) {
            loading.classList.remove('hidden');
        } else {
            loading.classList.add('hidden');
        }
    }
}

function showMessage(text, type) {
    const messageDiv = document.getElementById('message');
    if (messageDiv) {
        messageDiv.textContent = text;
        messageDiv.className = `message ${type}`;
        messageDiv.classList.remove('hidden');
        
        setTimeout(() => {
            messageDiv.classList.add('hidden');
        }, 3000);
    }
}

// Sayfa başlatma
function initializePage() {
    // Eğer Products sayfasındaysak
    if (document.getElementById('add-product-form')) {
        loadCategories();
        loadStorageTypes();
        initializeProductForm();
    }
}

// Document ready
document.addEventListener('DOMContentLoaded', function() {
    initializePage();
});

// Ana menü fonksiyonları (Index sayfası için)
function logout() {
    if (confirm('Çıkış yapmak istediğinizden emin misiniz?')) {
        window.location.href = '/Mobile/Logout';
    }
}

function openBarcodeScanner() {
    alert('Barkod tarama özelliği yakında eklenecek! 📷');
}

function showReports() {
    alert('Raporlar sayfası yakında hazır olacak! 📊');
}

function quickStockUpdate() {
    alert('Hızlı stok güncelleme özelliği geliştiriliyor! 📝');
}

function quickProductAdd() {
    window.location.href = '/Mobile/Products';
}

function quickSearch() {
    alert('Hızlı arama özelliği yakında! 🔍');
}

// Test fonksiyonları (Index sayfası için)
function testBasic() {
    const resultDiv = document.getElementById('result');
    if (resultDiv) {
        resultDiv.innerHTML = 
            '✅ JavaScript çalışıyor!<br>' +
            'Tarih: ' + new Date().toLocaleString('tr-TR') + '<br>' +
            'Ekran: ' + window.innerWidth + 'x' + window.innerHeight;
    }
}

async function testAPI() {
    const resultDiv = document.getElementById('result');
    if (!resultDiv) return;
    
    resultDiv.innerHTML = '🔄 API test ediliyor...';
    
    try {
        const response = await fetch('/Product/GetCategoryDropdown');
        
        if (response.ok) {
            const data = await response.json();
            resultDiv.innerHTML = 
                '✅ API çalışıyor!<br>' +
                'Durum: ' + response.status + '<br>' +
                'Kategori sayısı: ' + Object.keys(data).length;
        } else {
            resultDiv.innerHTML = 
                '❌ API Hatası: ' + response.status;
        }
    } catch (error) {
        resultDiv.innerHTML = 
            '❌ Bağlantı Hatası: ' + error.message;
    }
}