// File: OrdexIn/wwwroot/js/signup-validation.js
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('authForm');
    if (!form) return;

    const email = document.getElementById('Email');
    const pwd = document.getElementById('Password');
    const confirmPwd = document.getElementById('ConfirmPassword');

    // target validation spans rendered by tag helpers
    const emailError = document.querySelector('span[data-valmsg-for="Email"]');
    const pwdError = document.getElementById('pwdError') || document.querySelector('span[data-valmsg-for="Password"]');
    const confirmError = document.getElementById('confirmError') || document.querySelector('span[data-valmsg-for="ConfirmPassword"]');
    const submitBtn = document.getElementById('submitBtn');

    function isStrong(password) {
        if (!password) return { lengthOk: false, upperOk: false, lowerOk: false, digitOk: false, specialOk: false };
        const lengthOk = password.length >= 8;
        const upperOk = /[A-Z]/.test(password);
        const lowerOk = /[a-z]/.test(password);
        const digitOk = /[0-9]/.test(password);
        const specialOk = /[^A-Za-z0-9]/.test(password); // at least one non-alphanumeric
        return { lengthOk, upperOk, lowerOk, digitOk, specialOk };
    }

    function isEmailValid(value) {
        if (!value) return false;
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(value);
    }

    function updateValidation() {
        const emailVal = email ? email.value.trim() : '';
        const pw = pwd ? pwd.value : '';
        const check = isStrong(pw);
        const missing = [];

        // email checks
        if (emailError) {
            if (!emailVal) {
                emailError.textContent = 'Email es requerido';
            } else if (!isEmailValid(emailVal)) {
                emailError.textContent = 'Email no válido';
            } else {
                emailError.textContent = '';
            }
        }

        // password strength checks
        if (!check.lengthOk) missing.push('Al menos 8 caracteres');
        if (!check.upperOk) missing.push('Una letra mayúscula');
        if (!check.lowerOk) missing.push('Una letra minúscula');
        if (!check.digitOk) missing.push('Un número');
        if (!check.specialOk) missing.push('Al menos un carácter especial');

        if (pwdError) {
            if (missing.length) {
                pwdError.innerHTML = '<strong>Faltan:</strong><ul style="margin:6px 0 0 18px;padding:0;">' +
                    missing.map(m => `<li style="margin:2px 0;">${m}</li>`).join('') + '</ul>';
            } else {
                pwdError.textContent = '';
            }
        }

        // confirm password
        if (confirmPwd && confirmError) {
            if (confirmPwd.value && confirmPwd.value !== pw) {
                confirmError.textContent = 'Las contraseñas no coinciden';
            } else {
                confirmError.textContent = '';
            }
        }

        const emailOk = !emailError || emailError.textContent === '';
        const formValid = emailOk && missing.length === 0 && (!confirmPwd || confirmPwd.value === pw);
        if (submitBtn) submitBtn.disabled = !formValid;
        return formValid;
    }

    // initial state
    if (submitBtn) submitBtn.disabled = true;
    updateValidation();

    if (email) email.addEventListener('input', updateValidation);
    if (pwd) pwd.addEventListener('input', updateValidation);
    if (confirmPwd) confirmPwd.addEventListener('input', updateValidation);

    // toggle password visibility buttons
    document.querySelectorAll('.show-pwd-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = btn.closest('.input-wrapper').querySelector('input');
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

    form.addEventListener('submit', function (e) {
        if (!updateValidation()) {
            e.preventDefault();
            if (email && email.value.trim() === '') email.focus();
            else if (pwd && pwd.value.length < 8) pwd.focus();
            else if (confirmPwd && confirmPwd.value !== (pwd ? pwd.value : '')) confirmPwd.focus();
        }
    });
});