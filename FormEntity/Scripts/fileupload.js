$(document).ready(function () {
    var dropZone = $('#dropZone'),
        maxFileSize = 5000000; // максимальный размер файла - 5 мб.

    if (typeof (window.FileReader) == 'undefined') {
        dropZone.text('Обновите браузер!');
        dropZone.addClass('error');
    }

    window.ondragover = function () {
        dropZone.addClass('hover');
        return false;
    };

    window.ondragleave = function () {
        dropZone.removeClass('hover');
        return false;
    };

    dropZone[0].ondragover = function () {
        dropZone.addClass('hoverOk');
        return false;
    };

    dropZone[0].ondragleave = function () {
        dropZone.removeClass('hoverOk');
        return false;
    };

    dropZone[0].ondrop = function (event) {
        event.preventDefault();
        dropZone.removeClass('hover');
        dropZone.removeClass('hoverOk');

        for (var i = 0; i < event.dataTransfer.files.length; i++){

            var file = event.dataTransfer.files[i];

            if (file.size > maxFileSize) {
                alert("Файл " + file.name + " не загружен, т.к. его размер привышает 5Мб");
                continue;
                //return false;
            }

            var xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('progress', uploadProgress, false);
            xhr.onreadystatechange = stateChange;
            xhr.open('POST', '/upload.aspx', true);
            xhr.setRequestHeader("X-File-Name", unescape(encodeURIComponent(file.name)));
            xhr.send(file);
       }
    };


    function uploadProgress(event) {
        var percent = parseInt(event.loaded / event.total * 100);
        dropZone.text('Загрузка: ' + percent + '%');
    }


    function stateChange(event) {
        if (event.target.readyState == 4) {
            
            if (event.target.status == 200) {
                var newfile = decodeURIComponent(escape(event.target.getResponseHeader("filename")));
              //  dropZone.removeClass('hoverOk');
              //  dropZone.removeClass('hover');

                var div_id = newfile.replace(new RegExp('[.]', 'g'), '_tchk_');

                var block = $('#itemFile').html().replace('div_id', div_id).replace('hrefFile', "/files/" + newfile).replace('filesave', newfile);
                
                $('#inFiles').append(block);

                /*dropZone.text('Загрузка успешно завершена!');
                dropZone.addClass('drop');*/
            } else {
                alert("При загрузке файла произошла ошибка!!");
                /*dropZone.text('Произошла ошибка!');
                dropZone.addClass('error');*/
            }
            dropZone.text("Перетащить файлы сюда");
        }
    }

    $(window).on('beforeunload', function (e) {
        var allFile = $('i.remFile');
        for (var i = 0; i < allFile.length; i++) {
            remFile(allFile[i]);
        }
    });

});

function remFile(obj) {
    var fileName = $(obj).parent().prop('id');
    $.ajax({
        type: "POST",
        url: '/upload.aspx/remFile',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: "{'filename': '" + fileName + "'}",
        async: false, cache: false,
        success: function (data) {
            $('#'+fileName).remove();
        }
    });
}
