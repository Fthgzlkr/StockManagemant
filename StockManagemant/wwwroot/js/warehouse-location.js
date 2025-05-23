// Warehouse Location Management - JavaScript
let locations = [];
let allLocations = [];
let warehouseId = null;

// Storage Type Mapping
const storageTypes = {
    1: { name: 'Tanımsız', color: '#6c757d', icon: 'fas fa-question' },
    2: { name: 'Soğuk Depolama', color: '#17a2b8', icon: 'fas fa-snowflake' },
    3: { name: 'Yanıcı', color: '#dc3545', icon: 'fas fa-fire' },
    4: { name: 'Kırılgan', color: '#ffc107', icon: 'fas fa-glass-martini' },
    5: { name: 'Standart', color: '#28a745', icon: 'fas fa-box' },
    6: { name: 'Nem Korumalı', color: '#007bff', icon: 'fas fa-tint' }
};

// Page load
document.addEventListener("DOMContentLoaded", function () {
    initializeWarehouseLocation();
});

// Initialize the page
function initializeWarehouseLocation() {
    // URL'den warehouseId'yi al
    warehouseId = getWarehouseIdFromUrl();
    
    if (!warehouseId) {
        showToast('Depo ID bulunamadı!', 'error');
        return;
    }
    
    // Hidden input'a warehouseId'yi set et
    document.getElementById("warehouseIdHidden").value = warehouseId;
    document.getElementById("warehouseIdDisplay").textContent = warehouseId;
    
    // Lokasyonları yükle
    loadLocations();
}

// URL'den warehouse ID'yi çıkar
function getWarehouseIdFromUrl() {
    // URL pattern: /Warehouse/Locations/2
    const pathParts = window.location.pathname.split('/');
    const warehouseIndex = pathParts.indexOf('Locations');
    
    if (warehouseIndex !== -1 && pathParts[warehouseIndex + 1]) {
        return parseInt(pathParts[warehouseIndex + 1]);
    }
    
    // Fallback: URL parameter'dan al
    const urlParams = new URLSearchParams(window.location.search);
    const paramWarehouseId = urlParams.get('warehouseId');
    
    if (paramWarehouseId) {
        return parseInt(paramWarehouseId);
    }
    
    return null;
}

// Load all locations
async function loadLocations() {
    try {
        console.log(`📦 Depo ${warehouseId} için lokasyonlar yükleniyor...`);
        
        const response = await fetch(`/WarehouseLocation/GetLocationsByWarehouseId?warehouseId=${warehouseId}`);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        
        if (data.success === false) {
            throw new Error(data.message || 'Bilinmeyen hata');
        }
        
        allLocations = data || [];
        locations = [...allLocations];
        
        console.log(`✅ ${allLocations.length} lokasyon yüklendi`);
        
        renderLocationTree();
        populateParentSelect();
    } catch (error) {
        console.error('❌ Lokasyonlar yüklenirken hata:', error);
        document.getElementById("locationTreeContainer").innerHTML = 
            `<div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i> 
                Lokasyonlar yüklenirken hata oluştu: ${error.message}
                <button class="btn btn-outline-danger btn-sm ms-2" onclick="loadLocations()">
                    <i class="fas fa-redo"></i> Tekrar Dene
                </button>
            </div>`;
    }
}

// Render location tree
function renderLocationTree() {
    const container = document.getElementById("locationTreeContainer");
    
    if (!locations || locations.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-box-open fa-3x text-muted mb-3"></i>
                <h5 class="text-muted">Henüz lokasyon bulunmuyor</h5>
                <p class="text-muted">Yeni lokasyon eklemek için yukarıdaki butonu kullanın</p>
            </div>`;
        return;
    }

    // Build hierarchical structure
    const hierarchy = buildHierarchy(locations);
    const html = renderNodes(hierarchy);
    container.innerHTML = html;
}

// Build hierarchical structure
function buildHierarchy(flatLocations) {
    const map = {};
    const roots = [];

    // Create map
    flatLocations.forEach(location => {
        map[location.id] = { ...location, children: [] };
    });

    // Build hierarchy
    flatLocations.forEach(location => {
        if (location.parentId && map[location.parentId]) {
            map[location.parentId].children.push(map[location.id]);
        } else {
            roots.push(map[location.id]);
        }
    });

    return roots;
}

// Render nodes recursively
function renderNodes(nodes, level = 1) {
    if (!nodes || nodes.length === 0) return '';

    return nodes.map(node => {
        const storageType = storageTypes[node.storageType] || storageTypes[1];
        const childrenHtml = renderNodes(node.children, level + 1);
        const hasChildren = node.children && node.children.length > 0;
        
        return `
            <div class="tree-node level-${level}" data-id="${node.id}">
                <div class="node-header">
                    <div class="node-info">
                        <i class="${storageType.icon}" style="color: ${storageType.color}"></i>
                        <span class="node-title" 
                              contenteditable="true" 
                              data-original="${node.name}"
                              onblur="updateLocationName(this, ${node.id})"
                              onkeydown="handleEnterKey(event, this, ${node.id})">${node.name}</span>
                        <span class="level-badge">Seviye ${node.level}</span>
                        <span class="storage-type-badge" style="background-color: ${storageType.color}">
                            ${storageType.name}
                        </span>
                        ${hasChildren ? `<small class="text-muted">(${node.children.length} alt)</small>` : ''}
                    </div>
                    <div class="action-buttons">
                        <button class="action-btn btn-add" onclick="showAddChildForm(${node.id}, ${node.level})" title="Alt lokasyon ekle">
                            <i class="fas fa-plus"></i>
                        </button>
                        <button class="action-btn btn-edit" onclick="editLocation(${node.id})" title="Düzenle">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="action-btn btn-delete" onclick="deleteLocation(${node.id}, '${node.name.replace(/'/g, "\\'")}', ${hasChildren})" title="Sil">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                ${childrenHtml}
            </div>`;
    }).join('');
}

// Handle Enter key in contenteditable
function handleEnterKey(event, element, id) {
    if (event.key === 'Enter') {
        event.preventDefault();
        element.blur();
    }
}

// Update location name
async function updateLocationName(element, id) {
    const newName = element.textContent.trim();
    const originalName = element.dataset.original;
    
    if (!newName || newName === originalName) {
        element.textContent = originalName;
        return;
    }

    try {
        const location = allLocations.find(l => l.id === id);
        if (!location) {
            element.textContent = originalName;
            return;
        }

        const response = await fetch('/WarehouseLocation/EditLocation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                id: id,
                name: newName,
                warehouseId: parseInt(warehouseId),
                level: location.level,
                parentId: location.parentId,
                storageType: location.storageType
            })
        });

        const result = await response.json();
        
        if (result.success) {
            element.dataset.original = newName;
            await loadLocations(); // Refresh
            showToast('Lokasyon başarıyla güncellendi', 'success');
        } else {
            element.textContent = originalName;
            showToast(result.message || 'Güncelleme başarısız', 'error');
        }
    } catch (error) {
        element.textContent = originalName;
        console.error('Error updating location:', error);
        showToast('Güncelleme sırasında hata oluştu', 'error');
    }
}

// Show add location form
function showAddLocationForm() {
    document.getElementById("addLocationForm").classList.remove('hidden');
    document.getElementById("newLocationName").focus();
    clearAddForm();
}

// Hide add location form
function hideAddLocationForm() {
    document.getElementById("addLocationForm").classList.add('hidden');
    clearAddForm();
}

// Show add child form (pre-fill parent and inherit storage type)
function showAddChildForm(parentId, parentLevel) {
    showAddLocationForm();
    
    const parentSelect = document.getElementById("parentLocationSelect");
    const levelSelect = document.getElementById("levelSelect");
    
    parentSelect.value = parentId;
    levelSelect.value = Math.min(parentLevel + 1, 5);
    
    // Parent seçildiğinde StorageType'ı güncelle
    updateChildStorageType();
}

// Clear add form
function clearAddForm() {
    document.getElementById("newLocationName").value = '';
    document.getElementById("parentLocationSelect").value = '';
    document.getElementById("levelSelect").value = '1';
    document.getElementById("storageTypeSelect").value = '5';
    hideParentStorageWarning();
}

// Parent seçildiğinde child'ın StorageType'ını güncelle
function updateChildStorageType() {
    const parentId = document.getElementById("parentLocationSelect").value;
    const storageTypeSelect = document.getElementById("storageTypeSelect");
    
    if (!parentId) {
        // Parent seçilmemişse varsayılan değere dön
        storageTypeSelect.value = '5'; // Standart
        hideParentStorageWarning();
        return;
    }
    
    // Parent lokasyonunu bul
    const parentLocation = allLocations.find(loc => loc.id == parentId);
    
    if (parentLocation) {
        // Parent'ın StorageType'ını child'a ata
        storageTypeSelect.value = parentLocation.storageType;
        
        // Parent'ın StorageType'ını belirten uyarıyı göster
        showParentStorageWarning(parentLocation.name, parentLocation.storageType);
    }
}

// Parent StorageType uyarısını göster
function showParentStorageWarning(parentName, storageType) {
    const warningDiv = document.getElementById("parentStorageWarning");
    const warningText = document.getElementById("parentStorageText");
    const storageTypeName = storageTypes[storageType]?.name || 'Bilinmeyen';
    
    warningText.textContent = `Alt lokasyon, üst lokasyon "${parentName}" ile aynı depolama türünde (${storageTypeName}) olacak.`;
    warningDiv.classList.remove('hidden');
}

// Parent StorageType uyarısını gizle
function hideParentStorageWarning() {
    document.getElementById("parentStorageWarning").classList.add('hidden');
}

// Populate parent select
function populateParentSelect() {
    const select = document.getElementById("parentLocationSelect");
    
    let options = '<option value="">Ana Lokasyon (Kök)</option>';
    
    // Sort by level and name
    const sortedLocations = [...allLocations].sort((a, b) => {
        if (a.level !== b.level) return a.level - b.level;
        return a.name.localeCompare(b.name);
    });
    
    sortedLocations.forEach(location => {
        const indent = '&nbsp;'.repeat((location.level - 1) * 4);
        const storageTypeName = storageTypes[location.storageType]?.name || 'Bilinmeyen';
        options += `<option value="${location.id}" data-storage-type="${location.storageType}">
                        ${indent}${location.name} (Seviye ${location.level}, ${storageTypeName})
                    </option>`;
    });
    
    select.innerHTML = options;
}

// Add new location
async function addLocation() {
    const name = document.getElementById("newLocationName").value.trim();
    const parentId = document.getElementById("parentLocationSelect").value;
    const level = parseInt(document.getElementById("levelSelect").value);
    const storageType = parseInt(document.getElementById("storageTypeSelect").value);

    if (!name) {
        showToast('Lokasyon adı gerekli', 'error');
        document.getElementById("newLocationName").focus();
        return;
    }

    if (name.length > 50) {
        showToast('Lokasyon adı maksimum 50 karakter olabilir', 'error');
        return;
    }

    try {
        const requestData = {
            name: name,
            warehouseId: parseInt(warehouseId),
            level: level,
            parentId: parentId ? parseInt(parentId) : null,
            storageType: storageType
        };
        
        console.log('📤 Lokasyon ekleniyor:', requestData);

        const response = await fetch('/WarehouseLocation/AddLocation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });

        const result = await response.json();
        
        if (result.success) {
            hideAddLocationForm();
            await loadLocations();
            showToast('Lokasyon başarıyla eklendi', 'success');
        } else {
            showToast(result.message || 'Ekleme başarısız', 'error');
        }
    } catch (error) {
        console.error('Error adding location:', error);
        showToast('Ekleme sırasında hata oluştu', 'error');
    }
}

// Delete location
async function deleteLocation(id, name, hasChildren) {
    let confirmMessage = `"${name}" lokasyonunu silmek istediğinize emin misiniz?`;
    
    if (hasChildren) {
        confirmMessage += '\n\nBu lokasyonun alt lokasyonları da silinecektir!';
    }
    
    confirmMessage += '\n\nBu işlem geri alınamaz.';
    
    if (!confirm(confirmMessage)) {
        return;
    }

    try {
        const response = await fetch(`/WarehouseLocation/Delete?id=${id}`, {
            method: 'POST'
        });

        const result = await response.json();
        
        if (result.success) {
            await loadLocations();
            showToast('Lokasyon başarıyla silindi', 'success');
        } else {
            showToast(result.message || 'Silme başarısız', 'error');
        }
    } catch (error) {
        console.error('Error deleting location:', error);
        showToast('Silme sırasında hata oluştu', 'error');
    }
}

// Edit location
function editLocation(id) {
    const location = allLocations.find(l => l.id === id);
    if (!location) return;
    
    // Pre-fill form with existing data
    showAddLocationForm();
    document.getElementById("newLocationName").value = location.name;
    document.getElementById("parentLocationSelect").value = location.parentId || '';
    document.getElementById("levelSelect").value = location.level;
    document.getElementById("storageTypeSelect").value = location.storageType;
    
    // Change button text to update
    const saveBtn = document.querySelector('#addLocationForm .btn-success');
    saveBtn.innerHTML = '<i class="fas fa-save"></i> Güncelle';
    saveBtn.onclick = () => updateLocation(id);
}

// Update existing location
async function updateLocation(id) {
    const name = document.getElementById("newLocationName").value.trim();
    const parentId = document.getElementById("parentLocationSelect").value;
    const level = parseInt(document.getElementById("levelSelect").value);
    const storageType = parseInt(document.getElementById("storageTypeSelect").value);

    if (!name) {
        showToast('Lokasyon adı gerekli', 'error');
        return;
    }

    try {
        const response = await fetch('/WarehouseLocation/EditLocation', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                id: id,
                name: name,
                warehouseId: parseInt(warehouseId),
                level: level,
                parentId: parentId ? parseInt(parentId) : null,
                storageType: storageType
            })
        });

        const result = await response.json();
        
        if (result.success) {
            hideAddLocationForm();
            await loadLocations();
            showToast('Lokasyon başarıyla güncellendi', 'success');
        } else {
            showToast(result.message || 'Güncelleme başarısız', 'error');
        }
    } catch (error) {
        console.error('Error updating location:', error);
        showToast('Güncelleme sırasında hata oluştu', 'error');
    }
}

// Search locations
function searchLocations() {
    const query = document.getElementById("searchInput").value.toLowerCase().trim();
    
    if (!query) {
        locations = [...allLocations];
    } else {
        locations = allLocations.filter(location => 
            location.name.toLowerCase().includes(query)
        );
    }
    
    renderLocationTree();
}

// Show toast notification
function showToast(message, type = 'info') {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll('.toast-notification');
    existingToasts.forEach(toast => toast.remove());
    
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} position-fixed toast-notification`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 500px;';
    toast.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas fa-${type === 'error' ? 'exclamation-triangle' : type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
            <span class="flex-grow-1">${message}</span>
            <button type="button" class="btn-close ms-2" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, 5000);
}

// Reset form when hiding
document.addEventListener('DOMContentLoaded', function() {
    const addLocationForm = document.getElementById('addLocationForm');
    if (addLocationForm) {
        addLocationForm.addEventListener('transitionend', function() {
            if (this.classList.contains('hidden')) {
                // Reset button
                const saveBtn = this.querySelector('.btn-success');
                if (saveBtn) {
                    saveBtn.innerHTML = '<i class="fas fa-save"></i> Kaydet';
                    saveBtn.onclick = addLocation;
                }
            }
        });
    }
});