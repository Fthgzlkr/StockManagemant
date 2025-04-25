  $(document).ready(function () {
            let warehouseId;
            
      
            
            if (userRole === "BasicUser") {
                warehouseId = assignedWarehouseId;
                localStorage.setItem("selectedWarehouseId", warehouseId);
            } else {
                warehouseId = localStorage.getItem("selectedWarehouseId") || 1;
            }
            
            // Stok değişimlerini izleyen nesne
            let stockChanges = {};
                
            let currencyDropdown = { "TL": "Türk Lirası", "USD": "Dolar" };

       // jqGrid Başlat
       $("#warehouseProductGrid").jqGrid({
           url: `/WarehouseProduct/GetWarehouseProducts?warehouseId=${warehouseId}`,
           datatype: "json",
           mtype: "GET",
           colNames: ['ID','ProductId', 'Barkod','Stok Kodu','Ürün Adı', 'Kategori', 'Fiyat', 'Para Birimi', 'Stok', ' Depo Lokasyonu', 'İşlemler'],
           colModel: [
               { name: 'id', index: 'id', align: "center", sortable: true, key: true, hidden: true },
               { name: 'product_id', index: 'product_id', align: "center", sortable: true, key: true, hidden:true },
               { name: 'barcode', index: 'barcode', align: "center", sortable: true, hidden:false },
               { name: 'stockcode', index: ' stockcode', align: "center", sortable: true, hidden:false },
               { name: 'name', index: 'name', sortable: true, editable: false },
               { name: 'category', index: 'category', sortable: true, editable: false },
               { name: 'price', index: 'price', align: "center", sortable: true, editable: false, formatter: formatPrice },
               { name: 'currencyType', index: 'currencyType', sortable: true, editable: false, formatter: currencyFormatter ,hidden:true},
               { 
                   name: 'stock', 
                   index: 'stock', 
                   align: "center", 
                   sortable: true, 
                   editable: false, 
                   formatter: function (cellvalue, options, rowObject) {
                       return `
                           <div class="stock-container" style="display: flex; align-items: center; justify-content: center;">
                               <input type="number" class="stock-input" 
                                   data-id="${rowObject.id}"
                                   data-product-name="${rowObject.name}"
                                   data-stockcode="${rowObject.stockcode}"
                                   data-barcode="${rowObject.barcode}"
                                   value="${cellvalue}"
                                   data-original-value="${cellvalue}"
                                   readonly
                                   style="width: 70px; height: 25px; text-align: center; border: 1px solid #ccc; border-radius: 3px; font-size: 12px; cursor: pointer;">
                               <div class="stock-controls" style="display: flex; flex-direction: column; margin-left: 3px;">
                                   <button class="increaseStockBtn" data-id="${rowObject.id}" data-product-name="${rowObject.name}" data-stockcode="${rowObject.stockcode}" data-barcode="${rowObject.barcode}" title="Stok Artır"
                                       style="border: none; background: transparent; cursor: pointer; font-size: 10px; padding: 2px;">
                                       ▲
                                   </button>
                                   <button class="decreaseStockBtn" data-id="${rowObject.id}" data-product-name="${rowObject.name}" data-stockcode="${rowObject.stockcode}" data-barcode="${rowObject.barcode}" title="Stok Azalt"
                                       style="border: none; background: transparent; cursor: pointer; font-size: 10px; padding: 2px;">
                                       ▼
                                   </button>
                               </div>
                           </div>
                       `;
                   }
               },
               {
                   name: 'location',
                   index: 'location',
                   align: "center",
                   sortable: false,
                   formatter: function (cellvalue) {
                       return cellvalue ? cellvalue : `<span class="text-muted">Lokasyon Yok</span>`;
                   }
               },
               {
                   name: "actions",
                   label: "İşlemler",
                   align: "center",
                   formatter: function (cellvalue, options, rowObject) {
                       return `
                           <div class="action-icons">                            
                               <i class="fa-solid fa-trash text-danger fa-xl deleteWarehouseProductBtn"
                                   data-id="${rowObject.id}" title="Depodan Kaldır"></i>
                           </div>
                       `;
                   }
               }
           ],
           pager: '#pager',
           rowNum: 5,
           rowList: [5, 10, 15, 20, 30, 40, 50],
           sortname: 'id',
           sortorder: 'asc',
           viewrecords: true,
           height: 'auto',
           autowidth: true,
           shrinkToFit: true,
           caption: "Depodaki Ürünler",
           loadonce: false,
           guiStyle: "bootstrap4",
           jsonReader: {
               root: "rows",
               page: "page",
               total: "total",
               records: "records"
           },

             ondblClickRow: function (rowid) {
                if (rowid) {
                    const rowData = $("#warehouseProductGrid").jqGrid("getRowData", rowid);
                    const productId = rowData.product_id;
                    window.open('/Product/ProductDetail/' + productId, '_blank');
                }
            },

          
           rowattr: function (rowObject) {
               if (rowObject.stock < 15) {
                   return { "class": "low-stock-row" };
               }
           },
       });
           
       $("<style>")
        .prop("type", "text/css")
        .html(`
            .low-stock-row td {
                background-color: #ffcccc !important;
            }
        `)
        .appendTo("head");

        
        function currencyFormatter(cellValue, options, rowObject) {
            return currencyDropdown[cellValue] || cellValue;
        }

      
        function formatPrice(cellValue, options, rowObject) {
            if (!cellValue) return "0.00";
            let currencySymbol = rowObject.currencyType === "TL" ? "₺" : "$";
            return cellValue.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) + " " + currencySymbol;
        }

        
        function filterWarehouseProducts() {
            let searchName = $("#productSearch").val().trim();
            let minPrice = $("#minPrice").val();
            let maxPrice = $("#maxPrice").val();
            let StockCode = $("#stockCodeSearch").val().trim();

            $("#warehouseProductGrid").jqGrid('setGridParam', {
                url: `/WarehouseProduct/GetWarehouseProducts?warehouseId=${warehouseId}&search=${searchName}&minPrice=${minPrice}&maxPrice=${maxPrice}&stockCode=${StockCode}`,
                page: 1
            }).trigger("reloadGrid");
        }

        
        $("#filterBtn").on("click", function () {
            filterWarehouseProducts();
        });

  

// ➕ Artırma
$(document).on("click", ".increaseStockBtn", function () {
    let warehouseProductId = $(this).data("id");
    let productName = $(this).data("product-name");
    let stockCode = $(this).data("stockcode");
    let barcode = $(this).data("barcode");
    let input = $(`input.stock-input[data-id="${warehouseProductId}"]`);
    let current = parseInt(input.val());
    let originalValue = current;

    updateStock(warehouseProductId, current + 1, productName, stockCode, barcode, originalValue);
});

// ➖ Azaltma
$(document).on("click", ".decreaseStockBtn", function () {
    let warehouseProductId = $(this).data("id");
    let productName = $(this).data("product-name");
    let stockCode = $(this).data("stockcode");
    let barcode = $(this).data("barcode");
    let input = $(`input.stock-input[data-id="${warehouseProductId}"]`);
    let current = parseInt(input.val());
    let originalValue = current;

    if (current > 0) updateStock(warehouseProductId, current - 1, productName, stockCode, barcode, originalValue);
});

// 🖱️ Stok sayısına tıklayınca input aktifleşsin
$(document).on("click", ".stock-input", function () {
    $(this).data("original-value", $(this).val());
    $(this).removeAttr("readonly");

    setTimeout(() => {
        $(this).focus().select();
    }, 0);
});

// 🖊️ Kullanıcı inputtan çıkarsa veya Enter'a basarsa yeni değer gönder
$(document).on("blur", ".stock-input", function () {
    let input = $(this);
    let warehouseProductId = input.data("id");
    let productName = input.data("product-name");
    let stockCode = input.data("stockcode");
    let barcode = input.data("barcode");
    let newValue = parseInt(input.val());
    let originalValue = parseInt(input.data("original-value"));

    if (!isNaN(newValue)) updateStock(warehouseProductId, newValue, productName, stockCode, barcode, originalValue);
});

$(document).on("keypress", ".stock-input", function (e) {
    if (e.which === 13) {
        $(this).blur(); 
    }
});

function updateStock(warehouseProductId, newStock, productName, stockCode, barcode, originalValue) {
    // Orijinal değer hala tanımsızsa, grid'den alıyoruz
    if (originalValue === undefined) {
        originalValue = parseInt($(`input.stock-input[data-id="${warehouseProductId}"]`).data("original-value"));
        
        if (isNaN(originalValue)) {
            originalValue = parseInt($(`input.stock-input[data-id="${warehouseProductId}"]`).val());
        }
    }

    // ❗ Sunucuya stok güncellemesi yapmayı kaldırıyoruz. Sadece UI üstünden değişim gösteriyoruz.
    $(`input.stock-input[data-id="${warehouseProductId}"]`).val(newStock);

    // Stok değişimini kaydet (fiş oluşturulacak değişiklikleri işlemek için)
    trackStockChange(warehouseProductId, originalValue, newStock, productName, stockCode, barcode);
}

// Stok değişimlerini izleme fonksiyonu (güncellenmiş: fiyat dahil)
function trackStockChange(warehouseProductId, oldStock, newStock, productName, stockCode, barcode) {
    const difference = newStock - oldStock;
    if (difference === 0) return;

    // Grid'in verisini al
    const gridData = $("#warehouseProductGrid").jqGrid("getGridParam", "data");
    let productPrice = 0;
    let productId = null;

    // ID'ye göre depo ürününü bul ve product_id'yi çek
    if (gridData && gridData.length > 0) {
        const productRow = gridData.find(row => String(row.id) === String(warehouseProductId));
        if (productRow) {
            productPrice = productRow.price;
            productId = productRow.product_id; // ⭐️ Burada product_id alıyoruz
        }
    }

    if (!productPrice) {
        $.ajax({
            url: `/WarehouseProduct/GetWarehouseProducts?warehouseId=${warehouseId}`,
            type: 'GET',
            async: false,
            success: function(response) {
                if (response && response.rows) {
                    const product = response.rows.find(item => String(item.id) === String(warehouseProductId));
                    if (product) {
                        productPrice = product.price;
                        productId = product.product_id;
                    }
                }
            }
        });
    }

    if (!productId) {
        console.error("Ürünün ProductId bilgisi bulunamadı!");
        return;
    }

    if (stockChanges[productId]) {
        stockChanges[productId].quantity += difference;
        if (stockChanges[productId].quantity === 0) {
            delete stockChanges[productId];
        }
    } else {
        stockChanges[productId] = {
            id: productId,  // ✅ Artık productId gönderiyoruz
            productName: productName,
            quantity: difference,
            price: productPrice,
        };
    }

    updateReceiptPreview();
}

// Fiş önizlemelerini güncelleme fonksiyonu
function updateReceiptPreview() {
    // Giriş ve çıkış listelerini temizle
    $("#inputReceiptList").empty();
    $("#outputReceiptList").empty();
    
    let hasInputs = false;
    let hasOutputs = false;
    
    // Tüm stok değişimlerini dolaş
    Object.values(stockChanges).forEach(item => {
        if (item.quantity > 0) {
            // Giriş fişine ekle
            $("#inputReceiptList").append(`
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${item.productName}</strong>
                        <small class="d-block text-muted">Barkod: ${item.barcode || 'Yok'} | Stok Kodu: ${item.stockCode || 'Yok'}</small>
                    </div>
                    <span class="badge bg-success rounded-pill px-3 py-2 fs-6">+${item.quantity}</span>
                </li>
            `);
            hasInputs = true;
        } else if (item.quantity < 0) {
            // Çıkış fişine ekle
            $("#outputReceiptList").append(`
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${item.productName}</strong>
                        <small class="d-block text-muted">Barkod: ${item.barcode || 'Yok'} | Stok Kodu: ${item.stockCode || 'Yok'}</small>
                    </div>
                    <span class="badge bg-danger rounded-pill px-3 py-2 fs-6">${item.quantity}</span>
                </li>
            `);
            hasOutputs = true;
        }
    });
    
    // Kartların görünürlüğünü güncelle
    if (hasInputs) {
        $("#inputReceiptCard").removeClass("d-none");
    } else {
        $("#inputReceiptCard").addClass("d-none");
    }
    
    if (hasOutputs) {
        $("#outputReceiptCard").removeClass("d-none");
    } else {
        $("#outputReceiptCard").addClass("d-none");
    }
    
    // Tamamlama butonunun ve açıklama alanlarının görünürlüğünü güncelle
    if (hasInputs || hasOutputs) {
        $("#completeReceiptsBtn").removeClass("d-none");
        if (hasInputs) {
            $("#entryDescriptionContainer").removeClass("d-none");
        } else {
            $("#entryDescriptionContainer").addClass("d-none");
        }
        if (hasOutputs) {
            $("#exitDescriptionContainer").removeClass("d-none");
        } else {
            $("#exitDescriptionContainer").addClass("d-none");
        }
    } else {
        $("#completeReceiptsBtn").addClass("d-none");
        $("#entryDescriptionContainer").addClass("d-none");
        $("#exitDescriptionContainer").addClass("d-none");
    }
}

// Fişleri tamamlama fonksiyonu (Yöntem 1: Ayrı JSON istekleri)
$("#completeReceiptsBtn").on("click", function() {
    // Açıklama alanlarından değerleri al
    const entryDescription = $("#entryDescription").val().trim();
    const exitDescription = $("#exitDescription").val().trim();

    // Giriş ve çıkış değişikliklerini ayır
    const entryChanges = Object.values(stockChanges).filter(item => item.quantity > 0);
    const exitChanges = Object.values(stockChanges).filter(item => item.quantity < 0);

    // Gönderilecek istekler
    let requests = [];

    if (entryChanges.length > 0) {
        requests.push(
            $.ajax({
                url: '/Receipt/SaveEntryReceipt',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    warehouseId: warehouseId,
                    description: entryDescription,
                    changes: entryChanges
                })
            })
        );
    }
    if (exitChanges.length > 0) {
        requests.push(
            $.ajax({
                url: '/Receipt/SaveExitReceipt',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    warehouseId: warehouseId,
                    description: exitDescription,
                    changes: exitChanges
                })
            })
        );
    }

    // Tüm istekler tamamlanınca
    $.when.apply($, requests)
        .done(function() {
            Swal.fire({
                title: "Başarılı!",
                text: "Stok değişimleri kaydedildi.",
                icon: "success"
            });
            // Değişimleri ve açıklama alanını temizle
            stockChanges = {};
            $("#entryDescription").val("");
            $("#exitDescription").val("");
            updateReceiptPreview();
        })
        .fail(function() {
            Swal.fire({
                title: "Hata!",
                text: "Stok değişimleri kaydedilirken bir hata oluştu.",
                icon: "error"
            });
        });
});


        // Depodan Ürünü Kaldırma Butonu
        $(document).on("click", ".deleteWarehouseProductBtn", function () {
            let id = $(this).data("id");

            Swal.fire({
                title: "Ürünü Depodan Kaldır",
                text: "Bu ürünü depodan çıkarmak istediğinize emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Evet, Kaldır!",
                cancelButtonText: "İptal"
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: '/WarehouseProduct/RemoveProductFromWarehouse',
                        type: 'POST',
                        data: { warehouseProductId: id },
                        success: function (response) {
                            Swal.fire("Başarılı!", "Ürün depodan kaldırıldı.", "success");
                            $("#warehouseProductGrid").trigger("reloadGrid");
                        },
                        error: function () {
                            Swal.fire("Hata!", "Ürün kaldırılırken hata oluştu.", "error");
                        }
                    });
                }
            });
        });

            $(document).on("click", ".İncreaseStockBtn", function () {
        let productId = $(this).data("id");
        updateStock(productId, 1);
    });


    

        $("#pager_left").append(`
            <i id="customRefreshBtn" class="fa-solid fa-arrows-rotate" title="Yenile" style="cursor: pointer; margin-left: 10px;"></i>
        `);

                    $("#pager_left").append(`
        <i class="fa-solid fa-plus fa-2xl openProductaddModal" title="Yeni Ürün Ekle" style="cursor: pointer; margin-left: 10px;"></i>
    `);

            
        $("#customRefreshBtn").on("click", function () {
            $("#productSearch").val("");
            $("#minPrice").val("");
            $("#maxPrice").val("");
            $("#warehouseProductGrid").setGridParam({
                url: `/WarehouseProduct/GetWarehouseProducts?warehouseId=${warehouseId}`,
                page: 1
            }).trigger("reloadGrid");
        });

    });

