$('document').ready(function () {

    var articulos = [];
    var clientes = [];
    var configuracion;

    obtenerConfiguracion();

    $('.cancelar').unbind();

    $('.cancelar').click(function (e) {
        if (!confirm('¿Desea cancelar la operación?'))
            e.preventDefault();
        else
            cancelarVenta();
    });

    $('form').submit(function () {
        

        if (!('input[name=rbnNumber]:checked').val()) {
            alert('Debe seleccionar un plazo para realizar el pago de su compra');
            return false;
        }
        
        return true;
    });

    $('#siguiente').click(function () {
        var cantidad = 0;
        var sinExistencia = false;
        $('.articulo').each(function () {
            cantidad += $(this).find('.cantidad').val();
        });
        if (sinExistencia) {
            alert('Los siguientes articulos con la existencia suficiente');
            return false;
        }

        if (cantidad < 1 || !$('#Cliente_Id').val()) {
            alert('Los datos ingresados no son correctos, favor de verificar');
            return false;
        }
        $('.opcionPlazo').each(function () {
            var plazo = $(this).attr('plazo');
            var totalAdeudo = $('#total').text();
            var precioContado = calcularPrecioContado(totalAdeudo);
            var totalPagar = calcularTotalPagar(precioContado, plazo)
            var importeAbono = calcularImporteAbono(totalPagar, plazo);
            var importeAhorro = calcularImporteAhorro(totalAdeudo, totalPagar);
            $(this).find('.mensualidad').html('$' + importeAbono.toFixed(2));
            $(this).find('.totalPagar').html('TOTAL A PAGAR $' + totalPagar.toFixed(2));
            $(this).find('.ahorro').html('SE AHORRA $' + importeAhorro.toFixed(2));
        });
        $('#add').attr('disabled', true);
        $('.quitar').attr('disabled', true);
        $('.cantidad').attr('disabled', true);
        $(this).attr('hidden', true);
        $('.mensualidades').attr('hidden', false);
        $('#enviar').removeAttr('hidden');
    });

    $('table').on('change', '.cantidad', function () {
        var cantidad = $(this).val();
        var precio = $(this).parent().parent().find('.precio');
        var articulo = $(this).parent().find('.id_articulo').val();
        if (!modificarArticulo(articulo, cantidad)) {
            alert('El artículo seleccionado no cuenta con existencia, favor de verificar.');
            return;
        }
        precio = precio.text();
        $(this).parent().parent().find('.importe').html(cantidad * precio);
        calcularTotales();
    });

    $('table').on('click', '.quitar', function () {
        var articulo = $(this).closest('tr').find('.id_articulo').val();
        if (!quitarArticulo(articulo)) {
            alert('Ocurrio un problema al quitar el articulo');
            return;
        }
        $(this).closest('tr').remove();
        calcularTotales();
    });

    $('#Cliente').bind('input', function () {

        for (var i = 0; i < clientes.length; i++) {
            if ($(this).val() == clientes[i].Nombre + ' ' + clientes[i].ApellidoPaterno + ' ' + clientes[i].ApellidoMaterno) {
                $('#Cliente_Id').val(clientes[i].Id_Cliente);
                $('#RFC').text(clientes[i].RFC);
                return;
            }
        }
    });

    $('#Cliente').keyup(function (e) {
        if ($(this).val().length < 3) {
            clientes = [];
            $('#listaClientes').empty();
            return;
        }

        /*
         * La primera validacion es es por si clientes esta vacio (pudiera ser que llega por primera vez a 3 caracteres)
         * la segunda y tercera son para ver si se presiono el boton retroceso o supr, esto lo hago para no tener que
         * hacer multiples peticiones ajax cuando el objecto que queremos ya se obtuvo anteriormente, cuando se borra algun
         * caracter, toda la cadena puede cambiar y entonces si hay que volver a filtrar
        */
        if (clientes.length == 0 || e.keyCode == 8 || e.keyCode == 46) {
            obtenerClientes($(this).val());
            $('#listaClientes').empty();
            for (var i = 0; i < clientes.length; i++) {
                var opt = $('<option>');
                opt.attr('Cliente_Id', clientes[i].Id_Cliente);
                opt.val(clientes[i].Nombre + ' ' + clientes[i].ApellidoPaterno + ' ' + clientes[i].ApellidoMaterno);
                $('#listaClientes').append(opt);
            }
        }


    });

    $('#add').click(function () {
        for (var i = 0; i < articulos.length; i++) {
            if ($('#Articulo').val() == articulos[i].Descripcion) {
                if (!agregarArticulo(articulos[i].Id_Articulo, 1)) {
                    alert('El artículo seleccionado no cuenta con existencia, favor de verificar.');
                    return;
                }
                var existe = false;
                $('.articulo').each(function () {
                    if ($(this).find('.id_articulo').val() == articulos[i].Id_Articulo) {
                        $(this).find('.cantidad').val(parseFloat($(this).find('.cantidad').val()) + 1);
                        existe = true;
                        var cantidad = $(this).find('.cantidad').val();
                        var precio = $(this).find('.precio');
                        precio = precio.text();
                        $(this).find('.importe').html(cantidad * precio);
                        calcularTotales();
                        return;
                    }
                });
                if (existe)
                    return;
                var tbody = $('#tablaArticulos tbody');
                var tr = $('<tr class="articulo" existencia="' + articulos[i].Existencia + '">');
                tr.append('<td>' + articulos[i].Descripcion + '</td>');
                tr.append('<td>' + articulos[i].Modelo + '</td>');
                tr.append('<td><input type="text" class="id_articulo" value="' + articulos[i].Id_Articulo + '" hidden/><input type="number" class="cantidad form-control" style="width:80px;" value="1"/></td>');
                tr.append('<td><span class="precio">' + articulos[i].Precio + '</span></td>');
                tr.append('<td class="importe">' + articulos[i].Precio + '</td>');
                tr.append('<td><button type="button" class="btn btn-danger quitar">X</button></td>');
                tbody.append(tr);
                calcularTotales();
                break;
            }
        }

    });

    $('#Articulo').keyup(function (e) {
        if ($(this).val().length < 3) {
            articulos = [];
            $('#listaArticulos').empty();
            return;
        }

        if (articulos.length == 0 || e.keyCode == 8 || e.keyCode == 46) {
            obtenerArticulos($(this).val());
            $('#listaArticulos').empty();
            for (var i = 0; i < articulos.length; i++) {
                var opt = $('<option>');
                opt.val(articulos[i].Descripcion);
                $('#listaArticulos').append(opt);
            }
        }


    });


    function obtenerClientes(busqueda) {
        $.ajax({
            url: pathServicios + '/Clientes/Buscar/' + busqueda,
            type: 'GET',
            async: false,
            success: function (response) {
                clientes = response;
            }
        });
    }

    function obtenerArticulos(busqueda) {
        $.ajax({
            url: pathServicios + '/Articulos/Buscar/' + busqueda,
            type: 'GET',
            async: false,
            success: function (response) {
                articulos = response;
            }
        });
    }

    function obtenerConfiguracion() {
        $.ajax({
            url: pathServicios + '/Configuraciones/Obtener',
            type: 'GET',
            async: false,
            success: function (response) {
                configuracion = response;
            }
        });
    }

    function agregarArticulo(id_articulo, cantidad) {
        var respuesta = false;
        $.ajax({
            url: pathServicios + '/Ventas/AgregarArticulo',
            data: {
                "ventaView.Venta_Id": $('#Id_Venta').val(),
                "ventaView.Articulo_Id": id_articulo,
                "ventaView.Cantidad": cantidad
            },
            type: 'POST',
            async: false,
            success: function (response) {
                if (response.estado)
                    respuesta = true;
            }
        });
        return respuesta;
    }

    function quitarArticulo(id_articulo) {
        var respuesta = false;
        $.ajax({
            url: pathServicios + '/Ventas/quitarArticulo',
            data: {
                "ventaView.Venta_Id": $('#Id_Venta').val(),
                "ventaView.Articulo_Id": id_articulo
                },
            type: 'POST',
            async: false,
            success: function (response) {
                if (response.estado)
                    respuesta = true;
            }
        });
        return respuesta;
    }

    function modificarArticulo(id_articulo, cantidad) {
        var respuesta = false;
        $.ajax({
            url: pathServicios + '/Ventas/modificarArticulo',
            data: {
                "ventaView.Venta_Id": $('#Id_Venta').val(),
                "ventaView.Articulo_Id": id_articulo,
                "ventaView.Cantidad": cantidad
                },
            type: 'POST',
            async: false,
            success: function (response) {
                if (response.estado)
                    respuesta = true;
            }
        });
        return respuesta;
    }

    function cancelarVenta() {
        var respuesta = false;
        $.ajax({
            url: pathServicios + '/Ventas/CancelarVenta',
            data: {
                "venta.Id_Venta": $('#Id_Venta').val()
            },
            type: 'POST',
            async: false,
            success: function (response) {
                if (response.estado)
                    respuesta = true;
            }
        });
        return respuesta;
    }

    function calcularTotales() {
        var importe = 0;
        var enganche = 0;
        var bonificacion = 0;
        $('.articulo').each(function () {
            importe += parseFloat($(this).find('.importe').html());
        });
        enganche = calcularEnganche(importe);
        bonificacion = calcularBonificacion(enganche);

        $('#enganche').text(enganche.toFixed(2));
        $('#bonificacion').text(bonificacion.toFixed(2));
        $('#total').text((importe - enganche - bonificacion).toFixed(2));
    }

    function calcularEnganche(importe) {
        return (configuracion.Enganche / 100) * importe;
    }

    function calcularBonificacion(enganche) {
        return enganche * ((configuracion.TasaFinanciamiento * configuracion.PlazoMaximo) / 100);
    }

    function calcularTotalAdeudo(importe, enganche, bonificacion) {
        return importe - enganche - bonificacion;
    }

    function calcularPrecioContado(totalAdeudo) {
        return totalAdeudo / (1 + (configuracion.TasaFinanciamiento * configuracion.PlazoMaximo) / 100);
    }

    function calcularTotalPagar(precioContado, plazo) {
        return precioContado * (1 + (configuracion.TasaFinanciamiento * plazo) / 100);
    }

    function calcularImporteAbono(totalPagar, plazo) {
        return totalPagar / plazo;
    }

    function calcularImporteAhorro(totalAdeudo, totalPagar) {
        return totalAdeudo - totalPagar;
    }
});