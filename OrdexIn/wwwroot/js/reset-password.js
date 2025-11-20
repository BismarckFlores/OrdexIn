// File: OrdexIn/wwwroot/js/reset-password.js
(function () {
    function parseHashToParams(hash) {
        if (!hash) return new URLSearchParams();
        return new URLSearchParams(hash.replace(/^#/, '?'));
    }

    function showFormWithToken(token) {
        const form = document.getElementById('resetForm');
        const input = document.getElementById('accessToken');
        if (form && input) {
            input.value = token;
            form.style.display = 'block';
            const msg = document.getElementById('message');
            if (msg) msg.style.display = 'none';
        }
    }

    function redirectToExpired() {
        window.location.replace('/expired');
    }

    function redirectToLogin() {
        window.location.replace('/login');
    }

    function tokenIsExpired(expiresAtSeconds) {
        if (!expiresAtSeconds) return false;
        const nowSec = Math.floor(Date.now() / 1000);
        return nowSec >= expiresAtSeconds;
    }

    const queryParams = new URLSearchParams(window.location.search);
    const hashParams = parseHashToParams(window.location.hash);

    // If Supabase (or any provider) returned an error in hash or query, handle it.
    const err = queryParams.get('error') || hashParams.get('error');
    const errCode = queryParams.get('error_code') || hashParams.get('error_code');
    const errDesc = queryParams.get('error_description') || hashParams.get('error_description');

    if (errCode === 'otp_expired' || (err === 'access_denied' && errDesc && errDesc.toLowerCase().includes('expired'))) {
        redirectToExpired();
        return;
    }

    // Accept both 'access_token' and Supabase's 'token'
    const accessTokenQuery = queryParams.get('access_token');
    const tokenQuery = queryParams.get('token');
    const accessTokenHash = hashParams.get('access_token');
    const tokenHash = hashParams.get('token');

    const tokenInQuery = !!(accessTokenQuery || tokenQuery);
    const tokenInHash = !!(accessTokenHash || tokenHash);

    const token = accessTokenQuery || tokenQuery || accessTokenHash || tokenHash || null;
    const tokenSourceIsHash = tokenInHash && !tokenInQuery;

    const typeQuery = queryParams.get('type');
    const typeHash = hashParams.get('type');
    const tokenType = typeQuery || typeHash || null;

    const expiresAtQuery = queryParams.get('expires_at');
    const expiresInQuery = queryParams.get('expires_in');

    const expiresAtHash = hashParams.get('expires_at');
    const expiresInHash = hashParams.get('expires_in');

    let expiresAtSec = null;
    if (expiresAtQuery) {
        const v = parseInt(expiresAtQuery, 10);
        if (!Number.isNaN(v)) expiresAtSec = v;
    } else if (expiresAtHash) {
        const v = parseInt(expiresAtHash, 10);
        if (!Number.isNaN(v)) expiresAtSec = v;
    } else if (tokenSourceIsHash && expiresInHash) {
        const ei = parseInt(expiresInHash, 10);
        if (!Number.isNaN(ei)) expiresAtSec = Math.floor(Date.now() / 1000) + ei;
    }

    // If no token -> login
    if (!token) {
        redirectToLogin();
        return;
    }

    // If type is present and not 'recovery' -> login
    if (tokenType && tokenType !== 'recovery') {
        redirectToLogin();
        return;
    }

    // If we can determine expiration and it's expired -> expired view
    if (expiresAtSec && tokenIsExpired(expiresAtSec)) {
        redirectToExpired();
        return;
    }

    // If token came in hash, move it to query (map 'token' -> 'access_token') and preserve expiry/type
    if (tokenSourceIsHash) {
        const url = new URL(window.location.href);
        url.hash = '';
        url.searchParams.set('access_token', token);
        if (expiresAtSec) url.searchParams.set('expires_at', String(expiresAtSec));
        if (expiresInHash) url.searchParams.set('expires_in', expiresInHash);
        if (typeHash) url.searchParams.set('type', typeHash);
        window.location.replace(url.toString());
        return;
    }

    // Token is in query and valid/unknown -> show form
    showFormWithToken(token);
})();