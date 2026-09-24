(() => {
    const media = matchMedia('(prefers-color-scheme: dark)');
    let preference = 'system';
    try {
        const stored = localStorage.getItem('neros.theme');
        if (['system', 'light', 'dark'].includes(stored)) preference = stored;
        document.documentElement.dataset.input = sessionStorage.getItem('neros.input') || 'pointer';
    } catch { }

    function apply() {
        document.documentElement.dataset.theme = preference === 'system' ? (media.matches ? 'dark' : 'light') : preference;
        document.querySelectorAll('[data-theme-value]').forEach(button => {
            button.setAttribute('aria-pressed', String(button.dataset.themeValue === preference));
        });
    }

    window.nerosTheme = {
        set(value) {
            if (!['system', 'light', 'dark'].includes(value)) return;
            preference = value;
            try { localStorage.setItem('neros.theme', value); } catch { }
            apply();
        },
        apply
    };
    media.addEventListener('change', apply);
    window.addEventListener('storage', event => {
        if (event.key === 'neros.theme') {
            preference = ['system', 'light', 'dark'].includes(event.newValue) ? event.newValue : 'system';
            apply();
        }
    });
    apply();
})();