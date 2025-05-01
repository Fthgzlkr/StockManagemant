
let selectedReceiptId = null;
let startDate = null;
let endDate = null;
let selectedReceiptType = '';

function initReceiptGrid({ warehouseId = null }) {
    $(document).ready(function () {
        const baseUrl = warehouseId ? `/Receipt/GetReceipts?warehouseId=${warehouseId}` : `/Receipt/GetReceipts`;

        $("#receiptGrid").jqGrid({
            url: baseUrl,
            datatype: "json",
            mtype: "GET",
            colNames: ['Fiş No', 'Tarih', 'Toplam Tutar', 'İşlem Türü', 'İşlemler'],
            colModel: [
                { name: 'id', index: 'id', width: 40, align: "center", key: true, hidden: true },
                { name: 'date', index: 'date', width: 80, align: "center" },
                { name: 'totalAmount', index: 'totalAmount', width: 90, align: "right" },
                {
                    name: 'receiptType',
                    index: 'receiptType',
                    width: 100,
                    align: "center",
                    formatter: function (cellvalue) {
                        if (cellvalue === "Entry") {
                            return `<span class="text-success"><i class="fa-solid fa-arrow-down"></i> Depo Giriş</span>`;
                        } else if (cellvalue === "Exit") {
                            return `<span class="text-danger"><i class="fa-solid fa-arrow-up"></i> Depo Çıkış</span>`;
                        }
                        return cellvalue;
                    }
                },
                {
                    name: "actions",
                    label: "İşlemler",
                    align: "center",
                    width: 60,  // 📏 Burası küçüldü! 50-60 civarı ideal
                    formatter: function (cellvalue, options, rowObject) {
                        return `
                            <div class="action-icons">
                                <i class="fa-solid fa-pen text-primary fa-xs open-category-edit" data-id="${rowObject.id}" data-date="${rowObject.date}" title="Düzenle"></i>
                                <i class="fa-solid fa-trash text-danger fa-xs delete-btn" data-id="${rowObject.id}" title="Sil"></i>
                            </div>
                        `;
                    }
                }
            ],
            pager: '#receiptPager',
            rowNum: 10,
            rowList: [10, 20, 30, 50, 100],
            sortname: 'id',
            sortorder: 'asc',
            viewrecords: true,
            height: 'auto',
            autowidth: true,
            shrinkToFit: true,
            guiStyle: "bootstrap4",
            jsonReader: {
                root: "rows",
                page: "page",
                total: "total",
                records: "records"
            },
            onSelectRow: function (id) {
                selectedReceiptId = id;
            },
            ondblClickRow: function (rowid) {
                if (rowid) {
                    const title = 'Fiş Detayları';
                    openModal(`/Receipt/Details/${rowid}`, title, setupReceiptProductsEvents);
                }
            }
        });

        $("#receiptGrid").jqGrid('navGrid', '#receiptPager', { edit: false, add: false, del: false, search: false, refresh: true });

        // Date Range Picker
        $('input[name="daterange"]').daterangepicker({
            opens: 'center',
            minDate: "2025-01-03",
            locale: { format: 'YYYY-MM-DD' }
        }, function (start, end) {
            startDate = start.format('YYYY-MM-DD');
            endDate = end.format('YYYY-MM-DD');
            reloadGrid(baseUrl);
        }).on('cancel.daterangepicker', function () {
            startDate = null;
            endDate = null;
            reloadGrid(baseUrl);
        });

        // Receipt Type Filter
        $("#receiptTypeSelect").on("change", function () {
            selectedReceiptType = $(this).val();
            reloadGrid(baseUrl);
        });

        // Delete Button
        $(document).on("click", ".delete-btn", function () {
            const id = $(this).data("id");
            Swal.fire({
                title: "Silme İşlemi",
                text: "Bu fişi silmek istediğinize emin misiniz?",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Evet, Sil",
                cancelButtonText: "İptal"
            }).then((result) => {
                if (result.isConfirmed) {
                    $.post(`/Receipt/DeleteReceipt?receiptId=${id}`, function () {
                        Swal.fire("Başarılı", "Fiş başarıyla silindi.", "success");
                        $("#receiptGrid").trigger("reloadGrid");
                    }).fail(function () {
                        Swal.fire("Hata", "Fiş silinemedi.", "error");
                    });
                }
            });
        });

        // Edit Button
        $(document).on("click", ".open-category-edit", function () {
            const id = $(this).data("id");
            const oldDate = $(this).data("date");

            Swal.fire({
                title: "Fiş Güncelle",
                html: `<input type="date" id="receiptDate" class="swal2-input" value="${oldDate}">`,
                showCancelButton: true,
                confirmButtonText: "Güncelle",
                cancelButtonText: "İptal",
                preConfirm: () => {
                    const newDate = document.getElementById("receiptDate").value;
                    if (!newDate) {
                        Swal.showValidationMessage("Lütfen bir tarih seçin!");
                    }
                    return newDate;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    $.post("/Receipt/UpdateReceipt", { receiptId: id, date: result.value }, function (response) {
                        if (response.success) {
                            Swal.fire("Başarılı", "Fiş başarıyla güncellendi.", "success");
                            $("#receiptGrid").trigger("reloadGrid");
                        } else {
                            Swal.fire("Hata", response.message || "Güncelleme başarısız.", "error");
                        }
                    }).fail(function () {
                        Swal.fire("Hata", "İşlem başarısız oldu.", "error");
                    });
                }
            });
        });

    });

    function reloadGrid(baseUrl) {
        let url = baseUrl;
    
        let params = [];
    
        if (startDate && endDate) {
            params.push(`startDate=${startDate}`, `endDate=${endDate}`);
        }
        if (selectedReceiptType) {
            params.push(`receiptType=${selectedReceiptType}`);
        }
    
        if (params.length > 0) {
            url += (url.includes('?') ? '&' : '?') + params.join('&');
        }
    
        $("#receiptGrid").setGridParam({ url: url, page: 1 }).trigger("reloadGrid");
    }
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
    $(document).off('click', '#printReceiptBtn').on('click', '#printReceiptBtn', async function () {
        try {
            // receipt-card'ı clone'la
            let receiptClone = $('.receipt-card').clone();
            // Clone içinde butonları sil
            receiptClone.find('button').remove();
    
            let receiptHtml = receiptClone.html(); // Butonlar temizlenmiş hali
    
            let receiptId = $('#showProductsBtn').data('receipt-id');
    
            const response = await $.get(`/Receipt/GetReceiptDetails?receiptId=${receiptId}`);
            const products = response.rows;
    
            let productRows = '';
            products.forEach(function (item) {
                productRows += `
                    <tr>
                        <td>${item.productName}</td>
                        <td>${item.quantity}</td>
                        <td>${item.unitPrice}</td>
                        <td>${item.subTotal}</td>
                    </tr>
                `;
            });
    
            const printWindow = window.open('', '_blank');
            printWindow.document.write(`
                <html>
                    <head>
                        <title>Fiş Yazdır</title>
                        <style>
                            body { font-family: Arial, sans-serif; padding: 20px; }
                            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                            th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }
                            th { background-color: #f2f2f2; }
                            .receipt-card p { margin: 5px 0; }
                            .receipt-header { margin-bottom: 20px; }
                        </style>
                    </head>
                    <body>
                        <div class="receipt-header">
                            <h2>Fiş Bilgileri</h2>
                            <div class="receipt-card">
                                ${receiptHtml}
                            </div>
                        </div>
    
                        <h3>Ürün Listesi</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>Ürün Adı</th>
                                    <th>Miktar</th>
                                    <th>Birim Fiyat</th>
                                    <th>Ara Toplam</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${productRows}
                            </tbody>
                        </table>
    
                        <script>
                            window.onload = function() {
                                window.print();
                            };
                        </script>
                    </body>
                </html>
            `);
            printWindow.document.close();
        } catch (error) {
            console.error("Yazdırma sırasında hata oluştu:", error);
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
