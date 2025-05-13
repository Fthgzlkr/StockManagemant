



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

    // 🔹 Güncelle Butonuna Event Bağla
    document.getElementById("updateProductBtn").addEventListener("click", function () {
        updateProduct();
    });
}


function updateProduct() {
    let formData = new FormData(document.getElementById("editProductForm"));

  
    let productData = Object.fromEntries([...formData.entries()].map(([key, value]) => {
        if (["Price", "CategoryId", "Currency", "Id"].includes(key)) {
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

    // 🔹 Ekle Butonuna Event Bağla
    document.getElementById("createProductBtn").addEventListener("click", function () {
        createProduct();
    });
}


function createProduct() {
    let formData = new FormData(document.getElementById("createProductForm"));

    
    let productData = Object.fromEntries([...formData.entries()].map(([key, value]) => {
        if (["Price", "CategoryId", "Currency", "Id"].includes(key)) {
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

    loadCorridors(warehouseId);

    $("#corridorSelect").on("change", function () {
        const corridorId = $(this).val();
        loadShelves(warehouseId, corridorId);
    });

    $("#shelfSelect").on("change", function () {
        const corridorId = $("#corridorSelect").val();
        const shelfId = $(this).val();
        loadBins(warehouseId, corridorId, shelfId);
    });
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

            
            $("#selectedProductId").val(response.id);

            let productRow = `
                <tr>
                    <td>${response.id}</td>
                    <td>${response.name}</td>
                    <td>${response.categoryName}</td>
                    <td>${response.price} ${currencyLabel}</td>
                </tr>
            `;
            $("#productDetailsTable").html(productRow);
        },
        error: function () {
            $("#selectedProductId").val(""); // Hata durumunda sıfırla
            $("#productDetailsTable").html('<tr><td colspan="4" class="text-danger">Ürün bulunamadı!</td></tr>');
        }
    });
}

function addProductToWarehouse(warehouseId) {
    let productId = $("#selectedProductId").val();
    let stockQuantity = $("#stockQuantityInput").val();
    let stockCode = $("#stockCodeInput").val();

    let binId = $("#binSelect").val();
    let shelfId = $("#shelfSelect").val();
    let corridorId = $("#corridorSelect").val();

    // Hiyerarşi: bin > shelf > corridor
    let locationId = binId || shelfId || corridorId;

    if (!productId || !stockQuantity || !locationId) {
        alert("Lütfen tüm alanları doldurun.");
        return;
    }

    let productData = {
        productId: parseInt(productId),
        warehouseId: parseInt(warehouseId),
        stockQuantity: parseInt(stockQuantity),
        warehouseLocationId: parseInt(locationId),
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

function loadCorridors(warehouseId) {
    $.get(`/WarehouseLocation/GetCorridors?warehouseId=${warehouseId}`, function (data) {
        const select = $("#corridorSelect");
        select.empty().append('<option value="">Koridor Seçin</option>');
        data.forEach(item => {
            select.append(`<option value="${item.id}" data-name="${item.name}">${item.name}</option>`);
        });
    });
}

function loadShelves(warehouseId) {
    const corridorName = $("#corridorSelect option:selected").data("name");
    if (!corridorName) return;

    $.get(`/WarehouseLocation/GetShelves?warehouseId=${warehouseId}&corridor=${corridorName}`, function (data) {
        const select = $("#shelfSelect");
        $("#shelfSelectContainer").show();
        select.empty().append('<option value="">Raf Seçin</option>');
        data.forEach(item => {
            select.append(`<option value="${item.id}" data-name="${item.name}">${item.name}</option>`);
        });
    });
}

function loadBins(warehouseId) {
    const corridorName = $("#corridorSelect option:selected").data("name");
    const shelfName = $("#shelfSelect option:selected").data("name");
    if (!corridorName || !shelfName) return;

    $.get(`/WarehouseLocation/GetBins?warehouseId=${warehouseId}&corridor=${corridorName}&shelf=${shelfName}`, function (data) {
        const select = $("#binSelect");
        $("#binSelectContainer").show();
        select.empty().append('<option value="">Göz Seçin</option>');
        data.forEach(item => {
            select.append(`<option value="${item.id}" data-name="${item.name}">${item.name}</option>`);
        });
    });
}

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
