// javascript
(function () {
    // Important variables
    const DEFAULT_PAGE_SIZE = 10;
    let page = 1;
    let lastTotalPages = 1;

    // DOM elements (names match Index.cshtml)
    const tableContainer = document.getElementById('kardexTable');
    const initialDateInput = document.getElementById('initialDateInput');
    const finalDateInput = document.getElementById('finalDateInput');
    const searchBtn = document.getElementById('searchBtn');

    // Pagination elements
    const prevBtns = Array.from(document.querySelectorAll('.pager-prev'));
    const nextBtns = Array.from(document.querySelectorAll('.pager-next'));
    const pageInfos = Array.from(document.querySelectorAll('.pager-info'));
    const kardexMeta = document.getElementById('kardexMeta');

    if (!tableContainer) {
        console.error('Kardex.js: missing #kardexTable element.');
        return;
    }

    const escapeHtml = (s) => String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;');
    const formatCurrency = (n) => {
        try { return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(n); } catch { return n; }
    };
    const showError = (msg) => {
        tableContainer.innerHTML = `<div class="products-error">Error loading products: ${escapeHtml(String(msg))}</div>`;
    };

    async function apiFetch(url, opts) {
        const res = await fetch(url, opts);
        if (!res.ok) {
            const text = await res.text().catch(() => res.statusText);
            throw new Error(text || `HTTP ${res.status}`);
        }
        try { return await res.json(); } catch { return null; }
    }
    const apiGet = (u) => apiFetch(u, { method: 'GET' });

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

    function formatDateTime(iso) {
        try {
            if (!iso) return '';
            const d = new Date(iso);
            if (isNaN(d.getTime())) return String(iso);

            const pad = (n) => String(n).padStart(2, '0');
            const day = pad(d.getDate());
            const month = pad(d.getMonth() + 1);
            const year = d.getFullYear();

            let hours = d.getHours();
            const minutes = pad(d.getMinutes());
            const ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            if (hours === 0) hours = 12;
            const hourStr = pad(hours);

            return `${day}/${month}/${year}<br>${hourStr}:${minutes} ${ampm}`;
        } catch {
            return iso || '';
        }
    }

    const textMap = {
        'IN':  { icon: 'fa-arrow-up',      color: '#10b981', text: 'Entrada' },
        'OUT': { icon: 'fa-arrow-down',    color: '#ef4444', text: 'Salida' },
        'MOD': { icon: 'fa-pen',           color: 'var(--primary)', text: 'Modificación' },
        'ADD': { icon: 'fa-plus-circle',   color: '#10b981', text: 'Adición' },
        'DEL': { icon: 'fa-minus-circle',  color: '#ef4444', text: 'Eliminación' }
    };

    function getType(type) {
        const t = textMap[type] || { icon: 'fa-question-circle', color: '', text: type || '' };
        const colorAttr = t.color ? ` style="color:${t.color}"` : '';
        return `<span><i class="fa ${t.icon}"${colorAttr}></i> - ${escapeHtml(t.text)}</span>`;
    }

    function renderTable(data) {
        const returnedPage = (data && typeof data.page === 'number') ? data.page : page;
        const pageSize = (data && typeof data.pageSize === 'number') ? data.pageSize : DEFAULT_PAGE_SIZE;
        page = Math.max(1, returnedPage);

        const items = data && Array.isArray(data.items) ? data.items : [];
        const total = (data && typeof data.total === 'number') ? data.total : 0;

        if (kardexMeta) kardexMeta.innerHTML = `Total Movimientos: <strong>${total}</strong>`;

        const table = tableContainer.querySelector('table.kardex-table')
        if (!table) {
            tableContainer.innerHTML = '<div class="products-error">Table element missing in DOM.</div>';
            return;
        }

        let tbody = table.querySelector('tbody');
        if (!tbody) { tbody = document.createElement('tbody'); table.appendChild(tbody); }

        if (!items || items.length === 0){
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">No hay movimientos para mostrar.</td></tr>';
        } else {
            let rows = ''
            for (const e of items) {
                // Map server DTO (camelCase) to client fields
                const id = e.kardexId ?? '';
                const type = getType(e.type ?? '');
                const rawDesc = e.description || '';
                const desc = escapeHtml(rawDesc).replace(/\r?\n/g, '<br>');
                const quantity = e.quantity != null ? Number(e.quantity).toLocaleString() : '';
                const unitCost = formatCurrency(e.unitPrice ?? 0);
                const totalCost = formatCurrency(e.total ?? (Number(e.quantity || 0) * Number(e.unitPrice || 0)));
                const dateTime = formatDateTime(e.createdAt ?? e.createdAt);
                const user = escapeHtml(e.userEmail ?? '');

                rows += `<tr data-id="${id}">
                    <td>${type}</td>
                    <td>${desc}</td>
                    <td>${quantity}</td>
                    <td>${unitCost}</td>
                    <td>${totalCost}</td>
                    <td>${dateTime}</td>
                    <td>${user}</td>
                </tr>`;
            }
            tbody.innerHTML = rows;
        }

        updatePagerControls();
    }

    async function fetchPage() {
        try {
            const init = (initialDateInput || {}).value || '';
            const fin = (finalDateInput || {}).value || '';
            const pageSizeParam = DEFAULT_PAGE_SIZE;
            let url;
            if (init && fin) {
                url = `/api/historial/search?initialDate=${encodeURIComponent(init)}&finalDate=${encodeURIComponent(fin)}&page=${page}&pageSize=${pageSizeParam}`;
            } else {
                url = `/api/historial?page=${page}&pageSize=${pageSizeParam}`;
            }

            const res = await fetch(url);
            if (!res.ok) {
                const text = await res.text().catch(() => '');
                const err = `HTTP ${res.status} ${res.statusText} ${text}`;
                showError(err);
                console.error('Fetch error:', err);
                return;
            }
            const data = await res.json();
            // update lastTotalPages based on returned total and pageSize
            const total = data && typeof data.total === 'number' ? data.total : 0;
            const pageSize = data && typeof data.pageSize === 'number' ? data.pageSize : DEFAULT_PAGE_SIZE;
            lastTotalPages = Math.max(1, Math.ceil(total / pageSize));
            renderTable(data || {});
        } catch (e) {
            showError(e.message || e);
            console.error('Failed to load historial page', e);
        }
    }

    // Wire up controls
    if (searchBtn) searchBtn.addEventListener('click', () => { page = 1; fetchPage(); });
    if (initialDateInput) initialDateInput.addEventListener('change', () => { page = 1; });
    if (finalDateInput) finalDateInput.addEventListener('change', () => { page = 1; });

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