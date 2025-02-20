﻿



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
                    { name: 'id', index: 'id', key: true, width: 80, align: "center",hidden:true },
                    { name: 'productId', index: 'productId', width: 80, align: "center", hidden:true },
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

/**
 * ✅ Ürün Güncelleme Fonksiyonu
 */
function updateProduct() {
    let productData = {
        Id: document.getElementById("productId").value,
        Name: document.getElementById("productName").value,
        Price: parseFloat(document.getElementById("productPrice").value),
        Stock: parseInt(document.getElementById("productStock").value),
        CategoryId: parseInt(document.getElementById("productCategory").value), // Seçilen kategori
        Currency: parseInt(document.getElementById("productCurrency").value) // TL = 0, USD = 1
    };

    fetch('/Product/Edit', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Ürün başarıyla güncellendi!");

                // 🔹 Modalı kapat
                $('#generalModal').modal('hide');

                // 🔹 Sayfayı güncelle (gerekirse listeyi yenilemek için)
                location.reload();
            } else {
                alert("Hata: " + data.message);
            }
        })
        .catch(error => console.error("Güncelleme hatası:", error));
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

/**
 * ✅ Yeni Ürün Ekleme Fonksiyonu
 */
function createProduct() {
    let productData = {
        Name: document.getElementById("productName").value,
        Price: parseFloat(document.getElementById("productPrice").value) || 0,
        Stock: parseInt(document.getElementById("productStock").value) || 0,
        CategoryId: parseInt(document.getElementById("productCategory").value),
        Currency: parseInt(document.getElementById("productCurrency").value) // TL = 0, USD = 1
    };

    fetch('/Product/Create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(productData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("Ürün başarıyla eklendi!");

                // 🔹 Modalı kapat (Eğer modal içinde kullanıyorsan)
                $('#generalModal').modal('hide');

                // 🔹 Sayfayı güncelle (Gerekirse listeyi yenilemek için)
                location.reload();
            } else {
                alert("Hata: " + data.message);
            }
        })
        .catch(error => console.error("Ürün ekleme hatası:", error));
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


//Fiş Oluşturma 
document.querySelectorAll('.open-create-modal').forEach(button => {
    button.addEventListener('click', function () {
       
        const title = 'Fiş Oluştur';  // Başlık
        openModal(`/Receipt/Create`, title);  // Modalı aç ve içeriği yükle
    });
});



document.querySelectorAll('.open-category-edit-modal').forEach(button => {
    button.addEventListener('click', function () {

        const title = 'Kategori Düzenle';  // Başlık
        openModal(`/Categories/Edit`, title);  // Modalı aç ve içeriği yükle
    });
});



