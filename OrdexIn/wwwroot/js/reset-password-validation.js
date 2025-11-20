(function () {
    function isStrong(password) {
        if (!password) return { lengthOk: false, upperOk: false, lowerOk: false, digitOk: false, specialOk: false };
        return {
            lengthOk: password.length >= 8,
            upperOk: /[A-Z]/.test(password),
            lowerOk: /[a-z]/.test(password),
            digitOk: /[0-9]/.test(password),
            specialOk: /[^A-Za-z0-9]/.test(password)
        };
    }

    function createUpdater(elements) {
        return function updateValidation() {
            const pw = elements.newPwd ? elements.newPwd.value : '';
            const check = isStrong(pw);
            const missing = [];
            if (!check.lengthOk) missing.push('At least 8 characters');
            if (!check.upperOk) missing.push('One uppercase letter');
            if (!check.lowerOk) missing.push('One lowercase letter');
            if (!check.digitOk) missing.push('One number');
            if (!check.specialOk) missing.push('One special character');

            if (elements.newPwdError) {
                if (missing.length) {
                    elements.newPwdError.innerHTML = '<strong>Missing:</strong><ul style="margin:6px 0 0 18px;padding:0;">' +
                        missing.map(m => `<li style="margin:2px 0;">${m}</li>`).join('') + '</ul>';
                } else {
                    elements.newPwdError.textContent = '';
                }
            }

            if (elements.confirmPwd && elements.confirmPwdError) {
                if (elements.confirmPwd.value && elements.confirmPwd.value !== pw) {
                    elements.confirmPwdError.textContent = 'Passwords do not match';
                } else {
                    elements.confirmPwdError.textContent = '';
                }
            }

            const formValid = missing.length === 0 && (!elements.confirmPwd || elements.confirmPwd.value === pw) && pw.length > 0;
            if (elements.submitBtn) elements.submitBtn.disabled = !formValid;
            return formValid;
        };
    }

    function wireVisibilityButtons() {
        document.querySelectorAll('.show-pwd-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                const input = btn.closest('.input-wrapper')?.querySelector('input');
                if (!input) return;
                if (input.type === 'password') {
                    input.type = 'text';
                    btn.setAttribute('aria-pressed', 'true');
                    btn.innerHTML = '🙈';
                } else {
                    input.type = 'password';
                    btn.setAttribute('aria-pressed', 'false');
                    btn.innerHTML = '👁';
                }
            });
        });
    }

    function init(opts) {
        const form = document.getElementById(opts.formId || 'resetForm');
        if (!form) return;
        const newPwd = document.getElementById(opts.newPwdId || 'newPassword');
        const confirmPwd = document.getElementById(opts.confirmPwdId || 'confirmPassword');
        const newPwdError = document.getElementById(opts.newPwdErrorId) || document.querySelector('span[data-valmsg-for="Password"]') || document.getElementById('newPwdError');
        const confirmPwdError = document.getElementById(opts.confirmPwdErrorId) || document.querySelector('span[data-valmsg-for="ConfirmPassword"]') || document.getElementById('confirmPwdError');
        const submitBtn = document.getElementById(opts.submitBtnId || 'submitBtn');
        const accessTokenInput = document.getElementById(opts.accessTokenId || 'accessToken');
        const messageDiv = document.getElementById(opts.messageId || 'message');

        const elements = { form, newPwd, confirmPwd, newPwdError, confirmPwdError, submitBtn, accessTokenInput, messageDiv };
        const updateValidation = createUpdater(elements);

        // initial state
        if (submitBtn) submitBtn.disabled = true;
        updateValidation();

        if (newPwd) newPwd.addEventListener('input', updateValidation);
        if (confirmPwd) confirmPwd.addEventListener('input', updateValidation);

        wireVisibilityButtons();

        form.addEventListener('submit', function (e) {
            if (!updateValidation()) {
                e.preventDefault();
                if (newPwd && newPwd.value.length < 8) newPwd.focus();
                else if (confirmPwd && confirmPwd.value !== (newPwd ? newPwd.value : '')) confirmPwd.focus();
            }
        });
    }

    window.ResetPwdValidation = { init };
})();