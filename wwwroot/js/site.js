// Theme Toggle Management
document.addEventListener('DOMContentLoaded', function () {
    const themeToggleBtn = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');

    function updateThemeUI(theme) {
        if (!themeIcon) return;
        if (theme === 'dark') {
            themeIcon.className = 'bi bi-sun-fill text-warning';
            if (themeToggleBtn) {
                themeToggleBtn.setAttribute('title', 'Switch to Light theme');
                themeToggleBtn.setAttribute('aria-label', 'Switch to Light theme');
            }
        } else {
            themeIcon.className = 'bi bi-moon-stars-fill text-primary';
            if (themeToggleBtn) {
                themeToggleBtn.setAttribute('title', 'Switch to Dark theme');
                themeToggleBtn.setAttribute('aria-label', 'Switch to Dark theme');
            }
        }
    }

    const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
    updateThemeUI(currentTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            const activeTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
            const newTheme = activeTheme === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('campuslearn-theme', newTheme);
            updateThemeUI(newTheme);
        });
    }
});
