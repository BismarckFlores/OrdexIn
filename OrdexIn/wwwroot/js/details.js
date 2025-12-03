(function () {
    const container = document.getElementById('lotsContainer');
    const productId = container?.dataset.productId;
    if (!container) return;
    if (!productId) {
        container.innerText = 'Product id missing';
        return;
    }

    function escapeHtml(s){ return String(s ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;'); }

    async function loadLots() {
        try {
            container.innerText = 'Loading lots...';
            const url = `/api/PointOfSale/inventory/${encodeURIComponent(productId)}`;
            const res = await fetch(url);

            if (!res.ok) {
                const txt = await res.text().catch(() => res.statusText);
                container.innerText = `Failed to load lots: ${res.status} ${txt}`;
                return;
            }

            const lots = await res.json();

            if (!Array.isArray(lots) || lots.length === 0) {
                container.innerHTML = `<div>No lots found (returned ${Array.isArray(lots) ? lots.length : 'non-array'})</div><pre>${escapeHtml(JSON.stringify(lots, null, 2))}</pre>`;
                return;
            }

            let html = '<table class="products-table" role="table" aria-label="Lots"><thead><tr><th>Id</th><th>Cantidad</th><th>Expiración</th></tr></thead><tbody>';
            for (const l of lots) {
                const exp = l.expirationDate ? new Date(l.expirationDate).toLocaleDateString('es-ES') : '';
                html += `<tr>
                <td style="flex:0 0 60px;">${escapeHtml(l.id)}</td>
                <td>${escapeHtml(l.quantity)}</td>
                <td>${escapeHtml(exp)}</td>
              </tr>`;
            }
            html += '</tbody></table>';
            container.innerHTML = html;
        } catch (e) {
            container.innerText = 'Error loading lots: ' + (e && e.message ? e.message : e);
        }
    }

    loadLots();
})();