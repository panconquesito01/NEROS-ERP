(() => {
    window.nerosTheme?.apply();

    function modality(value) {
        document.documentElement.dataset.input = value;
        try { sessionStorage.setItem('neros.input', value); } catch { }
    }
    document.addEventListener('keydown', () => modality('keyboard'));
    document.addEventListener('pointerdown', () => modality('pointer'));

    const emailInput = document.getElementById('correo');
    const rememberEmail = document.querySelector('[data-remember-email]');
    const rememberedEmailKey = 'neros.remembered-email';
    if (emailInput && rememberEmail) {
        try {
            const savedEmail = localStorage.getItem(rememberedEmailKey);
            if (savedEmail && savedEmail.length <= 256) {
                emailInput.value = savedEmail;
                rememberEmail.checked = true;
            }
        } catch { }
        rememberEmail.addEventListener('change', () => {
            if (!rememberEmail.checked) {
                try { localStorage.removeItem(rememberedEmailKey); } catch { }
            }
        });
        emailInput.form.addEventListener('submit', () => {
            try {
                if (rememberEmail.checked) localStorage.setItem(rememberedEmailKey, emailInput.value.trim());
                else localStorage.removeItem(rememberedEmailKey);
            } catch { }
        });
    }

    const passwordInput = document.getElementById('clave');
    const capsWarning = document.getElementById('caps-lock');
    if (passwordInput && capsWarning) {
        const updateCapsWarning = event => capsWarning.hidden = !event.getModifierState('CapsLock');
        passwordInput.addEventListener('keydown', updateCapsWarning);
        passwordInput.addEventListener('keyup', updateCapsWarning);
        passwordInput.addEventListener('blur', () => capsWarning.hidden = true);
    }

    document.addEventListener('click', event => {
        const theme = event.target.closest('[data-theme-value]');
        if (theme) window.nerosTheme?.set(theme.dataset.themeValue);
        const toggle = event.target.closest('[data-password-toggle]');
        if (toggle) {
            const input = document.getElementById(toggle.dataset.passwordToggle);
            const visible = input.type === 'password';
            input.type = visible ? 'text' : 'password';
            toggle.setAttribute('aria-pressed', String(visible));
            toggle.setAttribute('aria-label', visible ? 'Ocultar contraseña' : 'Mostrar contraseña');
            toggle.title = toggle.getAttribute('aria-label');
            const icon = toggle.querySelector('.n-icon');
            if (icon) icon.style.setProperty('--icon', `url('/icons/${visible ? 'eye-off' : 'eye'}.svg')`);
        }
    });

    function normalize(value) {
        return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es').trim();
    }
    document.querySelector('[data-company-search]')?.addEventListener('input', event => {
        const query = normalize(event.target.value);
        let matches = 0;
        document.querySelectorAll('[data-company-row]').forEach(row => {
            row.hidden = !normalize(row.dataset.companyRow).includes(query);
            if (!row.hidden) matches++;
        });
        document.querySelector('[data-company-empty]').hidden = matches !== 0;
    });

    document.querySelectorAll('time[data-local-date]').forEach(time => {
        const date = new Date(time.dateTime);
        if (Number.isNaN(date.getTime())) return;
        time.textContent = new Intl.DateTimeFormat('es', time.dataset.localDate === 'long'
            ? { weekday: 'long', day: 'numeric', month: 'long' }
            : { dateStyle: 'medium', timeStyle: 'short' }).format(date);
    });

    function actualizarEnvio(form, ocupado) {
        if (ocupado) form.setAttribute('aria-busy', 'true');
        else form.removeAttribute('aria-busy');
        form.querySelectorAll('button[type="submit"]').forEach(button => button.disabled = ocupado);
        form.querySelectorAll('[data-idle-label]').forEach(label => label.hidden = ocupado);
        form.querySelectorAll('[data-busy-label]').forEach(label => label.hidden = !ocupado);
    }

    document.addEventListener('submit', async event => {
        const form = event.target.closest('[data-busy-form]');
        if (!form) return;
        if (form.getAttribute('aria-busy') === 'true') {
            event.preventDefault();
            return;
        }
        actualizarEnvio(form, true);
        if (!form.matches('[data-login-form]')) return;
        event.preventDefault();

        const aviso = document.querySelector('[data-login-result]');
        aviso.hidden = true;
        form.querySelectorAll('[aria-invalid]').forEach(input => input.removeAttribute('aria-invalid'));
        const controlador = new AbortController();
        const cancelar = () => controlador.abort();
        const limite = setTimeout(cancelar, 20000);
        window.addEventListener('pagehide', cancelar, { once: true });
        let navegar = false;
        function mostrarError(estado) {
            aviso.classList.remove('n-success');
            aviso.classList.add('n-error');
            aviso.setAttribute('role', 'alert');
            aviso.querySelector('p').textContent = aviso.dataset[estado] || aviso.dataset.servicio;
            aviso.hidden = false;
            if (estado === 'credenciales' || estado === 'datos') {
                form.querySelectorAll('#correo, #clave').forEach(input => input.setAttribute('aria-invalid', 'true'));
            }
        }
        try {
            const respuesta = await fetch(form.action, {
                method: 'POST', body: new FormData(form), headers: { Accept: 'application/json' },
                credentials: 'same-origin', redirect: 'error', signal: controlador.signal
            });
            if (respuesta.status === 204) {
                navegar = true;
                window.location.assign('/empresas');
                return;
            }
            if (respuesta.status === 429) mostrarError('intentos');
            else if (respuesta.headers.get('content-type')?.includes('application/json')) {
                const resultado = await respuesta.json();
                mostrarError(resultado.estado);
            } else mostrarError('servicio');
        } catch {
            mostrarError('red');
        } finally {
            clearTimeout(limite);
            window.removeEventListener('pagehide', cancelar);
            if (!navegar) actualizarEnvio(form, false);
        }
    });

    window.addEventListener('pageshow', () => {
        document.querySelectorAll('[data-busy-form]').forEach(form => {
            actualizarEnvio(form, false);
        });
        window.nerosTheme?.apply();
    });
})();