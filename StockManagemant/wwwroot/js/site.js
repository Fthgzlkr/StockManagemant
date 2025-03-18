



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



//Fiş ürünlerini göstermek için eventler
function setupReceiptProductsEvents() {
    console.log("🟢 Fiş Ürünleri eventleri bağlanıyor...");

    $(document).off('click', '#showProductsBtn').on('click', '#showProductsBtn', function () {
        let receiptId = $(this).data("receipt-id");

        if ($("#productsGrid").children().length === 0) {
            $("#productsGrid").jqGrid({
                url: '/Receipt/GetReceiptDetails?receiptId=' + receiptId,
                datatype: "json",
                mtype: "GET",
                colNames: ['Detay ID', 'Ürün ID', 'Ürün Adı', 'Miktar', 'Birim Fiyat', 'Ara Toplam', 'Actions'],
                colModel: [
                    { name: 'id', index: 'id', key: true, width: 80, align: "center", hidden: true },
                    { name: 'productId', index: 'productId', width: 80, align: "center", hidden: true },
                    { name: 'productName', index: 'productName', width: 200, align: "left" },
                    { name: 'quantity', index: 'quantity', width: 100, align: "center", editable: true },
                    { name: 'unitPrice', index: 'unitPrice', width: 120, align: "right" },
                    { name: 'subTotal', index: 'subTotal', width: 120, align: "right" },
                    {
                        name: "actions",
                        label: "Actions",
                        align: "center",
                        formatter: function (cellvalue, options, rowObject) {
                            return `
                            <div class="action-icons">
                                <i class="fa-solid fa-pen text-primary fa-xl edit-btn"
                                    data-id="${rowObject.id}" title="Düzenle"></i>
                                <i class="fa-solid fa-trash text-danger fa-xl delete-btn"
                                    data-id="${rowObject.id}" title="Sil"></i>
                            </div>
                            `;
                        }
                    }
                ],

                pager: "#pagerProducts",
                guiStyle: "bootstrap4",
                viewrecords: true,
                height: 'auto',
                autowidth: true,
                shrinkToFit: true,
                caption: "Fiş Ürünleri",
                footerrow: true,
                loadance: false,
                userDataOnFooter: true,

                loadComplete: function () {
                    console.log("JQGrid yüklendi, butonları eklemeye çalışıyoruz...");

                    let totalAmount = $("#showProductsBtn").data("total-amount");
                    $("#productsGrid").jqGrid('footerData', 'set', {
                        productName: "Toplam:",
                        subTotal: totalAmount
                    });

                    $("#productsGrid").jqGrid('navGrid', '#pagerProducts',
                        { edit: false, add: false, del: false, search: false, refresh: true }
                    );

                    // ✅ Eventleri bağla (modal açıldığında tekrar çalışsın)
                    bindProductGridEvents();
                },

                loadError: function (xhr, status, error) {
                    console.error("jqGrid yüklenirken hata oluştu:", error);
                }
            });

        } else {
            $("#productsGrid").trigger("reloadGrid");
        }
    });
}

// ✅ jqGrid içindeki Edit ve Delete butonlarını bağlayan fonksiyon
function bindProductGridEvents() {
    console.log("🟡 Düzenleme ve Silme butonları tekrar bağlanıyor...");

    // 🗑 Silme Butonu (Swal2 ile)
    $(document).off("click", ".delete-btn").on("click", ".delete-btn", function () {
        let receiptDetailId = $(this).data("id");

        Swal.fire({
            title: "Silme İşlemi",
            text: "Bu ürünü fişten çıkarmak istediğinize emin misiniz?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Evet, Sil!",
            cancelButtonText: "Hayır, İptal"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/Receipt/RemoveProductFromReceipt",
                    type: "POST",
                    data: { receiptDetailId: receiptDetailId },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: "Başarılı!",
                                text: "Ürün fişten çıkarıldı.",
                                icon: "success",
                                confirmButtonColor: "#3085d6"
                            });
                            $("#productsGrid").trigger("reloadGrid");
                        } else {
                            Swal.fire("Hata!", "Ürün silinemedi!", "error");
                        }
                    },
                    error: function () {
                        Swal.fire("Hata!", "Silme işlemi başarısız oldu!", "error");
                    }
                });
            }
        });
    });

    // 🖊 Düzenleme Butonu (Swal2 ile)
    $(document).off("click", ".edit-btn").on("click", ".edit-btn", function () {
        let receiptDetailId = $(this).data("id");

        Swal.fire({
            title: "Ürünü Düzenle",
            input: "number",
            inputLabel: "Yeni Miktar",
            inputPlaceholder: "Yeni miktarı girin...",
            showCancelButton: true,
            confirmButtonText: "Kaydet",
            cancelButtonText: "İptal",
            inputAttributes: {
                min: 1, // Minimum değer (negatif değerleri engeller)
                step: 1 // Adım aralığı (örn. 1'er 1'er artırma)
            },
            inputValue: 1, // Varsayılan başlangıç değeri
            inputValidator: (value) => {
                if (!value || value <= 0) {
                    return "Geçerli bir miktar giriniz!";
                }
            }
        }).then((result) => {
            if (result.isConfirmed) {
                let newQuantity = result.value;

                $.ajax({
                    url: "/Receipt/UpdateProductQuantityInReceipt",
                    type: "POST",
                    data: { receiptDetailId: receiptDetailId, newQuantity: newQuantity },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: "Güncellendi!",
                                text: "Ürün miktarı başarıyla güncellendi.",
                                icon: "success",
                                timer: 1500,
                                showConfirmButton: false
                            });
                            $("#productsGrid").trigger("reloadGrid");
                        } else {
                            Swal.fire("Hata!", "Ürün güncellenemedi!", "error");
                        }
                    },
                    error: function () {
                        Swal.fire("Hata!", "Ürün güncellenemedi!", "error");
                    }
                });
            }
        });
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
        return (!isNaN(value) && value.trim() !== "") ? [key, Number(value)] : [key, value];
    }));

    fetch('/Product/Edit', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Ürün başarıyla güncellendi!");

                
                $('#generalModal').modal('hide');

                
                location.reload();
            } else {
                alert("Hata: " + (data.message || "Bilinmeyen bir hata oluştu."));
            }
        })
        .catch(error => {
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
     
        if (!isNaN(value) && value.trim() !== "") {
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

    // 🔹 URL'den depo ID'yi al
    const urlParams = new URLSearchParams(window.location.search);
    const warehouseId = urlParams.get("warehouseId");

    if (!warehouseId) {
        console.error("⚠️ Hata: Depo ID bulunamadı!");
        $("#stockMessage").html('<div class="alert alert-danger">Hata: Depo ID bulunamadı!</div>');
        return;
    }

    console.log(`📦 Depo ID bulundu: ${warehouseId}`);

    // 🔹 Depo ID'yi input içine yaz ve değiştirilemez yap
    $("#warehouseIdInput").val(warehouseId).prop("disabled", true);

    // 📌 Ürün Arama Butonuna Event Bağlama
    $("#searchProductBtn").off("click").on("click", searchProductById);

    // 📌 Stok Ekleme Butonuna Event Bağlama
    $("#addStockBtn").off("click").on("click", function () {
        addProductToWarehouse(warehouseId);
    });
}



// İd ye göre ürün arama kısmı
function searchProductById() {
    let productId = $("#productIdInput").val();
    if (!productId) {
        alert("Lütfen bir ürün ID giriniz.");
        return;
    }

    $.ajax({
        url: `/Product/GetProductById?id=${productId}`,
        type: "GET",
        success: function (response) {
            let currencyLabel = response.currency === 0 ? "TL" : "USD";

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
            $("#productDetailsTable").html('<tr><td colspan="4" class="text-danger">Ürün bulunamadı!</td></tr>');
        }
    });
}



// Stok ekleme işlemleri
function addProductToWarehouse(warehouseId) {
    let productId = $("#productIdInput").val();
    let stockQuantity = $("#stockQuantityInput").val();

    if (!productId || !stockQuantity) {
        alert("Lütfen tüm alanları doldurun.");
        return;
    }

    let productData = {
        productId: parseInt(productId),
        warehouseId: parseInt(warehouseId),
        stockQuantity: parseInt(stockQuantity)
    };

    $.ajax({
        url: `/WarehouseProduct/AddProductToWarehouse`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(productData),
        success: function (response) {
            $("#stockMessage").html(`<div class="alert alert-success">${response.message}</div>`);

            // 🔹 Modalı kapat ve sayfayı yenile
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


document.querySelectorAll('.openProductaddModal').forEach(button => {
    button.addEventListener('click', function () {

        const title = 'Depoya Ürün Ekle';  // Başlık
        openModal(`WarehouseProduct/AddProduct`, title);  // Modalı aç ve içeriği yükle
    });
});


$(document).on('click', '.openProductaddModal', function () {
    const title = 'Ürünü Depoya Ekle';
    openModal(`/WarehouseProduct/AddProduct`, title, setupAddProductEvents);
});
