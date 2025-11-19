document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const toggleBtn = document.getElementById('sidebarToggle');
    const body = document.body;

    // Establecer el estado inicial si no lo haces en el HTML
    body.classList.add('sidebar-closed');
    sidebar.classList.add('collapsed');
    toggleBtn.querySelector('i').classList.add('fa-chevron-right');
    toggleBtn.querySelector('i').classList.remove('fa-chevron-left');


    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            // Alternar las clases
            sidebar.classList.toggle('collapsed');
            body.classList.toggle('sidebar-closed');

            // Cambiar el ícono del botón y la dirección
            const icon = toggleBtn.querySelector('i');
            if (sidebar.classList.contains('collapsed')) {
                icon.classList.remove('fa-chevron-left');
                icon.classList.add('fa-chevron-right');
            } else {
                icon.classList.remove('fa-chevron-right');
                icon.classList.add('fa-chevron-left');
            }
        });
    }
});