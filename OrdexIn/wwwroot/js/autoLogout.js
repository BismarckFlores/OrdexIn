(function () {
    const IDLE_MINUTES = 15;          // minutes of inactivity
    const WARNING_SECONDS = 60;       // seconds before final logout to warn
    const logoutUrl = '/Account/Logout';
    const tokenMeta = document.querySelector('meta[name="csrf-token"]');
    const csrfToken = tokenMeta ? tokenMeta.getAttribute('content') : null;

    let idleMs = IDLE_MINUTES * 60 * 1000;
    let warnMs = Math.max(0, idleMs - (WARNING_SECONDS * 1000));
    let warnTimer = null;
    let logoutTimer = null;
    let lastActivity = Date.now();

    function resetTimers() {
        lastActivity = Date.now();
        if (warnTimer) { clearTimeout(warnTimer); warnTimer = null; }
        if (logoutTimer) { clearTimeout(logoutTimer); logoutTimer = null; }
        warnTimer = setTimeout(onWarn, warnMs);
        logoutTimer = setTimeout(onLogout, idleMs);
    }

    function onWarn() {
        const stay = confirm('You will be logged out due to inactivity in ' + WARNING_SECONDS + ' seconds. Stay signed in?');
        if (stay) {
            resetTimers();
        } else {
            onLogout();
        }
    }

    async function onLogout() {
        try {
            if (csrfToken) {
                await fetch(logoutUrl, {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: {
                        'RequestVerificationToken': csrfToken,
                        'Content-Type': 'application/json'
                    },
                    body: '{}'
                });
            } else {
                await fetch(logoutUrl, { method: 'POST', credentials: 'same-origin' });
            }
        } catch (e) {
            // ignore network errors on logout
        } finally {
            window.location.href = '/';
        }
    }

    window.addEventListener('beforeunload', function () {
        try {
            if (navigator.sendBeacon && csrfToken) {
                const form = new FormData();
                form.append('__RequestVerificationToken', csrfToken);
                navigator.sendBeacon(logoutUrl, form);
            }
        } catch (e) { /* ignore */ }
    });

    ['mousemove', 'mousedown', 'keydown', 'touchstart', 'scroll', 'click'].forEach(evt =>
        window.addEventListener(evt, resetTimers, { passive: true }));

    document.addEventListener('visibilitychange', () => {
        if (!document.hidden) resetTimers();
    });

    window.resetIdleTimer = resetTimers;
    window.triggerLogoutNow = onLogout;

    resetTimers();
})();