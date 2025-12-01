// javascript
// File: `OrdexIn/wwwroot/js/dashboard.js`
// Add a helper to read either camelCase or PascalCase properties.
(async function(){
    const getProp = (obj, camel, pascal, fallback) => {
        if (!obj) return fallback;
        if (obj[camel] !== undefined) return obj[camel];
        if (obj[pascal] !== undefined) return obj[pascal];
        return fallback;
    };

    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (el) el.textContent = value ?? '—';
    };

    // initial stats fetch
    try {
        const res = await fetch('/api/inventory/stats');
        if (res.ok) {
            const stats = await res.json();
            setText('productsCount', getProp(stats, 'totalProducts', 'TotalProducts', '—'));
            setText('stockCount', getProp(stats, 'totalStock', 'TotalStock', '—'));
            const invVal = getProp(stats, 'totalInventoryValue', 'TotalInventoryValue', null);
            setText('inventoryValue', invVal != null ? new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(invVal) : '—');
            setText('lowStockCount', getProp(stats, 'lowStockCount', 'LowStockCount', '—'));
        } else {
            console.warn('Stats request failed', res.status);
        }
    } catch (e) {
        console.warn('Initial stats fetch failed', e);
    }

    const txMap = {
        'OUT': { icon: 'fas fa-arrow-down', color: '#ef4444' },
        'IN' : { icon: 'fas fa-arrow-up',   color: '#10b981' },
        'MOD': { icon: 'fas fa-pen',        color: 'var(--primary)' },
        'ADD': { icon: 'fas fa-plus-circle',color: '#10b981' },
        'DEL': { icon: 'fas fa-minus-circle', color: '#ef4444' }
    };

    const timeAgo = (iso) => {
        try {
            const then = new Date(iso);
            const diff = Math.floor((Date.now() - then.getTime()) / 1000);
            if (diff < 60) return `Hace ${diff}s`;
            if (diff < 3600) return `Hace ${Math.floor(diff/60)}m`;
            if (diff < 86400) return `Hace ${Math.floor(diff/3600)}h`;
            return `Hace ${Math.floor(diff/86400)}d`;
        } catch { return new Date(iso).toLocaleString(); }
    };

    const renderRecent = (items) => {
        const ul = document.getElementById('recentMovements');
        if (!ul) return;
        ul.innerHTML = '';

        if (!items || items.length === 0) {
            const li = document.createElement('li');
            li.style.color = 'var(--muted)';
            li.style.padding = '10px 0';
            li.style.fontSize = '14px';
            li.innerHTML = '<i class="fas fa-clipboard" style="margin-right:8px"></i> No hay movimientos recientes.';
            ul.appendChild(li);
            return;
        }

        items.forEach(raw => {
            const i = raw || {};
            const txType = (getProp(i, 'transactionType', 'TransactionType', '') || '').toString().toUpperCase();
            const t = txMap[txType] || { icon: 'fas fa-info-circle', color: 'var(--muted)' };

            const li = document.createElement('li');
            li.style.borderBottom = '1px solid var(--border)';
            li.style.padding = '10px 0';
            li.style.fontSize = '14px';

            const icon = document.createElement('i');
            icon.className = t.icon;
            icon.style.color = t.color;
            icon.style.marginRight = '8px';
            li.appendChild(icon);

            const productId = getProp(i, 'productId', 'ProductId', null);
            const qty = getProp(i, 'quantity', 'Quantity', null);
            const reason = getProp(i, 'reason', 'Reason', '');
            const text = document.createElement('span');
            text.textContent = reason ? reason : (productId != null ? `Producto #${productId} — ${qty ?? ''} uds.` : 'Movimiento');
            li.appendChild(text);

            const createdAt = getProp(i, 'createdAt', 'CreatedAt', getProp(i, 'CreatedAt', 'CreatedAt', null));
            const timeSpan = document.createElement('span');
            timeSpan.style.color = 'var(--muted)';
            timeSpan.style.float = 'right';
            timeSpan.textContent = createdAt ? timeAgo(createdAt) : '';
            li.appendChild(timeSpan);

            ul.appendChild(li);
        });
    };

    // initial fetch for recent movements
    try {
        const r = await fetch('/api/inventory/recent');
        if (r.ok) {
            const recent = await r.json();
            renderRecent(recent);
        } else {
            console.warn('Failed to load recent movements', r.status);
        }
    } catch (e) {
        console.warn('Error loading recent movements', e);
    }

    // Single SignalR connection handling both events
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/inventoryHub')
            .withAutomaticReconnect()
            .build();

        connection.on('InventoryUpdated', function(stats) {
            setText('productsCount', getProp(stats, 'totalProducts', 'TotalProducts', '—'));
            setText('stockCount', getProp(stats, 'totalStock', 'TotalStock', '—'));
            const inv = getProp(stats, 'totalInventoryValue', 'TotalInventoryValue', null);
            setText('inventoryValue', inv != null ? new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(inv) : '—');
            setText('lowStockCount', getProp(stats, 'lowStockCount', 'LowStockCount', '—'));
        });

        connection.on('KardexUpdated', function(movements) {
            renderRecent(movements);
        });

        await connection.start();
        console.info('SignalR connected');
    } catch (e) {
        console.warn('SignalR connection failed, falling back to polling', e);
        setInterval(async () => {
            try {
                const r = await fetch('/api/inventory/stats');
                if (r.ok) {
                    const s = await r.json();
                    setText('productsCount', getProp(s, 'totalProducts', 'TotalProducts', '—'));
                    setText('stockCount', getProp(s, 'totalStock', 'TotalStock', '—'));
                    const inv = getProp(s, 'totalInventoryValue', 'TotalInventoryValue', null);
                    setText('inventoryValue', inv != null ? new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(inv) : '—');
                    setText('lowStockCount', getProp(s, 'lowStockCount', 'LowStockCount', '—'));
                }
            } catch {}
            try {
                const rr = await fetch('/api/inventory/recent');
                if (rr.ok) renderRecent(await rr.json());
            } catch {}
        }, 10000);
    }

})();
