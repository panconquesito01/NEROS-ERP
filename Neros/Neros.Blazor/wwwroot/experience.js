(() => {
    window.nerosTheme?.apply();
    if (document.querySelector('[data-resultado-unico]')) history.replaceState(null, '', location.href);

    const idioma = document.documentElement.lang || 'es';
    let textos = {};
    try { textos = JSON.parse(document.getElementById('neros-textos')?.textContent || '{}'); } catch { }
    window.nerosTexto = (clave, ...valores) =>
        (textos[clave] ?? clave).replace(/\{(\d+)\}/g, (_, indice) => String(valores[Number(indice)] ?? ''));
    const texto = window.nerosTexto;

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
            toggle.setAttribute('aria-label', texto(visible ? 'OcultarClave' : 'MostrarClave'));
            toggle.title = toggle.getAttribute('aria-label');
            const icon = toggle.querySelector('.n-icon');
            if (icon) icon.style.setProperty('--icon', `url('/icons/${visible ? 'eye-off' : 'eye'}.svg')`);
        }
        const copy = event.target.closest('[data-copy]');
        if (copy) copiar(copy);
        document.querySelectorAll('[data-language-menu][open], [data-menu][open]').forEach(menu => {
            if (!menu.contains(event.target)) menu.open = false;
        });
    });
    async function copiar(boton) {
        const origen = document.getElementById(boton.dataset.copy);
        const etiqueta = boton.querySelector('span:not(.n-icon)');
        const original = etiqueta?.textContent;
        try {
            await navigator.clipboard.writeText(origen.textContent.trim());
            if (etiqueta) etiqueta.textContent = texto('Copiado');
        } catch {
            getSelection().selectAllChildren(origen);
            if (etiqueta) etiqueta.textContent = texto('CopiaNoDisponible');
        }
        setTimeout(() => { if (etiqueta) etiqueta.textContent = original; }, 2500);
    }
    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        document.querySelectorAll('[data-language-menu][open], [data-menu][open]').forEach(menu => {
            menu.open = false;
            menu.querySelector('summary').focus();
        });
    });

    function normalize(value) {
        return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase(idioma).trim();
    }
    const companySearch = document.querySelector('[data-company-search]');
    if (companySearch) {
        const count = document.querySelector('[data-company-count]');
        const filtrar = () => {
            const query = normalize(companySearch.value);
            let matches = 0;
            document.querySelectorAll('[data-company-row]').forEach(row => {
                row.hidden = !normalize(row.dataset.companyRow).includes(query);
                if (!row.hidden) matches++;
            });
            document.querySelector('[data-company-empty]').hidden = matches !== 0;
            if (count) count.textContent = query ? texto('CoincidenciasEmpresas', matches, count.dataset.total) : '';
        };
        companySearch.addEventListener('input', filtrar);
        companySearch.addEventListener('keydown', event => {
            if (event.key === 'Escape' && companySearch.value) {
                companySearch.value = '';
                filtrar();
            }
        });
        document.addEventListener('keydown', event => {
            const editable = event.target.closest?.('input, textarea, select, [contenteditable="true"]');
            if (event.key === '/' && !editable && !event.ctrlKey && !event.metaKey && !event.altKey) {
                event.preventDefault();
                companySearch.focus();
            }
        });
        if (companySearch.value) filtrar();
    }

    const formatoLargo = new Intl.DateTimeFormat(idioma, { weekday: 'long', day: 'numeric', month: 'long' });
    const formatoCorto = new Intl.DateTimeFormat(idioma, { dateStyle: 'medium', timeStyle: 'short' });
    const formatoHora = new Intl.DateTimeFormat(idioma, { timeStyle: 'short' });
    const relativo = new Intl.RelativeTimeFormat(idioma, { numeric: 'auto' });
    function fechaRelativa(date) {
        const minutos = Math.round((Date.now() - date.getTime()) / 60000);
        if (minutos < 1) return texto('HaceUnMomento');
        if (minutos < 60) return capitalizar(relativo.format(-minutos, 'minute'));
        const hoy = new Date();
        const dias = Math.round((new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate()) - new Date(date.getFullYear(), date.getMonth(), date.getDate())) / 86400000);
        if (dias === 0) return capitalizar(relativo.format(-Math.round(minutos / 60), 'hour'));
        if (dias < 7) return `${capitalizar(relativo.format(-dias, 'day'))}, ${formatoHora.format(date)}`;
        return formatoCorto.format(date);
    }
    function capitalizar(texto) {
        return texto.charAt(0).toLocaleUpperCase(idioma) + texto.slice(1);
    }
    document.querySelectorAll('time[data-local-date]').forEach(time => {
        const date = new Date(time.dateTime);
        if (Number.isNaN(date.getTime())) return;
        const tipo = time.dataset.localDate;
        if (tipo === 'relative') {
            time.textContent = fechaRelativa(date);
            time.title = formatoCorto.format(date);
        } else {
            time.textContent = (tipo === 'long' ? formatoLargo : formatoCorto).format(date);
        }
    });
    document.querySelectorAll('.n-time-absolute').forEach(time => {
        time.hidden = !time.textContent || time.textContent === time.previousElementSibling?.textContent;
    });

    function actualizarEnvio(form, ocupado) {
        if (ocupado) form.setAttribute('aria-busy', 'true');
        else form.removeAttribute('aria-busy');
        form.querySelectorAll('button[type="submit"]').forEach(button => button.disabled = ocupado);
        form.querySelectorAll('[data-idle-label]').forEach(label => label.hidden = ocupado);
        form.querySelectorAll('[data-busy-label]').forEach(label => label.hidden = !ocupado);
    }

    document.addEventListener('submit', async event => {
        const confirmacion = event.submitter?.dataset.confirm;
        if (confirmacion && !window.confirm(confirmacion)) {
            event.preventDefault();
            return;
        }
        const form = event.target.closest('[data-busy-form]');
        if (!form) return;
        if (form.getAttribute('aria-busy') === 'true') {
            event.preventDefault();
            return;
        }
        actualizarEnvio(form, true);
        if (form.matches('[data-company-form]')) {
            const lista = form.closest('.n-company-list');
            lista?.setAttribute('data-selecting', '');
            lista?.querySelectorAll('[data-company-form] button[type="submit"]').forEach(button => button.disabled = true);
            return;
        }
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
            if (respuesta.ok) {
                let destino = '/empresas';
                if (respuesta.status !== 204 && respuesta.headers.get('content-type')?.includes('application/json')) {
                    const resultado = await respuesta.json();
                    if (typeof resultado.destino === 'string' && /^\/(?![\/\\])/.test(resultado.destino)) destino = resultado.destino;
                }
                navegar = true;
                window.location.assign(destino);
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
        document.querySelectorAll('.n-company-list[data-selecting]').forEach(lista => lista.removeAttribute('data-selecting'));
        window.nerosTheme?.apply();
    });
})();