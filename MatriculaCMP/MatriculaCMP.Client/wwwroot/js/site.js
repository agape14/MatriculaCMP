window.confirmarSweet = async function (titulo, texto) {
    const result = await Swal.fire({
        title: titulo,
        text: texto,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
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
    setTimeout(() => {
        if (!$.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable({
                responsive: true,
                language: {
                    url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
                }
            });
        }
    }, 100);
};
