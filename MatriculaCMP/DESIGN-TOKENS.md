# Guía de diseño – MatriculaCMP (refactor Blazor → diseño React/captura)

Documento de **FASE 0**: variables CSS y guía de estilos extraídos de la carpeta `Matricula` (React) y de la captura del formulario de Ficha Pre-matrícula. Usar como referencia para FASE 1 (layout), FASE 2 (componentes Mat*) y FASE 3/4 (formularios).

---

## 1. Paleta de colores

### Colores principales (React Sidebar / RegistrationView / captura)

| Token CSS | Valor | Uso |
|-----------|--------|-----|
| `--mat-primary` | `#78276B` | Morado principal: sidebar gradient top, encabezados de card, botón "Siguiente", iconos, bordes activos |
| `--mat-primary-dark` | `#5a1d50` | Morado oscuro: gradient bottom del sidebar (React: `#5a1d50`), variante oscura |
| `--mat-primary-gradient` | `linear-gradient(to bottom, #78276B, #5a1d50)` | Sidebar (React) |
| `--mat-primary-gradient-card` | `linear-gradient(135deg, #78276B 0%, #9333AB 100%)` | Barra superior de cards (títulos de paso) |
| `--mat-accent` | `#F8AD1D` | Amarillo: ítem activo sidebar, paso activo stepper, alertas, badges "Paso 1/5" |
| `--mat-accent-soft` | `#FFF9E6` o `#fff3cd` | Fondo de alertas (amarillo muy claro) |
| `--mat-accent-alpha-20` | `#F8AD1D20` / `rgba(248,173,29,0.13)` | Fondos suaves (iconos, mensajes) |
| `--mat-destructive` | `#dc3545` | Botón "Cerrar Sesión" (header) |
| `--mat-success` | `#22C55E` | Progreso 100%, mensajes de éxito, DDJJ validada |
| `--mat-success-bg` | `#F0FDF4` | Fondo de mensaje éxito |

### Fondos y texto

| Token | Valor | Uso |
|-------|--------|-----|
| `--mat-bg-page` | `#ffffff` | Fondo general contenido |
| `--mat-bg-page-subtle` | `#f9fafb` / `gray-50` | Fondo área contenido (React: `bg-gradient-to-b from-purple-50/30 to-white`) |
| `--mat-bg-input` | `#f3f3f5` | Fondo inputs (theme React: `--input-background`) |
| `--mat-text-primary` | `#111827` / `gray-900` | Títulos, labels importantes |
| `--mat-text-secondary` | `#6b7280` / `gray-600` | Descripciones, breadcrumb, texto ayuda |
| `--mat-text-muted` | `#9ca3af` / `gray-400` | Placeholders, íconos inactivos |
| `--mat-border` | `rgba(0,0,0,0.1)` / `#e5e7eb` | Bordes inputs, separadores |
| `--mat-border-focus` | Igual que `--mat-primary` | Borde focus/activo en inputs |

### Sidebar (específico)

| Token | Valor |
|-------|--------|
| `--mat-sidebar-bg` | `linear-gradient(180deg, #78276B 0%, #5a1d50 100%)` |
| `--mat-sidebar-text` | `#ffffff` |
| `--mat-sidebar-hover` | `rgba(255,255,255,0.1)` |
| `--mat-sidebar-active-bg` | `#F8AD1D` |
| `--mat-sidebar-active-text` | `#78276B` (o primary dark) |
| `--mat-sidebar-section` | `#F8AD1D` (títulos de sección: "SOLICITANTE") |

---

## 2. Tipografía

- **Familia**: sans-serif (en React/theme: variables por defecto; en captura: tipo Inter/Roboto/Arial).
- **Tamaños sugeridos** (alineados a theme.css y uso en RegistrationView):
  - `--mat-text-xs`: 0.75rem (12px) – texto ayuda, badges.
  - `--mat-text-sm`: 0.875rem (14px) – labels, descripciones.
  - `--mat-text-base`: 1rem (16px) – cuerpo, inputs.
  - `--mat-text-lg`: 1.125rem (18px) – títulos de card (Paso 1: …).
  - `--mat-text-xl`: 1.25rem (20px).
  - `--mat-text-2xl`: 1.5rem (24px).
  - `--mat-text-3xl`: 1.875rem (30px) – título página "FICHA PRE-MATRÍCULA".
- **Pesos**:
  - `--mat-font-normal`: 400.
  - `--mat-font-medium`: 500.
  - `--mat-font-semibold`: 600 (títulos, labels).
  - `--mat-font-bold`: 700 (totales, énfasis).

---

## 3. Espaciados

- **Padding contenido**: 24px (1.5rem) – cards, contenido principal.
- **Padding sidebar**: 16px–24px (1rem–1.5rem) horizontal; 12–16px vertical por ítem.
- **Gap entre campos**: 16px–24px (1rem–1.5rem); en grid `gap-6` (1.5rem).
- **Margin entre secciones/cards**: 24px (1.5rem) o `space-y-6`.
- **Padding botones**: 12px 24px (py-3 px-6) botón principal; `size="lg"` h-10 px-6.

---

## 4. Componentes (referencia React + captura)

### Input / Select
- Altura: 44px (h-11) o 48px (h-12) en formularios.
- Borde: 2px, gris por defecto; hover/focus: `--mat-primary`.
- Border-radius: 6–8px (`rounded-md`).
- Fondo: `--mat-bg-input` (#f3f3f5).
- Label: `text-sm font-medium/semibold text-gray-900`; asterisco rojo para requerido.

### Botón
- **Primary**: fondo `--mat-primary`, texto blanco, `rounded-md`, shadow opcional.
- **Outline**: borde 2px `--mat-primary`, texto `--mat-primary`, hover fondo gris claro.
- **Danger**: fondo `--mat-destructive`, texto blanco (Cerrar Sesión).
- **Secondary**: fondo blanco/gris muy claro, texto oscuro (Cancelar).

### Card (sección de formulario)
- Contenedor: fondo blanco, `rounded-xl` (8–12px), `shadow-lg`, borde opcional 2px `--mat-primary` en card destacada.
- **Encabezado de card**: barra superior con `--mat-primary-gradient-card`, texto blanco, padding px-6 py-4; icono en círculo `bg-white/20`.
- **Contenido**: `p-6`, grid 1/2/3 columnas según ancho.

### Stepper (pasos)
- Círculos: 64px (w-16 h-16), `rounded-full`, borde 3px.
- Completado: fondo `--mat-primary`, icono check blanco.
- Activo: fondo `--mat-accent`, icono del paso en blanco; badge "Paso n/5" con `--mat-accent`.
- Inactivo: fondo blanco, borde gris, icono gris.
- Conector: altura 4px, completado `--mat-primary`, pendiente gris (#e5e7eb).

### Alerta (importante)
- Fondo: `--mat-accent-soft` (#FFF9E6), borde izquierdo o global amarillo.
- Icono: `--mat-accent`.
- Texto: gray-800, asteriscos rojos.

---

## 5. Layout (estructura React + captura)

### Estructura global
1. **Sidebar** (izquierda): ancho 256px (w-64); gradient morado; logo arriba; ítems con icono + texto; ítem activo amarillo; Cerrar sesión abajo.
2. **Header (toolbar)** (arriba): fondo blanco, borde inferior gris; izquierda: nombre usuario + rol (icono morado/amarillo); derecha: botón Cerrar Sesión rojo; opcional: breadcrumb (Inicio > Ficha Pre-matrícula).
3. **Main**: flex-1, padding (p-8 o similar), fondo claro (`--mat-bg-page-subtle` o blanco).

### Sidebar colapsable (FASE 1)
- Expandido: ancho 256px (o 280px como en Blazor actual), icono + texto.
- Colapsado: solo iconos (ancho ~64–72px).
- Estado en `localStorage` vía JS Interop.

### Responsive
- Móvil: sidebar oculto; botón hamburguesa en toolbar que abre menú (drawer/modal); mismo estilo de ítems.

---

## 6. Sombras y bordes

- **Card**: `box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -2px rgba(0,0,0,0.1)` o `shadow-lg`.
- **Border-radius**: inputs/selects/botones 6–8px; cards 8–12px; círculos stepper 50%.
- **Input/select**: border 1px–2px; radius 6px (`rounded-md`).

---

## 7. Archivos de referencia (Matricula React)

- **Colores y layout sidebar**: `Matricula/src/app/components/Sidebar.tsx` (#78276B, #5a1d50, #F8AD1D).
- **Header**: `Matricula/src/app/components/Header.tsx` (blanco, usuario, rol, botón destructive).
- **Formulario y cards**: `Matricula/src/app/components/RegistrationView.tsx` (cards con barra morada, inputs h-11, botones, stepper, alerta).
- **Tema base**: `Matricula/src/styles/theme.css` (variables :root; en nuestro caso priorizar colores de Sidebar/RegistrationView).
- **App layout**: `Matricula/src/app/App.tsx` (flex, Sidebar + col Header + main).

---

## 8. Resumen para implementación Blazor

- Definir en `layout.css` o `site.css` las variables de la sección 1 y 2 (colores + tipografía).
- **FASE 1**: MainLayout con estructura div (sidebar + topbar + main); NavMenu con clases que usen `--mat-sidebar-*` y `--mat-accent` para activo; toolbar con usuario y Cerrar sesión; opcional breadcrumb.
- **FASE 2**: Componentes Mat* (MatInput, MatSelect, MatButton, MatCard, etc.) usando estos tokens.
- **FASE 3**: FichaPrematricula.razor con stepper, cards con barra morada, alerta amarilla y botones según esta guía, manteniendo la lógica actual.
