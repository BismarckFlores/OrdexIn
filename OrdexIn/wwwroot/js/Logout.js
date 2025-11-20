// File: OrdexIn/wwwroot/js/logout.js
document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('logoutBtn');
    if (!btn) return;

    async function logout() {
        try {
            // read antiforgery token injected by @Html.AntiForgeryToken()
            const tokenInput = document.querySelector('#logoutTokenForm input[name="__RequestVerificationToken"]');
            const token = tokenInput ? tokenInput.value : null;

            // If token missing, fallback to submitting the hidden form (will navigate)
            if (!token) {
                document.getElementById('logoutTokenForm').submit();
                return;
            }

            const res = await fetch('/logout', {
                method: 'POST',
                credentials: 'same-origin', // send cookies
                headers: {
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: '' // no body needed; token is in header
            });

            if (res.ok) {
                window.location.href = '/login';
            } else {
                console.error('Logout failed, status:', res.status);
                // fallback to form submit if needed:
                // document.getElementById('logoutTokenForm').submit();
            }
        } catch (err) {
            console.error('Logout error:', err);
        }
    }

    btn.addEventListener('click', logout);
});