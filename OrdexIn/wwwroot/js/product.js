// file: `OrdexIn/wwwroot/js/product.js`
(function () {
    // Important constants
    const DEFAULT_PAGE_SIZE = 100;
    let page = 1;
    let lastTotalPages = 1;

    // DOM elements
    const tableContainer = document.getElementById('productsTable');
    const searchInput = document.getElementById('searchInput');
    const searchBtn = document.getElementById('searchBtn');
    const addBtn = document.getElementById('addProductBtn');
    const filterBtn = document.getElementById('filterLowStock');

    // pagination controls (multiple instances supported)
    const prevBtns = Array.from(document.querySelectorAll('.pager-prev'));
    const nextBtns = Array.from(document.querySelectorAll('.pager-next'));
    const pageInfos = Array.from(document.querySelectorAll('.pager-info'));
    const productsMeta = document.getElementById('productsMeta');
    
    // Admin check
    const isAdmin = tableContainer ? tableContainer.dataset.isAdmin === '1' : false;

    if (!tableContainer) {
        console.error('product.js: missing #productsTable element.');
        return;
    }

    const escapeHtml = (s) => String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;');
    const formatCurrency = (n) => {
        try { return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(n); } catch { return n; }
    };
    const showError = (msg) => {
        tableContainer.innerHTML = `<div class="products-error">Error loading products: ${escapeHtml(String(msg))}</div>`;
    };

    function createModal(html) {
        const overlay = document.createElement('div');
        overlay.className = 'pi-modal-overlay';
        overlay.innerHTML = `
            <div class="pi-modal" role="dialog" aria-modal="true">
                <div class="app-card pi-modal-card" style="position:relative;">
                    <button class="pi-modal-close btn" aria-label="Close">×</button>
                    <div class="pi-modal-body">${html}</div>
                </div>
            </div>`;
        document.body.appendChild(overlay);
        overlay.querySelector('.pi-modal-close').addEventListener('click', () => closeModal(overlay));
        overlay.addEventListener('click', (e) => { if (e.target === overlay) closeModal(overlay); });
        return overlay;
    }
    function closeModal(overlay) { overlay.remove(); }

    function getRowId(el) {
        const tr = el && el.closest ? el.closest('tr') : null;
        return tr ? parseInt(tr.getAttribute('data-id'), 10) : null;
    }

    async function apiFetch(url, opts) {
        const res = await fetch(url, opts);
        if (!res.ok) {
            const text = await res.text().catch(() => res.statusText);
            throw new Error(text || `HTTP ${res.status}`);
        }
        try { return await res.json(); } catch { return null; }
    }
    const apiGet = (u) => apiFetch(u, { method: 'GET' });
    const apiPost = (u, b) => apiFetch(u, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(b) });
    const apiPut = (u, b) => apiFetch(u, { method: 'PUT',  headers: {'Content-Type':'application/json'}, body: JSON.stringify(b) });
    const apiDelete = (u) => apiFetch(u, { method: 'DELETE' });

    function updatePagerControls() {
        pageInfos.forEach(pi => { pi.textContent = `Pagina ${page} / ${lastTotalPages}`; });
        const disablePrev = page <= 1;
        const disableNext = page >= lastTotalPages;
        prevBtns.forEach(b => b.disabled = disablePrev);
        nextBtns.forEach(b => b.disabled = disableNext);
    }

    function attachPagerListeners() {
        prevBtns.forEach(b => {
            if (b.dataset.piAttachedPrev) return;
            b.dataset.piAttachedPrev = '1';
            b.addEventListener('click', (e) => {
                e.preventDefault();
                page = Math.max(1, page - 1);
                fetchPage();
            });
        });
        nextBtns.forEach(b => {
            if (b.dataset.piAttachedNext) return;
            b.dataset.piAttachedNext = '1';
            b.addEventListener('click', (e) => {
                e.preventDefault();
                page = Math.min(lastTotalPages, page + 1);
                fetchPage();
            });
        });
    }

    function renderTable(data) {
        const returnedPage = (data && typeof data.page === 'number') ? data.page : page;
        const pageSize = (data && typeof data.pageSize === 'number') ? data.pageSize : DEFAULT_PAGE_SIZE;
        page = Math.max(1, returnedPage);

        const items = data && Array.isArray(data.items) ? data.items : (data || []).items || [];
        const total = (data && typeof data.total === 'number') ? data.total : (data && data.count) || 0;

        lastTotalPages = Math.max(1, Math.ceil((total || 0) / pageSize));
        const start = (page - 1) * pageSize + (items.length ? 1 : 0);
        const end = start + items.length - 1;

        if (productsMeta) productsMeta.innerHTML = `Mostrando ${start || 0}-${end || 0} de ${total} productos`;

        const table = tableContainer.querySelector('table.products-table') || tableContainer.querySelector('table');
        if (!table) { tableContainer.innerHTML = '<div class="products-error">Table element missing in DOM.</div>'; return; }

        let tbody = table.querySelector('tbody');
        if (!tbody) { tbody = document.createElement('tbody'); table.appendChild(tbody); }

        const baseRaw = (window.appBasePath || '');
        const base = baseRaw.replace(/\/$/, '');

        if (!items || items.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6">No hay productos</td></tr>';
        } else {
            let rows = '';
            for (const p of items) {
                const adminActions = isAdmin ? `
                      <button type="button" class="action-btn icon editBtn" title="Edit" aria-label="Edit"><i class="fas fa-edit" aria-hidden="true"></i></button>
                      <button type="button" class="action-btn icon deleteBtn" title="Delete" aria-label="Delete"><i class="fas fa-trash-alt" aria-hidden="true"></i></button>
                  ` : '';
                  
                  rows += `<tr data-id="${p.id}">
                      <td>${p.id}</td>
                      <td>${escapeHtml(p.name)}</td>
                      <td>${formatCurrency(p.price)}</td>
                      <td>${p.stock ?? 0}</td>
                      <td>${p.minStock ?? (p.minStock === 0 ? 0 : '')}</td>
                      <td class="actions">
                        <a class="action-btn icon detailsBtn" href="${base}/Product/Details/${p.id}" role="button" aria-label="View details" title="Details"><i class="fas fa-info-circle" aria-hidden="true"></i></a>
                        <button type="button" class="action-btn icon entryBtn" title="Entrada de lote" aria-label="Add batch"><i class="fas fa-box-open" aria-hidden="true"></i></button>
                        <button type="button" class="action-btn icon saleBtn" title="Salida por venta" aria-label="Sell"><i class="fas fa-shopping-cart" aria-hidden="true"></i></button>
                        ${adminActions}
                      </td>
                    </tr>`;
            }
            tbody.innerHTML = rows;
        }

        updatePagerControls();

        table.querySelectorAll('.entryBtn').forEach(b => {
            b.removeEventListener && b.removeEventListener('click', () => {});
            b.addEventListener('click', () => openBatchEntryModal(getRowId(b)));
        });
        table.querySelectorAll('.saleBtn').forEach(b => {
            b.removeEventListener && b.removeEventListener('click', () => {});
            b.addEventListener('click', () => openSaleModal(getRowId(b)));
        });
        table.querySelectorAll('.editBtn').forEach(b => {
            b.removeEventListener && b.removeEventListener('click', () => {});
            b.addEventListener('click', () => openEditModal(getRowId(b)));
        });
        table.querySelectorAll('.deleteBtn').forEach(b => {
            b.removeEventListener && b.removeEventListener('click', () => {});
            b.addEventListener('click', () => openDeleteConfirm(getRowId(b)));
        });
    }

    async function fetchPage() {
        try {
            const q = (searchInput || {}).value || '';
            const lowStockFlag = filterBtn ? (filterBtn.dataset.active === '1') : false;
            const lowParam = lowStockFlag ? '&lowStock=true' : '';
            const url = q ? `/api/product/search?q=${encodeURIComponent(q)}&page=${page}&pageSize=${DEFAULT_PAGE_SIZE}${lowParam}` : `/api/product?page=${page}&pageSize=${DEFAULT_PAGE_SIZE}${lowParam}`;
            const res = await fetch(url);
            if (!res.ok) {
                const text = await res.text().catch(() => '');
                const err = `HTTP ${res.status} ${res.statusText} ${text}`;
                showError(err);
                console.error('Fetch error:', err);
                return;
            }
            const data = await res.json();
            renderTable(data || {});
        } catch (e) {
            showError(e.message || e);
            console.error('Failed to load products page', e);
        }
    }

    function openAddModal() {
        const html = `
            <h2>Agregar Producto</h2>
            <label>Name<br/><input id="m_name" class="form-control" /></label><br/>
            <label>Price<br/><input id="m_price" class="form-control" type="number" step="0.01" /></label><br/>
            <label>Stock<br/><input id="m_stock" class="form-control" type="number" /></label><br/>
            <label>Stock Minimo<br/><input id="m_stockmin" class="form-control" type="number" /></label>
            <div style="margin-top:12px;"><button id="m_save" class="btn btn-primary">Save</button> <button id="m_cancel" class="btn">Cancel</button></div>
        `;
        const overlay = createModal(html);
        overlay.querySelector('#m_cancel').addEventListener('click', () => closeModal(overlay));
        overlay.querySelector('#m_save').addEventListener('click', async () => {
            const dto = {
                name: overlay.querySelector('#m_name').value,
                price: parseFloat(overlay.querySelector('#m_price').value || 0),
                stock: parseInt(overlay.querySelector('#m_stock').value || 0, 10),
                minStock: parseInt(overlay.querySelector('#m_stockmin').value || 0, 10)
            };
            try {
                await apiPost('/api/product', dto);
                closeModal(overlay);
                page = 1;
                await fetchPage();
            } catch (e) {
                alert('Create failed: ' + e);
            }
        });
    }

    async function openEditModal(id) {
        try {
            const dto = await apiGet(`/api/product/${id}`);
            if (!dto) { window.location.href = `/Product/Details/${id}`; return; }
            const html = `
                <h2>Edit Product #${dto.id}</h2>
                <label>Name<br/><input id="m_name" class="form-control" value="${escapeHtml(dto.name)}" /></label><br/>
                <label>Price<br/><input id="m_price" class="form-control" type="number" step="0.01" value="${dto.price}" /></label><br/>
                <label>Stock Minimo<br/><input id="m_stockmin" class="form-control" type="number" value="${dto.minStock}" /></label><br/>
                <div style="margin-top:12px;"><button id="m_save" class="btn btn-primary">Save</button> <button id="m_cancel" class="btn">Cancel</button></div>
            `;
            const overlay = createModal(html);
            overlay.querySelector('#m_cancel').addEventListener('click', () => closeModal(overlay));
            overlay.querySelector('#m_save').addEventListener('click', async () => {
                const body = {
                    id: dto.id,
                    name: overlay.querySelector('#m_name').value,
                    price: parseFloat(overlay.querySelector('#m_price').value || 0),
                    minStock: parseInt(overlay.querySelector('#m_stockmin').value || 0, 10)
                };
                try {
                    await apiPut(`/api/product/${dto.id}`, body);
                    closeModal(overlay);
                    await fetchPage();
                } catch (e) {
                    alert('Update failed: ' + e);
                }
            });
        } catch (e) {
            window.location.href = `/Product/Details/${id}`;
        }
    }

    function openDeleteConfirm(id) {
        const html = `
            <h3>Delete product #${id}?</h3>
            <div style="margin-top:12px;"><button id="m_del" class="btn btn-danger">Delete</button> <button id="m_cancel" class="btn">Cancel</button></div>
        `;
        const overlay = createModal(html);
        overlay.querySelector('#m_cancel').addEventListener('click', () => closeModal(overlay));
        overlay.querySelector('#m_del').addEventListener('click', async () => {
            try {
                await apiDelete(`/api/product/${id}`);
                closeModal(overlay);
                await fetchPage();
            } catch (e) {
                alert('Delete failed: ' + e.message);
            }
        });
    }

    function openBatchEntryModal(id) {
        const html = `
            <h3>Entrada de lote - Producto #${id}</h3>
            <label>Cantidad<br/><input id="b_qty" class="form-control" type="number" value="0" /></label><br/>
            <label>Fecha de expiración<br/><input id="b_exp" class="form-control" type="date" /></label><br/>
            <div style="margin-top:12px;"><button id="b_save" class="btn btn-primary">Agregar Lote</button> <button id="b_cancel" class="btn">Cancelar</button></div>
        `;
        const overlay = createModal(html);
        overlay.querySelector('#b_cancel').addEventListener('click', () => closeModal(overlay));
        overlay.querySelector('#b_save').addEventListener('click', async () => {
            const qty = parseInt(overlay.querySelector('#b_qty').value || 0, 10);
            const expVal = overlay.querySelector('#b_exp').value;
            const dto = {
                quantity: qty,
                expirationDate: expVal ? new Date(expVal).toISOString() : null,
            };
            try {
                await apiPost(`/api/product/${id}/batch`, dto);
                closeModal(overlay);
                await fetchPage();
            } catch (e) {
                alert('Agregar lote falló: ' + e.message);
            }
        });
    }

    function openSaleModal(id) {
        const html = `
            <h3>Salida por venta - Producto #${id}</h3>
            <label>Cantidad<br/><input id="s_qty" class="form-control" type="number" value="1" /></label><br/>
            <div style="margin-top:12px;"><button id="s_save" class="btn btn-primary">Registrar Venta</button> <button id="s_cancel" class="btn">Cancelar</button></div>
        `;
        const overlay = createModal(html);
        overlay.querySelector('#s_cancel').addEventListener('click', () => closeModal(overlay));
        overlay.querySelector('#s_save').addEventListener('click', async () => {
            const qty = parseInt(overlay.querySelector('#s_qty').value || 0, 10);
            const dto = {
                quantity: qty,
            };
            try {
                await apiPost(`/api/product/${id}/sell`, dto);
                closeModal(overlay);
                await fetchPage();
            } catch (e) {
                alert('Registrar venta falló: ' + e.message);
            }
        });
    }

    // Wire up controls
    if (addBtn) addBtn.addEventListener('click', openAddModal);
    if (searchBtn) searchBtn.addEventListener('click', () => { page = 1; fetchPage(); });
    if (searchInput) searchInput.addEventListener('keydown', (e) => { if (e.key === 'Enter') { page = 1; fetchPage(); } });

    // Filter button logic moved here (handles UI and triggers reload)
    function setFilterActive(active) {
        if (!filterBtn) return;
        filterBtn.dataset.active = active ? '1' : '0';
        filterBtn.setAttribute('aria-pressed', active ? 'true' : 'false');
        if (active) {
            filterBtn.classList.add('btn-primary');
            filterBtn.innerHTML = '<i class="fa fa-bell"></i> Solo bajo stock';
        } else {
            filterBtn.classList.remove('btn-primary');
            filterBtn.innerHTML = '<i class="fa fa-bell"></i> Mostrar bajo stock';
        }
    }

    if (filterBtn) {
        // initialize from markup (default `data-active="0"`)
        setFilterActive(filterBtn.dataset.active === '1');
        filterBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const isActive = filterBtn.dataset.active === '1';
            setFilterActive(!isActive);
            // reset to first page and reload
            page = 1;
            fetchPage();
        });
    }

    attachPagerListeners();
    fetchPage();

    try {
        if (typeof signalR === 'undefined') {
            setInterval(fetchPage, 10000);
        } else {
            const connection = new signalR.HubConnectionBuilder().withUrl('/inventoryHub').withAutomaticReconnect().build();
            connection.on('ProductsChanged', () => fetchPage());
            connection.start().catch(() => setInterval(fetchPage, 10000));
        }
    } catch {
        setInterval(fetchPage, 10000);
    }
})();