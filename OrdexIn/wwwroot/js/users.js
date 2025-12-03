// javascript
// file: `OrdexIn/wwwroot/js/users.js`
(function () {
    const table = document.querySelector('.users-table');
    const addBtn = document.getElementById('addUserBtn');
    const usersMeta = document.getElementById('usersMeta');

    function escapeHtml(s) { return String(s || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#39;'); }

    async function apiFetch(url, opts) {
        const res = await fetch(url, opts);
        if (!res.ok) {
            const text = await res.text().catch(() => res.statusText);
            throw new Error(text || `HTTP ${res.status}`);
        }
        return await res.json();
    }

    async function loadUsers() {
        try {
            const data = await apiFetch('/api/users');
            renderUsers(data || []);
        } catch (e) {
            if (table) table.querySelector('tbody').innerHTML = `<tr><td colspan="2">Failed to load users: ${escapeHtml(e.message)}</td></tr>`;
            console.error(e);
        }
    }

    // Choose a Font Awesome class depending on role/isAdmin
    function roleIconClass(isAdmin, roleStr) {
        const r = (roleStr || '').toString().toLowerCase();
        if (isAdmin) return 'fas fa-user-shield';        // admin
        if (r === 'owner' || r === 'superadmin') return 'fas fa-crown';
        if (r === 'moderator' || r === 'mod') return 'fas fa-user-tie';
        return 'fas fa-user'; // default user icon
    }

    function renderUsers(items) {
        if (!table) return;
        const tbody = table.querySelector('tbody');
        if (!items.length) {
            tbody.innerHTML = '<tr><td colspan="2">No users</td></tr>';
            if (usersMeta) usersMeta.textContent = '0 users';
            return;
        }
        tbody.innerHTML = items.map(u => {
            // normalize email and admin/role fields (handle camelCase and PascalCase)
            const email = u.email || u.Email || '';
            const isAdminFlag = (typeof u.isAdmin !== 'undefined' ? u.isAdmin
                : (typeof u.IsAdmin !== 'undefined' ? u.IsAdmin : null));
            const roleRaw = (u.role || u.Role || '').toString();
            const isAdmin = isAdminFlag !== null ? !!isAdminFlag : (roleRaw.toLowerCase() === 'admin');

            // normalize user id (handle userId / UserId / id / Id)
            const rawId = u.userId || u.UserId || u.id || u.Id || '';
            const idAttr = rawId ? ` id="${escapeHtml(rawId)}"` : '';

            // determine icon class (trusted classes from mapper)
            const iconClass = roleIconClass(isAdmin, roleRaw);

            // role text (prefer explicit Role field, else admin/user)
            const roleText = roleRaw || (isAdmin ? 'admin' : 'user');

            return `<tr${idAttr}><td>${escapeHtml(email)}</td><td><i class="${iconClass}" aria-hidden="true" style="margin-right:8px;"></i>${escapeHtml(roleText)}</td></tr>`;
        }).join('');
        if (usersMeta) usersMeta.textContent = `Showing ${items.length} users`;
    }

    function createModal(html) {
        const overlay = document.createElement('div');
        overlay.className = 'pi-modal-overlay';
        overlay.innerHTML = `
            <div class="pi-modal" role="dialog" aria-modal="true">
                <div class="app-card pi-modal-card" style="position:relative; max-width:520px; margin:24px auto;">
                    <button class="pi-modal-close btn" aria-label="Close">×</button>
                    <div class="pi-modal-body">${html}</div>
                </div>
            </div>`;
        document.body.appendChild(overlay);
        overlay.querySelector('.pi-modal-close').addEventListener('click', () => overlay.remove());
        overlay.addEventListener('click', (e) => { if (e.target === overlay) overlay.remove(); });
        return overlay;
    }

    function openAddUserModal() {
        const html = `
            <h2>Create User</h2>
            <form id="modalRegisterForm" novalidate>
                <div class="form-group">
                    <label>Email<br/><input id="m_Email" name="Email" type="email" class="form-control" /></label>
                    <span data-valmsg-for="Email" class="text-danger"></span>
                </div>
                <div class="form-group">
                    <label><input id="m_IsAdmin" type="checkbox" /> Is Admin</label>
                </div>
                <div style="margin-top:12px;">
                    <button id="m_save" type="submit" class="btn btn-primary" disabled>Save</button>
                    <button id="m_cancel" type="button" class="btn">Cancel</button>
                </div>
            </form>
            <p class="small">A default password will be assigned automatically.</p>
        `;
        const overlay = createModal(html);
        const form = overlay.querySelector('#modalRegisterForm');
        const emailInput = overlay.querySelector('#m_Email');
        const saveBtn = overlay.querySelector('#m_save');
        const cancelBtn = overlay.querySelector('#m_cancel');

        // Always wire cancel to close the modal
        if (cancelBtn) cancelBtn.addEventListener('click', () => overlay.remove());

        // Simple email check to enable Save (works even if external validation exists)
        function simpleCheck() {
            const e = (emailInput?.value || '').trim();
            saveBtn.disabled = !(e && e.indexOf('@') > 0);
        }

        // If external validation exists, still attach simpleCheck to keep Save usable
        if (window.initSignupValidation) {
            try { window.initSignupValidation(overlay); } catch (ex) { /* ignore */ }
        }
        emailInput.addEventListener('input', simpleCheck);
        simpleCheck();

        form.addEventListener('submit', async (ev) => {
            ev.preventDefault();
            const email = emailInput.value.trim();
            const isAdmin = !!overlay.querySelector('#m_IsAdmin').checked;
            try {
                // send property names matching DTO: Email / IsAdmin
                await apiFetch('/api/users', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Email: email, IsAdmin: isAdmin })
                });
                overlay.remove();
                await loadUsers();
            } catch (err) {
                alert('Create user failed: ' + (err.message || err));
            }
        });
    }

    if (addBtn) addBtn.addEventListener('click', openAddUserModal);
    loadUsers();
})();