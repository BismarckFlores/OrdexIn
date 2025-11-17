// Lógica de Login/Sign Up y Validación - wwwroot/js/login-toggle.js

document.addEventListener('DOMContentLoaded', function () {
    const authForm = document.getElementById('authForm');

    // Solo necesitamos estos elementos si la página es la de Registro (SignUp)
    if (authForm) {
        const passwordInput = document.getElementById('Password');
        const confirmPasswordInput = document.getElementById('ConfirmPassword');

        // Usamos los IDs de los <span> donde mostramos los errores
        const passwordError = document.getElementById('pwdError');
        const confirmError = document.getElementById('confirmError');

        // ===================================================
        // Función de validación de fortaleza de contraseña
        // ===================================================
        function isStrongClient(pwd) {
            if (!pwd || pwd.length < 8) return { ok: false, msg: "Al menos 8 caracteres." };
            if (!/[A-Z]/.test(pwd)) return { ok: false, msg: "Al menos una mayúscula." };
            if (!/[a-z]/.test(pwd)) return { ok: false, msg: "Al menos una minúscula." };
            if (!/[0-9]/.test(pwd)) return { ok: false, msg: "Al menos un dígito." };
            if (!/[^A-Za-z0-9]/.test(pwd)) return { ok: false, msg: "Al menos un carácter especial." };
            return { ok: true, msg: "" };
        }

        // ===================================================
        // Listener para la validación al enviar el formulario
        // ===================================================
        authForm.addEventListener('submit', function (e) {

            const pwd = passwordInput.value;
            const confirmPwd = confirmPasswordInput.value;
            const pwdResult = isStrongClient(pwd);

            // Limpiar errores
            passwordError.textContent = '';
            confirmError.textContent = '';
            let hasError = false;

            // 1. Verificar fortaleza
            if (!pwdResult.ok) {
                e.preventDefault();
                passwordError.textContent = 'Contraseña débil: ' + pwdResult.msg;
                hasError = true;
            }

            // 2. Verificar coincidencia
            if (pwd !== confirmPwd) {
                e.preventDefault();
                confirmError.textContent = 'Las contraseñas no coinciden.';
                hasError = true;
            }

            return !hasError;

             // Permite el envío al servidor
        });
    }
});