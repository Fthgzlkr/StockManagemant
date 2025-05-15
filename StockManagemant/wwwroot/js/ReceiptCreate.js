
    let warehouseId = new URLSearchParams(window.location.search).get("warehouseId");
    let receiptItems = [];
    let selectedSourceId = null;

    // Helper: Receipt içeriğini tamamen temizle ve UI sıfırla
    function clearReceiptContents() {
        receiptItems = [];
        $("#receiptItems").empty();
        updateTotal();
        // Kaynak seçimlerini tekrar aç
        $("#sourceType").prop("disabled", false);
        $("#sourceSelect").prop("disabled", false);
        $("input[name='receiptType']").prop("disabled", false);
        $("#searchedProduct").addClass("d-none");
        updateProductSearchAvailability();
    }

    // Açıklama, helper ve event kaydı fonksiyonlarını tek yerde topla
    function registerEvents() {
        // Sayfa yüklendiğinde ürün arama alanını devre dışı bırak
        $("#productBarcodeInput").prop("disabled", true);
        $("#searchProductBtn").prop("disabled", true);
        updateProductSearchInfo();
        updateBarcodeInputHelper();

        // ReceiptType değişiminde
        $("input[name='receiptType']").off("change").on("change", function() {
            if (receiptItems.length > 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Fiş içeriği temizlenecek. Onaylıyor musunuz?",
                    showCancelButton: true,
                    confirmButtonText: "Evet, temizle",
                    cancelButtonText: "Vazgeç"
                }).then((result) => {
                    if (result.isConfirmed) {
                        clearReceiptContents();
                        updateProductSearchInfo();
                        updateBarcodeInputHelper();
                    } else {
                        // Önceki seçimi geri getir
                        let prevType = $("input[name='receiptType']").not(this).prop("checked", true);
                    }
                });
                return;
            }
            updateProductSearchAvailability();
            updateProductSearchInfo();
            updateBarcodeInputHelper();
        });

        // Source Type değişimi
        $('#sourceType').off("change").on('change', function() {
            if (receiptItems.length > 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Fiş içeriği temizlenecek. Onaylıyor musunuz?",
                    showCancelButton: true,
                    confirmButtonText: "Evet, temizle",
                    cancelButtonText: "Vazgeç"
                }).then((result) => {
                    if (result.isConfirmed) {
                        clearReceiptContents();
                        fetchSourceOptions();
                        updateProductSearchInfo();
                        updateBarcodeInputHelper();
                    } else {
                        // Önceki seçimi geri getir
                        let prev = $(this).data("previous");
                        $("#sourceType").val(prev);
                    }
                });
                return;
            }
            fetchSourceOptions();
            updateProductSearchInfo();
            updateBarcodeInputHelper();
        }).each(function() {
            // İlk değer kaydı
            $(this).data("previous", $(this).val());
        }).on("focus", function() {
            $(this).data("previous", $(this).val());
        });

        // Source ID değişimi
        $('#sourceSelect').off("change").on('change', function() {
            selectedSourceId = $(this).val();
            updateProductSearchAvailability();
            updateProductSearchInfo();
            updateBarcodeInputHelper();
        });

        // Ürün arama butonu
        $("#searchProductBtn").off("click").on("click", function () {
            let barcode = $("#productBarcodeInput").val();
            if (!barcode) {
                Swal.fire("Hata!", "Lütfen geçerli bir ürün barkodu girin!", "error");
                return;
            }

            // Hangi depo ID'si ile arama yapılacağını belirle
            let searchWarehouseId = warehouseId;
            const receiptType = parseInt($("input[name='receiptType']:checked").val());
            const sourceType = parseInt($("#sourceType").val());
            if (sourceType === 1 && receiptType === 1 && selectedSourceId) {
                searchWarehouseId = selectedSourceId;
            }

            $.get(`/Receipt/GetWarehouseProduct?warehouseId=${searchWarehouseId}&barcode=${barcode}`, function (response) {
                if (response.success) {
                    showSearchedProduct(response.data);
                } else {
                    Swal.fire("Hata!", response.message, "error");
                }
            }).fail(function () {
                Swal.fire("Hata!", "Ürün getirilirken bir hata oluştu!", "error");
            });
        });

        // Ürün fişe ekleme butonunu showSearchedProduct içinde tanımlı

        // Ürün silme
        $(document).off("click", ".remove-product").on("click", ".remove-product", function () {
            let row = $(this).closest("div[data-id]");
            let productId = parseInt(row.attr("data-id"));
            receiptItems = receiptItems.filter(p => p.productId !== productId);
            row.remove();
            updateTotal();
            if (receiptItems.length === 0) {
                // Seçimleri tekrar aç
                $("#sourceType").prop("disabled", false);
                $("#sourceSelect").prop("disabled", false);
                $("input[name='receiptType']").prop("disabled", false);
            }
        });
    }

    // Kaynak tipine göre kaynak listesini getir
    function fetchSourceOptions() {
        const selectedType = $("#sourceType").val();
        const $sourceSelect = $('#sourceSelect');
        $sourceSelect.empty();
        selectedSourceId = null;
        if (selectedType == 1) { // Warehouse (Depo)
            fetch('/Warehouse/GetAllWarehouses')
                .then(response => response.json())
                .then(data => {
                    if (data.data && data.data.length > 0) {
                        $sourceSelect.append(new Option("Depo Seçiniz", ""));
                        data.data.forEach(w => {
                            $sourceSelect.append(new Option(w.name, w.id));
                        });
                    } else {
                        $sourceSelect.append(new Option("Depo bulunamadı", ""));
                    }
                    updateProductSearchAvailability();
                })
                .catch(err => {
                    console.error("Depolar yüklenemedi", err);
                });
        } else if (selectedType == 2) { // Customer (Müşteri)
            fetch('/Customer/GetCustomers')
                .then(response => response.json())
                .then(data => {
                    if (data.rows && data.rows.length > 0) {
                        $sourceSelect.append(new Option("Müşteri Seçiniz", ""));
                        data.rows.forEach(c => {
                            $sourceSelect.append(new Option(c.name, c.id));
                        });
                    } else {
                        $sourceSelect.append(new Option("Müşteri bulunamadı", ""));
                    }
                    updateProductSearchAvailability();
                })
                .catch(err => {
                    console.error("Müşteriler yüklenemedi", err);
                });
        } else { // None (3)
            $sourceSelect.append(new Option("Seçim yok", ""));
            updateProductSearchAvailability();
        }
    }

    // Ürün arama alanının durumunu güncelle ve helper güncelle
    function updateProductSearchAvailability() {
        const receiptType = parseInt($("input[name='receiptType']:checked").val());
        const sourceType = parseInt($("#sourceType").val());
        // Varsayılan olarak devre dışı bırak
        $("#productBarcodeInput").prop("disabled", true);
        $("#searchProductBtn").prop("disabled", true);
        if (sourceType && selectedSourceId && selectedSourceId !== "") {
            $("#productBarcodeInput").prop("disabled", false);
            $("#searchProductBtn").prop("disabled", false);
        }
    }

    // Ürün arama kartı üstündeki açıklama
    function updateProductSearchInfo() {
        const receiptType = parseInt($("input[name='receiptType']:checked").val());
        const sourceType = parseInt($("#sourceType").val());
        let info = "";
        if (sourceType === 1 && receiptType === 1) {
            info = "Seçilen depodan ürün arayabilirsiniz.";
        } else if (sourceType === 1 && receiptType === 2) {
            info = "Mevcut depodan ürün çıkışı yapılacak.";
        } else if (sourceType === 2) {
            info = "Müşteri seçimine göre ürün arayabilirsiniz.";
        } else if (sourceType === 3) {
            info = "Kaynak seçimi yapılmadı.";
        }
        $("#productSearchInfo").text(info);
    }

    // Barkod input altındaki helper açıklama
    function updateBarcodeInputHelper() {
        const receiptType = parseInt($("input[name='receiptType']:checked").val());
        const sourceType = parseInt($("#sourceType").val());
        let text = "";
        if (sourceType === 1 && receiptType === 1) {
            text = "Seçili depodan ürün ekleyebilirsiniz.";
        } else if (sourceType === 1 && receiptType === 2) {
            text = "Mevcut depodan ürün çıkışı için barkod girin.";
        } else if (sourceType === 2) {
            text = "Seçili müşteriye göre ürün ekleyebilirsiniz.";
        } else {
            text = "Önce kaynak ve tür seçiniz.";
        }
        $("#barcodeInputHelper").text(text);
    }

   function showSearchedProduct(product) {
    if (!product || !product.productId) {
        Swal.fire("Hata!", "Geçerli bir ürün bulunamadı!", "error");
        return;
    }

    $("#productName").text(product.productName);
    $("#productCategory").text(product.categoryName);
    $("#productPrice").text(product.price);
    $("#productStock").text(product.stockQuantity);
    $("#productQuantity").attr("max", product.stockQuantity);
    $("#searchedProduct").removeClass("d-none");

    $("#addProductBtn").off("click").on("click", function () {
        let quantity = parseInt($("#productQuantity").val());

        const receiptType = parseInt($("input[name='receiptType']:checked").val()); // 1 = Giriş, 2 = Çıkış
        const sourceType = parseInt($("#sourceType").val()); // 1 = Depo, 2 = Müşteri, 3 = None

        let checkAgainstStock = false;

        // Eğer depolar arası transfer yapılıyorsa ve giriş fişi oluşturuluyorsa, kaynaktan çıkış var demektir.
        if (sourceType === 1 && receiptType === 1) {
            checkAgainstStock = true;
        }

        // Eğer normal çıkış fişi oluşturuyorsa da mevcut depodan çıkış var demektir.
        if (receiptType === 2) {
            checkAgainstStock = true;
        }

        // Kontrol yapalım
        if (quantity < 1 || (checkAgainstStock && quantity > product.stockQuantity)) {
            Swal.fire("Hata!", "Geçerli bir miktar girin! (Stok yetersiz)", "error");
            return;
        }

        addProductToReceipt(product, quantity);
    });
}

    function addProductToReceipt(product, quantity) {
        let existingProduct = receiptItems.find(item => item.productId === product.productId);
        if (existingProduct) {
            Swal.fire("Hata!", "Bu ürün zaten fişe eklenmiş!", "error");
            return;
        }
        let newItem = `
            <div class="d-flex justify-content-between mb-1" data-id="${product.productId}">
                <div>
                    <span class="fw-bold">${product.productName}</span>
                    <span class="small text-muted">(${product.categoryName})</span>
                    <span class="small">x ${quantity}</span>
                </div>
                <div>
                    <span>${(product.price * quantity).toFixed(2)} TL</span>
                    <button class="btn btn-danger btn-sm ms-2 remove-product">X</button>
                </div>
            </div>
        `;
        $("#receiptItems").append(newItem);
        receiptItems.push({
            productId: product.productId,
            quantity: quantity,
            price: product.price
        });
        updateTotal();
        // Seçim kilitleme: ilk ürün eklendiğinde
        if (receiptItems.length === 1) {
            $("#sourceType").prop("disabled", true);
            $("#sourceSelect").prop("disabled", true);
            $("input[name='receiptType']").prop("disabled", true);
        }
    }

    function updateTotal() {
        let total = receiptItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        $("#totalAmount").text(total.toFixed(2) + " TL");
    }

    $("#createReceiptBtn").off("click").on("click", async function () {
        if (receiptItems.length === 0) {
            Swal.fire("Hata!", "Fişe en az bir ürün eklemelisiniz!", "warning");
            return;
        }

        const selectedSourceType = parseInt($("#sourceType").val());
        let selectedSourceId = null;
        if (selectedSourceType !== 3) {
            const sourceValue = $("#sourceSelect").val();
            if (sourceValue && !isNaN(sourceValue)) {
                selectedSourceId = parseInt(sourceValue);
            }
        }
        const receiptType = parseInt(document.querySelector("input[name='receiptType']:checked").value);
        const description = $("#receiptDescription").val();
        const totalAmount = receiptItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);

        try {
            // 1. Ana fiş oluştur (mevcut depo için)
            const createMainReceipt = await $.ajax({
                url: "/Receipt/CreateReceipt",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    wareHouseId: parseInt(warehouseId),
                    totalAmount: totalAmount,
                    date: new Date().toISOString(),
                    receiptType: receiptType,
                    description: description,
                    sourceType: selectedSourceType,
                    sourceId: selectedSourceId
                })
            });

            if (!createMainReceipt.success) {
                Swal.fire("Hata!", createMainReceipt.message, "error");
                return;
            }

            // 2. Ana fiş için ürünleri ekle
            await $.ajax({
                url: `/Receipt/AddProductsToReceipt?receiptId=${createMainReceipt.receiptId}`,
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(receiptItems)
            });

            // 2.5 Eğer kaynak depoda ürün yoksa ekle
            if (selectedSourceType === 1 && selectedSourceId) {
                for (const item of receiptItems) {
                    await $.ajax({
                        url: `/WarehouseProduct/EnsureWarehouseProductExists`,
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify({
                            warehouseId: selectedSourceId,
                            productId: item.productId
                        })
                    });
                }
            }

            // 3. Eğer Depolar Arası ise kaynak depoda da EXIT fişi oluştur
            if (selectedSourceType === 1 && selectedSourceId) {
                const createSourceReceipt = await $.ajax({
                    url: "/Receipt/CreateReceipt",
                    method: "POST",
                    contentType: "application/json",
                    data: JSON.stringify({
                        wareHouseId: selectedSourceId,
                        totalAmount: totalAmount,
                        date: new Date().toISOString(),
                        receiptType: receiptType === 1 ? 2 : 1, // Entry seçiliyse burada Exit oluştur
                        description: `Depolar arası transfer (${description || "Transfer"})`,
                        sourceType: 1, // warehouse
                        sourceId: parseInt(warehouseId) // kaynak = ana depo
                    })
                });

                if (createSourceReceipt.success) {
                    
                    const sourceReceiptItems = receiptItems.map(item => ({
                        productId: item.productId,
                        quantity: item.quantity,
                        price: item.price
                    }));

                    await $.ajax({
                        url: `/Receipt/AddProductsToReceipt?receiptId=${createSourceReceipt.receiptId}`,
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify(sourceReceiptItems)
                    });
                }
            }

            Swal.fire("Başarılı!", "Fiş(ler) başarıyla oluşturuldu.", "success").then(() => {
                window.location.href = `/Receipt/ListByWarehouse?warehouseId=${warehouseId}`;
            });

        } catch (error) {
            console.error(error);
            Swal.fire("Hata!", "Fiş(ler) oluşturulamadı!", "error");
        }
    });

    // Sayfa ilk açılışında eventleri kaydet, helperları güncelle
    $(document).ready(function() {
        registerEvents();
        updateProductSearchInfo();
        updateBarcodeInputHelper();
    });
