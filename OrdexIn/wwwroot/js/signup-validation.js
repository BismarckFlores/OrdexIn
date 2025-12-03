// JavaScript - file: `OrdexIn/wwwroot/js/signup-validation.js`
// Modified to expose an initializer so dynamic forms (modal) can be initialized.
(function () {
    function isStrong(password) {
        if (!password) return { lengthOk: false, upperOk: false, lowerOk: false, digitOk: false, specialOk: false };
        const lengthOk = password.length >= 8;
        const upperOk = /[A-Z]/.test(password);
        const lowerOk = /[a-z]/.test(password);
        const digitOk = /[0-9]/.test(password);
        const specialOk = /[^A-Za-z0-9]/.test(password);
        return { lengthOk, upperOk, lowerOk, digitOk, specialOk };
    }

    function isEmailValid(value) {
        if (!value) return false;
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(value);
    }

    function attachValidation(root) {
        if (!root) root = document;
        const form = root.querySelector('#authForm') || root.querySelector('#modalRegisterForm');
        if (!form) return;
        const email = form.querySelector('#Email') || form.querySelector('#m_Email');
        const pwd = form.querySelector('#Password') || form.querySelector('#m_Password');
        const confirmPwd = form.querySelector('#ConfirmPassword') || form.querySelector('#m_ConfirmPassword');
        const emailError = form.querySelector('span[data-valmsg-for="Email"]');
        const pwdError = form.querySelector('#pwdError') || form.querySelector('#m_pwdError') || form.querySelector('span[data-valmsg-for="Password"]');
        const confirmError = form.querySelector('#confirmError') || form.querySelector('#m_confirmError') || form.querySelector('span[data-valmsg-for="ConfirmPassword"]');
        const submitBtn = form.querySelector('#submitBtn') || form.querySelector('#m_save');

        function updateValidation() {
            const emailVal = email ? email.value.trim() : '';
            const pw = pwd ? pwd.value : '';
            const check = isStrong(pw);
            const missing = [];

            if (emailError) {
                if (!emailVal) emailError.textContent = 'Email is required';
                else if (!isEmailValid(emailVal)) emailError.textContent = 'Email not valid';
                else emailError.textContent = '';
            }

            if (!check.lengthOk) missing.push('At least 8 characters');
            if (!check.upperOk) missing.push('An uppercase letter');
            if (!check.lowerOk) missing.push('A lowercase letter');
            if (!check.digitOk) missing.push('A number');
            if (!check.specialOk) missing.push('A special character');

            if (pwdError) {
                if (missing.length) {
                    pwdError.innerHTML = '<strong>Missing:</strong><ul style="margin:6px 0 0 18px;padding:0;">' +
                        missing.map(m => `<li style="margin:2px 0;">${m}</li>`).join('') + '</ul>';
                } else {
                    pwdError.textContent = '';
                }
            }

            if (confirmPwd && confirmError) {
                if (confirmPwd.value && confirmPwd.value !== pw) confirmError.textContent = 'Passwords do not match';
                else confirmError.textContent = '';
            }

            const emailOk = !emailError || emailError.textContent === '';
            const formValid = emailOk && missing.length === 0 && (!confirmPwd || confirmPwd.value === pw);
            if (submitBtn) submitBtn.disabled = !formValid;
            return formValid;
        }

        // initial
        if (submitBtn) submitBtn.disabled = true;
        updateValidation();

        if (email) email.addEventListener('input', updateValidation);
        if (pwd) pwd.addEventListener('input', updateValidation);
        if (confirmPwd) confirmPwd.addEventListener('input', updateValidation);

        form.addEventListener('submit', function (e) {
            if (!updateValidation()) {
                e.preventDefault();
                if (email && email.value.trim() === '') email.focus();
                else if (pwd && pwd.value.length < 8) pwd.focus();
                else if (confirmPwd && confirmPwd.value !== (pwd ? pwd.value : '')) confirmPwd.focus();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () { attachValidation(document); });

    // expose initializer for dynamic content (modal)
    window.initSignupValidation = function (root) {
        attachValidation(root);
    };
})();