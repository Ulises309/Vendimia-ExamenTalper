var pathServicios = 'http://examentalper.somee.com/';
$('document').ready(function () {

    $('.cancelar').click(function (e) {
        if (!confirm('¿Desea cancelar la operación?'))
            e.preventDefault();
    });

});