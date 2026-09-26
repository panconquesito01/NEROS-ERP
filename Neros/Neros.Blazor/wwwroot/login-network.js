class RedAcceso extends HTMLElement {
    connectedCallback() {
        this.control = new AbortController();
        this.canvas = document.createElement('canvas');
        this.canvas.setAttribute('aria-hidden', 'true');
        this.replaceChildren(this.canvas);
        this.contexto = this.canvas.getContext('2d');
        if (!this.contexto) return;

        this.boton = this.closest('.n-login').querySelector('[data-network-toggle]');
        this.reducido = matchMedia('(prefers-reduced-motion: reduce)');
        this.pausado = false;
        this.visible = true;
        this.tiempo = 0;
        this.ultimo = 0;
        this.nodos = Array.from({ length: 60 }, (_, indice) => ({
            horizontal: ((indice % 10) + .2 + Math.sin(indice * 7) * .24) / 9.5,
            vertical: (Math.floor(indice / 10) + .4 + Math.cos(indice * 3) * .25) / 6,
            fase: indice * 2.4
        }));
        this.enlaces = [];
        this.nodos.forEach((nodo, indice) => {
            if (indice % 10 < 9) this.enlaces.push([indice, indice + 1]);
            if (indice < 50) this.enlaces.push([indice, indice + 10]);
            if (indice < 50 && indice % 10 < 9 && indice % 3 === 0) this.enlaces.push([indice, indice + 11]);
        });

        const opciones = { signal: this.control.signal };
        this.boton.hidden = false;
        this.boton.addEventListener('click', () => {
            this.pausado = !this.pausado;
            this.sincronizar();
        }, opciones);
        this.reducido.addEventListener('change', () => this.sincronizar(), opciones);
        document.addEventListener('visibilitychange', () => this.sincronizar(), opciones);
        this.tema = new MutationObserver(() => this.colorear());
        this.tema.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
        this.tamano = new ResizeObserver(() => this.dimensionar());
        this.tamano.observe(this);
        this.interseccion = new IntersectionObserver(([entrada]) => {
            this.visible = entrada.isIntersecting;
            this.sincronizar();
        });
        this.interseccion.observe(this);
        this.colorear();
        this.dimensionar();
        this.sincronizar();
    }

    colorear() {
        const estilo = getComputedStyle(this);
        this.acento = estilo.getPropertyValue('--accent').trim();
        this.coral = estilo.getPropertyValue('--coral').trim();
        this.dibujar();
    }

    dimensionar() {
        this.ancho = this.clientWidth;
        this.alto = this.clientHeight;
        const escala = Math.min(devicePixelRatio || 1, 2);
        this.canvas.width = Math.round(this.ancho * escala);
        this.canvas.height = Math.round(this.alto * escala);
        this.contexto.setTransform(escala, 0, 0, escala, 0, 0);
        this.dibujar();
    }

    sincronizar() {
        cancelAnimationFrame(this.frame);
        this.ultimo = 0;
        const detenido = this.pausado || this.reducido.matches;
        this.boton.disabled = this.reducido.matches;
        this.boton.setAttribute('aria-pressed', String(detenido));
        const clave = this.reducido.matches ? 'MovimientoReducido' : detenido ? 'ReanudarAnimacion' : 'PausarAnimacion';
        const etiqueta = window.nerosTexto?.(clave) ?? clave;
        this.boton.setAttribute('aria-label', etiqueta);
        this.boton.title = etiqueta;
        this.boton.querySelector('.n-icon').style.setProperty('--icon', `url('/icons/${detenido ? 'play' : 'pause'}.svg')`);
        this.dibujar();
        if (!detenido && !document.hidden && this.visible) this.frame = requestAnimationFrame(marca => this.animar(marca));
    }

    animar(marca) {
        if (!this.ultimo) this.ultimo = marca;
        const intervalo = marca - this.ultimo;
        if (intervalo >= 1000 / 30) {
            this.tiempo += Math.min(intervalo, 100) / 1000;
            this.ultimo = marca;
            this.dibujar();
        }
        this.frame = requestAnimationFrame(siguiente => this.animar(siguiente));
    }

    dibujar() {
        if (!this.ancho || !this.alto) return;
        const contexto = this.contexto;
        contexto.clearRect(0, 0, this.ancho, this.alto);
        const puntos = this.nodos.map(nodo => ({
            horizontal: nodo.horizontal * this.ancho + Math.sin(this.tiempo * .18 + nodo.fase) * 16,
            vertical: nodo.vertical * this.alto + Math.cos(this.tiempo * .14 + nodo.fase) * 20
        }));
        this.enlaces.forEach(([origen, destino], indice) => {
            const inicio = puntos[origen];
            const fin = puntos[destino];
            contexto.strokeStyle = indice % 5 === 0 ? this.coral : this.acento;
            contexto.globalAlpha = .2;
            contexto.lineWidth = 1;
            contexto.beginPath();
            contexto.moveTo(inicio.horizontal, inicio.vertical);
            contexto.lineTo(fin.horizontal, fin.vertical);
            contexto.stroke();
            if (indice % 5 !== 0) return;
            const avance = (this.tiempo * .12 + indice * .137) % 1;
            contexto.globalAlpha = .65;
            contexto.fillStyle = contexto.strokeStyle;
            contexto.beginPath();
            contexto.arc(inicio.horizontal + (fin.horizontal - inicio.horizontal) * avance,
                inicio.vertical + (fin.vertical - inicio.vertical) * avance, 2.3, 0, Math.PI * 2);
            contexto.fill();
        });
        puntos.forEach((punto, indice) => {
            contexto.globalAlpha = indice % 7 === 0 ? .65 : .35;
            contexto.fillStyle = indice % 7 === 0 ? this.coral : this.acento;
            contexto.beginPath();
            contexto.arc(punto.horizontal, punto.vertical, indice % 7 === 0 ? 3.5 : 2, 0, Math.PI * 2);
            contexto.fill();
        });
        contexto.globalAlpha = 1;
    }

    disconnectedCallback() {
        cancelAnimationFrame(this.frame);
        this.control?.abort();
        this.tema?.disconnect();
        this.tamano?.disconnect();
        this.interseccion?.disconnect();
    }
}

customElements.define('neros-red', RedAcceso);