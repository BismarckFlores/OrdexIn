(async function(){
    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    };

    // initial fetch
    try {
        const res = await fetch('/api/inventory/stats');
        if (res.ok) {
            const stats = await res.json();
            setText('productsCount', stats.totalProducts);
            setText('stockCount', stats.totalStock);
            setText('inventoryValue', new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(stats.totalInventoryValue));
            setText('lowStockCount', stats.lowStockCount);
        }
    } catch (e) {
        console.warn('Initial stats fetch failed', e);
    }

    // SignalR connection
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/inventoryHub')
            .withAutomaticReconnect()
            .build();

        connection.on('InventoryUpdated', function(stats) {
            setText('productsCount', stats.totalProducts);
            setText('stockCount', stats.totalStock);
            setText('inventoryValue', new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(stats.totalInventoryValue));
            setText('lowStockCount', stats.lowStockCount);
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
                    setText('productsCount', s.totalProducts);
                    setText('stockCount', s.totalStock);
                    setText('inventoryValue', new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(s.totalInventoryValue));
                    setText('lowStockCount', s.lowStockCount);
                }
            } catch {}
        }, 10000);
    }
})();