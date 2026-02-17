window.confirmarSweet = async function (titulo, texto) {
    const result = await Swal.fire({
        title: titulo,
        text: texto,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, proceder',
        cancelButtonText: 'Salir'
    });
    return result.isConfirmed;
};

window.mostrarModal = (id) => {
    console.log('muestra el moda', id);
    var modal = new bootstrap.Modal(document.querySelector(id));
    modal.show();
};

window.ocultarModal = (id) => {
    var modalEl = document.querySelector(id);
    var modal = bootstrap.Modal.getInstance(modalEl);
    modal.hide();
};

window.inicializarDataTable = (tableId) => {
    const selector = tableId;

    // Verifica si ya está inicializado
    if ($.fn.DataTable.isDataTable(selector)) {
        $(selector).DataTable().clear().destroy();
    }

    setTimeout(() => {
        $(selector).DataTable({
            responsive: true,
            destroy: true,
            autoWidth: false,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json'
            }
        });
    }, 100);
};

window.initSelect2UniversidadesExtranjeras = (elementId) => {
    try {
        const selector = '#' + elementId;
        const element = $(selector);

        console.log('Intentando inicializar Select2 para:', elementId, 'Elemento encontrado:', element.length > 0);

        if (element.length === 0) {
            console.warn('Elemento no encontrado:', elementId);
            return false;
        }
        
        // Destruir Select2 si ya existe
        if (element.data('select2')) {
            element.select2('destroy');
            element.off('change.select2');
        }
        
        // Inicializar Select2
        element.select2({
            placeholder: 'Seleccione o escriba para buscar...',
            allowClear: true,
            width: '100%',
            dropdownAutoWidth: true,
            language: {
                noResults: function() {
                    return 'No se encontraron resultados';
                },
                searching: function() {
                    return 'Buscando...';
                }
            }
        });

        // Manejar el evento de cambio para Blazor
        element.on('change.select2', function (e) {
            var selectedValue = $(this).val();
            console.log('Select2 cambió a:', selectedValue);
            
            // Disparar el evento change nativo para que Blazor lo detecte
            var nativeEvent = new Event('change', { bubbles: true });
            this.dispatchEvent(nativeEvent);
        });

        // Forzar apertura del dropdown para verificar que funciona
        console.log('Select2 inicializado correctamente para:', elementId, 'Options:', element.find('option').length);
        return true;
    } catch (error) {
        console.error('Error al inicializar Select2 para', elementId, ':', error);
        return false;
    }
};

// Función para obtener el valor seleccionado del Select2
window.getSelect2Value = (elementId) => {
    const selector = '#' + elementId;
    const element = $(selector);
    if (element.length > 0 && element.data('select2')) {
        return element.val();
    }
    return null;
};

// Prematrícula: persistir que el usuario ya completó paso 3 y está en paso 4 (pago)
window.premaSetPaso4 = (solicitudId) => {
    try { localStorage.setItem('prematricula_paso4_' + solicitudId, '1'); } catch (e) {}
};
window.premaGetPaso4 = (solicitudId) => {
    try { return localStorage.getItem('prematricula_paso4_' + solicitudId) || ''; } catch (e) { return ''; }
};
window.premaClearPaso4 = (solicitudId) => {
    try { localStorage.removeItem('prematricula_paso4_' + solicitudId); } catch (e) {}
};
