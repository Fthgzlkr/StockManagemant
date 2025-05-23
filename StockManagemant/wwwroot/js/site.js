



/**
 * Genel Modal Açma Fonksiyonu
 * @param {string} url - Modal içeriğinin yükleneceği URL.
 * @param {string} title - Modal başlığı.
 * @param {function} pageEventBinder - Sayfaya özel eventleri bağlamak için opsiyonel fonksiyon.
 */
function openModal(url, title, pageEventBinder = null) {
    // Modal başlığını güncelle
    const modalTitle = document.getElementById('generalModalLabel');
    modalTitle.innerText = title;

    // Modal içeriğini yükleyeceğimiz alan
    const modalContent = document.getElementById('modalContent');

    // Modal'ı aç
    const modal = new bootstrap.Modal(document.getElementById('generalModal'));
    modal.show();

    // AJAX ile içeriği yükle
    fetch(url)
        .then(response => response.text())
        .then(data => {
            modalContent.innerHTML = data;



            // Sayfaya özel eventleri bağla
            if (typeof pageEventBinder === "function") {
                pageEventBinder();
            }
        })
        .catch(err => {
            modalContent.innerHTML = `<p class="text-danger">İçerik yüklenirken hata oluştu.</p>`;
            console.error(err);
        });
}










//Ürün güncelleme fonksiyonları için eventler
function setupEditProductEvents() {
    console.log("🟢 Ürün düzenleme modalı açıldı, eventler bağlanıyor...");

    // 🔹 Kategorileri yükle
    fetch('/Product/GetCategoryDropdown')
        .then(response => response.json())
        .then(categories => {
            let categoryDropdown = document.getElementById("productCategory");
            categoryDropdown.innerHTML = ""; // Önce temizle

            for (const [id, name] of Object.entries(categories)) {
                let option = document.createElement("option");
                option.value = id;
                option.textContent = name;

                // Mevcut kategori seçili gelsin
                if (id == categoryDropdown.getAttribute("data-selected")) {
                    option.selected = true;
                }

                categoryDropdown.appendChild(option);
            }
        })
        .catch(error => console.error("Kategori yüklenirken hata oluştu:", error));

    // 🔹 Depolama Türlerini Yükle
    fetch('/Product/GetStorageTypeOptions')
        .then(response => response.json())
        .then(storageTypes => {
            let storageDropdown = document.getElementById("productStorageType");
            storageDropdown.innerHTML = "";

            for (const [id, name] of Object.entries(storageTypes)) {
                let option = document.createElement("option");
                option.value = id;
                option.textContent = name;

                if (id == storageDropdown.getAttribute("data-selected")) {
                    option.selected = true;
                }

                storageDropdown.appendChild(option);
            }
        })
        .catch(error => console.error("Depolama türleri yüklenirken hata oluştu:", error));

    // 🔹 Güncelle Butonuna Event Bağla
    document.getElementById("updateProductBtn").addEventListener("click", function () {
        updateProduct();
    });
}


function updateProduct() {
    let formData = new FormData(document.getElementById("editProductForm"));

  
    let productData = Object.fromEntries([...formData.entries()].map(([key, value]) => {
        if (["Price", "CategoryId", "Currency", "Id", "StorageType"].includes(key)) {
            return [key, Number(value)];
        }
        return [key, value]; // string kalsın
    }));

    fetch('/Product/Edit', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(data => {
            debugger;

            if (data.success) {
                alert("Ürün başarıyla güncellendi!");

                
                $('#generalModal').modal('hide');

                
                location.reload();
            } else {
                alert("Hata: " + (data.message || "Bilinmeyen bir hata oluştu."));
            }
        })
        .catch(error => {
            debugger;
            console.error("Güncelleme hatası:", error);
            alert("Ürün güncellenirken bir hata oluştu!");
        });
}




function setupCreateProductEvents() {
    console.log("🟢 Ürün ekleme modalı açıldı, eventler bağlanıyor...");

    // 🔹 Kategorileri yükle
    fetch('/Product/GetCategoryDropdown')
        .then(response => response.json())
        .then(categories => {
            let categoryDropdown = document.getElementById("productCategory");
            categoryDropdown.innerHTML = ""; // Önce temizle

            for (const [id, name] of Object.entries(categories)) {
                let option = document.createElement("option");
                option.value = id;
                option.textContent = name;
                categoryDropdown.appendChild(option);
            }
        })
        .catch(error => console.error("Kategori yüklenirken hata oluştu:", error));

    // 🔹 Depolama Türlerini yükle
    fetch('/Product/GetStorageTypeOptions')
        .then(response => response.json())
        .then(storageTypes => {
            let storageDropdown = document.getElementById("productStorageType");
            storageDropdown.innerHTML = "";

            for (const [id, name] of Object.entries(storageTypes)) {
                let option = document.createElement("option");
                option.value = id;
                option.textContent = name;
                storageDropdown.appendChild(option);
            }
        })
        .catch(error => console.error("Depolama türleri yüklenirken hata oluştu:", error));

    // 🔹 Ekle Butonuna Event Bağla
    document.getElementById("createProductBtn").addEventListener("click", function () {
        createProduct();
    });
}


function createProduct() {
    let formData = new FormData(document.getElementById("createProductForm"));

    
    let productData = Object.fromEntries([...formData.entries()].map(([key, value]) => {
        if (["Price", "CategoryId", "Currency", "Id", "StorageType"].includes(key)) {
            return [key, Number(value)];
        }
        return [key, value];
    }));

    fetch('/Product/Create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Ürün başarıyla eklendi!");

                
                $('#generalModal').modal('hide');


                location.reload();
            } else {
                alert("Hata: " + (data.message || "Bilinmeyen bir hata oluştu."));
            }
        })
        .catch(error => {
            console.error("Ürün ekleme hatası:", error);
            alert("Ürün eklenirken bir hata oluştu!");
        });
}




function setupAddProductEvents() {
    console.log("🟢 Ürün depoya ekleme modalı açıldı, eventler bağlanıyor...");

    const urlParams = new URLSearchParams(window.location.search);
    const warehouseId = urlParams.get("warehouseId");

    if (!warehouseId) {
        console.error("⚠️ Hata: Depo ID bulunamadı!");
        $("#stockMessage").html('<div class="alert alert-danger">Hata: Depo ID bulunamadı!</div>');
        return;
    }

    console.log(`📦 Depo ID bulundu: ${warehouseId}`);
    $("#warehouseIdInput").val(warehouseId).prop("disabled", true);

    $("#searchProductBtn").off("click").on("click", searchProductByBarcode);

    $("#addStockBtn").off("click").on("click", function () {
        addProductToWarehouse(warehouseId);
    });

    // Yeni dinamik lokasyon yükleme
    loadDynamicLocations(warehouseId);
}

function searchProductByBarcode() {
    let barcode = $("#productIdInput").val().trim();
    
    if (!barcode) {
        alert("Lütfen bir ürün barkodu giriniz.");
        return;
    }

    $.ajax({
        url: `/Product/GetProductByBarcode?barcode=${barcode}`,
        type: "GET",
        success: function (response) {
            let currencyLabel = response.currency === 0 ? "TL" : "USD";
            
            // StorageType mapping
            const storageTypeNames = {
                1: 'Tanımsız',
                2: 'Soğuk Depolama', 
                3: 'Yanıcı',
                4: 'Kırılgan',
                5: 'Standart',
                6: 'Nem Korumalı'
            };
            
            $("#selectedProductId").val(response.id);
            $("#selectedProductStorageType").val(response.storageType || 5); // Default standart

            let productRow = `
                <tr>
                    <td>${response.id}</td>
                    <td>${response.name}</td>
                    <td>${response.categoryName}</td>
                    <td>${response.price} ${currencyLabel}</td>
                    <td><span class="badge bg-info">${storageTypeNames[response.storageType] || 'Standart'}</span></td>
                </tr>
            `;
            $("#productDetailsTable").html(productRow);
            
            // Lokasyon seçimlerini kontrol et (eğer ürün seçildikten sonra lokasyon filtrelemek istiyorsak)
            filterLocationsByStorageType();
        },
        error: function () {
            $("#selectedProductId").val("");
            $("#selectedProductStorageType").val("");
            $("#productDetailsTable").html('<tr><td colspan="5" class="text-danger">Ürün bulunamadı!</td></tr>');
        }
    });
}

function addProductToWarehouse(warehouseId) {
    let productId = $("#selectedProductId").val();
    let stockQuantity = $("#stockQuantityInput").val();
    let stockCode = $("#stockCodeInput").val();
    let locationId = $("#selectedLocationId").val();

    if (!productId || !stockQuantity || !locationId) {
        alert("Lütfen tüm alanları doldurun.");
        return;
    }

    // StorageType uyumluluğu kontrolü
    if (!validateStorageTypeCompatibility()) {
        return;
    }

    let productData = {
        productId: parseInt(productId),
        warehouseId: parseInt(warehouseId),
        warehouseLocationId: parseInt(locationId),
        stockQuantity: parseInt(stockQuantity),
        stockCode: stockCode 
    };

    $.ajax({
        url: `/WarehouseProduct/AddProductToWarehouse`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(productData),
        success: function (response) {
            $("#stockMessage").html(`<div class="alert alert-success">${response.message}</div>`);
            setTimeout(() => {
                $('#generalModal').modal('hide');
                location.reload();
            }, 1000);
        },
        error: function () {
            $("#stockMessage").html('<div class="alert alert-danger">Stok eklenirken hata oluştu!</div>');
        }
    });
}

// Dinamik lokasyon yükleme
function loadDynamicLocations(warehouseId) {
    console.log(`📍 Depo ${warehouseId} için lokasyonlar yükleniyor...`);
    
    $.ajax({
        url: `/WarehouseLocation/GetLocationsByWarehouseId?warehouseId=${warehouseId}`,
        type: "GET",
        success: function (locations) {
            window.allLocations = locations; // Global değişkende sakla
            createLocationLevel(1, null); // İlk seviyeyi oluştur
        },
        error: function () {
            $("#dynamicLocationSelectors").html('<div class="alert alert-danger">Lokasyonlar yüklenemedi!</div>');
        }
    });
}

// Belirli seviye için dropdown oluştur
function createLocationLevel(level, parentId) {
    const containerSelector = "#dynamicLocationSelectors";
    
    // Bu seviyedeki lokasyonları filtrele
    let levelLocations;
    if (parentId === null) {
        // İlk seviye: parent'ı olmayan lokasyonlar
        levelLocations = window.allLocations.filter(loc => !loc.parentId);
    } else {
        // Alt seviyeler: belirtilen parent'ın children'ları
        levelLocations = window.allLocations.filter(loc => loc.parentId === parentId);
    }

    if (levelLocations.length === 0) {
        return; // Bu seviyede lokasyon yoksa dropdown oluşturma
    }

    // Dropdown HTML'i oluştur
    const dropdownId = `locationSelect_${level}`;
    const dropdownHtml = `
        <div class="mb-2" id="locationContainer_${level}">
            <label for="${dropdownId}" class="form-label">Seviye ${level}:</label>
            <select id="${dropdownId}" class="form-select" data-level="${level}">
                <option value="">Seçin...</option>
            </select>
        </div>
    `;
    
    $(containerSelector).append(dropdownHtml);
    
    // Dropdown'ı doldur
    const select = $(`#${dropdownId}`);
    levelLocations.forEach(location => {
        const storageTypeIcon = getStorageTypeIcon(location.storageType);
        select.append(`
            <option value="${location.id}" 
                    data-name="${location.name}" 
                    data-storage-type="${location.storageType}"
                    data-has-children="${location.hasChildren}">
                ${storageTypeIcon} ${location.name}
            </option>
        `);
    });
    
    // Event listener ekle
    select.on('change', function() {
        const selectedId = parseInt($(this).val());
        const selectedOption = $(this).find('option:selected');
        const hasChildren = selectedOption.data('has-children');
        
        if (selectedId) {
            // Seçilen lokasyonu kaydet
            $("#selectedLocationId").val(selectedId);
            
            // Alt seviyeleri temizle
            clearLowerLevels(level);
            
            // Breadcrumb güncelle
            updateLocationBreadcrumb();
            
            // Alt seviye varsa dropdown oluştur
            if (hasChildren) {
                createLocationLevel(level + 1, selectedId);
            }
            
            // StorageType kontrolü
            validateStorageTypeCompatibility();
        } else {
            // Seçim temizlendi
            $("#selectedLocationId").val('');
            clearLowerLevels(level);
            hideLocationPath();
        }
    });
}

// Alt seviyeleri temizle
function clearLowerLevels(fromLevel) {
    let levelToRemove = fromLevel + 1;
    while ($(`#locationContainer_${levelToRemove}`).length > 0) {
        $(`#locationContainer_${levelToRemove}`).remove();
        levelToRemove++;
    }
}

// Lokasyon yolunu (breadcrumb) güncelle
function updateLocationBreadcrumb() {
    let breadcrumb = [];
    let level = 1;
    
    while ($(`#locationSelect_${level}`).length > 0) {
        const select = $(`#locationSelect_${level}`);
        const selectedOption = select.find('option:selected');
        const selectedName = selectedOption.data('name');
        
        if (selectedName) {
            breadcrumb.push(selectedName);
        }
        level++;
    }
    
    if (breadcrumb.length > 0) {
        $("#locationBreadcrumb").text(breadcrumb.join(' > '));
        $("#selectedLocationPath").show();
    } else {
        hideLocationPath();
    }
}

function hideLocationPath() {
    $("#selectedLocationPath").hide();
    $("#storageTypeWarning").hide();
}

// StorageType uyumluluğunu kontrol et
function validateStorageTypeCompatibility() {
    const productStorageType = parseInt($("#selectedProductStorageType").val());
    const selectedLocationId = $("#selectedLocationId").val();
    
    if (!productStorageType || !selectedLocationId) {
        $("#storageTypeWarning").hide();
        return true;
    }
    
    // Seçilen lokasyonun StorageType'ını bul
    const selectedLocation = window.allLocations.find(loc => loc.id == selectedLocationId);
    if (!selectedLocation) {
        return true;
    }
    
    const locationStorageType = selectedLocation.storageType;
    
    // StorageType isimleri
    const storageTypeNames = {
        1: 'Tanımsız',
        2: 'Soğuk Depolama',
        3: 'Yanıcı', 
        4: 'Kırılgan',
        5: 'Standart',
        6: 'Nem Korumalı'
    };
    
    // Uyumluluk kontrolü (Standart ve Tanımsız her yerde saklanabilir)
    const isCompatible = 
        productStorageType === locationStorageType || 
        productStorageType === 5 || // Standart ürün
        productStorageType === 1 || // Tanımsız ürün
        locationStorageType === 5 || // Standart lokasyon
        locationStorageType === 1;   // Tanımsız lokasyon
    
    if (!isCompatible) {
        const warningText = `Ürün depolama türü (${storageTypeNames[productStorageType]}) ile lokasyon türü (${storageTypeNames[locationStorageType]}) uyumlu değil!`;
        $("#storageWarningText").text(warningText);
        $("#storageTypeWarning").show();
        return false;
    } else {
        $("#storageTypeWarning").hide();
        return true;
    }
}

// Lokasyon seçimlerini StorageType'a göre filtrele (opsiyonel)
function filterLocationsByStorageType() {
    // Bu fonksiyon ürün seçildikten sonra sadece uyumlu lokasyonları göstermek için kullanılabilir
    // Şimdilik boş bırakıyoruz, ihtiyaç halinde implement edilebilir
}

// StorageType'a göre ikon al
function getStorageTypeIcon(storageType) {
    const icons = {
        1: '❓', // Tanımsız
        2: '❄️', // Soğuk Depolama
        3: '🔥', // Yanıcı
        4: '📦', // Kırılgan
        5: '📋', // Standart
        6: '💧'  // Nem Korumalı
    };
    return icons[storageType] || '📋';
}

// Diğer fonksiyonlar değişmeden kalıyor...
function SetupProductDetail(productId) {
    const modal = document.getElementById("generalModal");
    if (!modal || !productId || isNaN(productId)) {
        modal.querySelector("#loadingText").innerText = "Geçersiz ürün ID.";
        return;
    }

    fetch(`/Product/GetProductById?id=${productId}`)
        .then(res => {
            if (!res.ok) throw new Error("Ürün bulunamadı");
            return res.json();
        })
        .then(product => {
            modal.querySelector("#productName").innerText = product.name || "—";
            modal.querySelector("#productCategory").innerText = product.categoryName || "—";
            modal.querySelector("#productPrice").innerText = (product.price || 0) + " " + (product.currency === 1 ? "USD" : "TL");
            modal.querySelector("#productBarcode").innerText = product.barcode || "Yok";
            modal.querySelector("#productCurrency").innerText = product.currency === 1 ? "USD" : "TL";
            modal.querySelector("#productDescription").innerText = product.description || "Açıklama girilmemiş";

            const imageUrl = product.imageUrl ? `/uploads/${product.imageUrl}` : "/uploads/default-placeholder.png";
            modal.querySelector("#productImage").src = imageUrl;

            modal.querySelector("#loadingText").style.display = "none";
            modal.querySelector("#productDetailContainer").style.display = "flex";
        })
        .catch(err => {
            modal.querySelector("#loadingText").innerText = "Ürün bulunamadı.";
            console.error("Detay yüklenirken hata:", err);
        });
}

function setupCreateCustomerEvents() {
    $("#saveCustomerBtn").click(function () {
        const data = {
            name: $("#customerName").val(),
            phone: $("#customerPhone").val(),
            email: $("#customerEmail").val(),
            address: $("#customerAddress").val()
        };

        $.ajax({
            url: '/Customer/Create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    Swal.fire("Başarılı", response.message, "success");
                    $("#generalModal").modal('hide');
                    $("#customerGrid").trigger("reloadGrid");
                }
            }
        });
    });
}

function setupEditCustomerEvents() {
    $("#saveCustomerBtn").click(function () {
        const data = {
            id: $("#customerId").val(),
            name: $("#customerName").val(),
            phone: $("#customerPhone").val(),
            email: $("#customerEmail").val(),
            address: $("#customerAddress").val()
        };

        $.ajax({
            url: '/Customer/Edit',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    Swal.fire("Başarılı", response.message, "success");
                    $("#generalModal").modal('hide');
                    $("#customerGrid").trigger("reloadGrid");
                }
            }
        });
    });
}

// SAYFALARI MODAL OLARAK BUTONLAR SAYESİNDE AÇMAYA YARAYAN KISIM







//Ürün Edit 
$(document).on('click', '.openProductEditModal', function () {
    const productId = $(this).data('id'); // Tıklanan ikona ait product ID'yi al

    if (productId) {
        const title = 'Ürün Düzenle'; // Modal başlığı
        openModal(`/Product/Edit/${productId}`, title, setupEditProductEvents); // openModal fonksiyonunu çağır
    } else {
        alert('Lütfen bir ürün seçin.'); // Eğer ürün seçilmemişse uyarı göster
    }
});


$(document).on('click', '.openProductCreateModal', function () {


    const title = 'Ürün Oluştur';  // Başlık
    openModal(`/Product/Create`, title, setupCreateProductEvents);  // Modalı aç ve içeriği yükle
});




document.querySelectorAll('.open-category-edit-modal').forEach(button => {
    button.addEventListener('click', function () {

        const title = 'Kategori Düzenle';  // Başlık
        openModal(`/Categories/Edit`, title);  // Modalı aç ve içeriği yükle
    });
});


$("#addCustomerBtn").click(function () {
    openModal("/Customer/Create", "Yeni Müşteri", setupCreateCustomerEvents);
});

// Düzenleme
$(document).on("click", ".editCustomerBtn", function () {
    const id = $(this).data("id");
    openModal(`/Customer/Edit/${id}`, "Müşteri Düzenle", setupEditCustomerEvents);
});


$(document).on('click', '.openProductaddModal', function () {
    const title = 'Ürünü Depoya Ekle';
    openModal(`/WarehouseProduct/AddProduct`, title, setupAddProductEvents);
});
