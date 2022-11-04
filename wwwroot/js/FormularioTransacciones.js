
function InicializarFormularioTransaccion(urlObtenerCategorias) {
    $("#TipoOperacionId").change(async function () {
        const valorSeleccionado = $(this).val();
        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            body: valorSeleccionado,
            headers: {
                'Content-Type': 'application/json'
            }
        });
        const json = await respuesta.json();
        const opciones = json.map(categoria => {
            return `<option value='${categoria.value}'>${categoria.text}</option>`;
        });
        console.log(opciones);
        $("#CategoriaId").html(opciones);
    });

}
