    // warehouseId is now read from a hidden input to support modal loading context
    const warehouseId = document.getElementById("warehouseIdHidden")?.value;

    document.addEventListener("DOMContentLoaded", function () {
        loadTree();
    });

    function toggleCorridorInput() {
        const area = document.getElementById("corridorInputArea");
        area.style.display = area.style.display === "none" ? "block" : "none";
    }

    function loadTree() {
        fetch(`/WarehouseLocation/GetLocationsByWarehouseId?warehouseId=${warehouseId}`)
            .then(response => response.json())
            .then(data => {
                renderTreeView(data);
            })
            .catch(err => {
                document.getElementById("locationTreeContainer").innerHTML =
                    `<div class="alert alert-danger">Lokasyonlar yüklenemedi.</div>`;
                console.error(err);
            });
    }

    function addCorridor() {
        const corridorName = document.getElementById("newCorridorInput").value.trim();
        if (!corridorName) return alert("Lütfen bir koridor adı girin.");

        fetch('/WarehouseLocation/AddLocation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                warehouseId: parseInt(warehouseId),
                corridor: corridorName
            })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    document.getElementById("newCorridorInput").value = "";
                    toggleCorridorInput();
                    loadTree();
                } else {
                    alert(data.message);
                }
            });
    }

    function renderTreeView(locations) {
        if (!locations || locations.length === 0) {
            document.getElementById("locationTreeContainer").innerHTML =
                `<div class="alert alert-warning">Bu depoya ait hiç lokasyon bulunamadı.</div>`;
            return;
        }

        const grouped = {};
        locations.forEach(loc => {
            if (!loc || !loc.corridor) return;
            if (!grouped[loc.corridor]) {
                grouped[loc.corridor] = { id: loc.id, shelves: {} };
            }
            if (loc.shelf) {
                if (!grouped[loc.corridor].shelves[loc.shelf]) {
                    grouped[loc.corridor].shelves[loc.shelf] = [];
                }
                if (loc.bin) {
                    grouped[loc.corridor].shelves[loc.shelf].push({ id: loc.id, name: loc.bin });
                }
            }
        });

        let html = "";
        for (const corridor in grouped) {
            const corridorId = grouped[corridor].id;
            html += `<div class="tree-card">
                        <h6>
                          📁 <span class="editable" contenteditable="true" data-id="${corridorId}" onblur="updateText(this, 'corridor', ${warehouseId}, '${corridor}')">${corridor}</span>
                          <span class="action-icons">
                              <i class="fa fa-plus" onclick='openAddShelfInput("${corridor}", this)'></i>
                              <i class="fa fa-minus" onclick="deleteLocation('${corridorId}', 'corridor', ${warehouseId})"></i>
                          </span>
                        </h6>
                        <ul class="tree">`;

            for (const shelf in grouped[corridor].shelves) {
                html += `<li class="level-shelf">
                            📂 <span class="editable" contenteditable="true" data-id="${(grouped[corridor].shelves[shelf] && grouped[corridor].shelves[shelf][0]?.id) || ''}" onblur="updateText(this, 'shelf', ${warehouseId}, '${corridor}', '${shelf}')">${shelf}</span>
                            <span class="action-icons">
                                <i class="fa fa-plus" onclick='openAddBinInput("${corridor}", "${shelf}", this)'></i>
                                <i class="fa fa-minus" onclick="deleteLocation('${(grouped[corridor].shelves[shelf] && grouped[corridor].shelves[shelf][0]?.id) || ''}', 'shelf', ${warehouseId}, '${corridor}')"></i>
                            </span>
                          </li>
                          <ul>
                            ${(grouped[corridor].shelves[shelf] || []).map(bin => `<li class="level-bin">📦 <span class="editable" contenteditable="true" data-id="${bin.id}" onblur="updateText(this, 'bin', ${warehouseId}, '${corridor}', '${shelf}', '${bin.name}')">${bin.name}</span> 
                              <span class="action-icons">
                                <i class="fa fa-minus" onclick="deleteLocation('${bin.id}', 'bin', ${warehouseId}, '${corridor}', '${shelf}')"></i>
                              </span></li>`).join('')}
                          </ul>`;
            }

            html += `</ul></div>`;
        }

        document.getElementById("locationTreeContainer").innerHTML = html;
    }

    function openAddShelfInput(corridor, btn) {
        const parent = btn.closest(".tree-card");
        const input = document.createElement("textarea");
        input.className = "form-control add-input mt-2";
        input.placeholder = "Yeni raf adı";
        input.rows = 1;
        input.oninput = function () { this.value = this.value.toUpperCase().slice(0, 2); };

        const submit = document.createElement("button");
        submit.className = "btn btn-sm btn-success mt-1";
        submit.innerText = "Ekle";
        submit.onclick = function () {
            const shelfName = input.value.trim();
            if (!shelfName) return;
            fetch('/WarehouseLocation/AddLocation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    warehouseId: parseInt(warehouseId),
                    corridor: corridor,
                    shelf: shelfName
                })
            })
            .then(res => res.json())
            .then(data => {
                if (data.success) loadTree();
                else alert(data.message);
            });
        };

        parent.appendChild(input);
        parent.appendChild(submit);
        btn.disabled = true;
    }

    function openAddBinInput(corridor, shelf, btn) {
        const li = btn.closest("li");
        const input = document.createElement("textarea");
        input.className = "form-control add-input mt-2";
        input.placeholder = "Yeni göz adı";
        input.rows = 1;
        input.oninput = function () { this.value = this.value.toUpperCase().slice(0, 2); };

        const submit = document.createElement("button");
        submit.className = "btn btn-sm btn-success mt-1";
        submit.innerText = "Ekle";
        submit.onclick = function () {
            const binName = input.value.trim();
            if (!binName) return;
            fetch('/WarehouseLocation/AddLocation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    warehouseId: parseInt(warehouseId),
                    corridor: corridor,
                    shelf: shelf,
                    bin: binName
                })
            })
            .then(res => res.json())
            .then(data => {
                if (data.success) loadTree();
                else alert(data.message);
            });
        };

        li.appendChild(input);
        li.appendChild(submit);
        btn.disabled = true;
    }

    function updateText(element, level, warehouseId, corridor = null, shelf = null, originalValue = null) {
        const newValue = element.innerText.trim();
        if (!newValue || newValue === originalValue) return;

        fetch('/WarehouseLocation/EditLocation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                id: element.dataset.id,
                warehouseId: warehouseId,
                corridor: level === 'corridor' ? newValue : corridor,
                shelf: level === 'shelf' ? newValue : shelf,
                bin: level === 'bin' ? newValue : null
            })
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) loadTree();
            else alert(data.message);
        });
    }

    function deleteLocation(name, level, warehouseId, corridor = null, shelf = null) {
        if (!confirm("Bu lokasyonu silmek istediğinize emin misiniz?")) return;

        fetch(`/WarehouseLocation/Delete?id=${name}`, {
            method: 'POST'
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) loadTree();
            else alert(data.message);
        });
    }

