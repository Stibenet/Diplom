var chat;
$(function () {

   // $('#chatBody').hide();
   /// $('#loginBlock').show();

    // Ссылка на автоматически-сгенерированный прокси хаба
    chat = $.connection.chatHub;
    // Объявление функции, которая хаб вызывает при получении сообщений

    $.connection.hub.error(function (error) {
        console.log('SignalR error: ' + error);
        $.connection.hub.start();
    });

    //Получения сообщения
    chat.client.messageReceived = function (name, message, time, utoken, grId) {
        var html = "";

        var pattern = /^.*<a.*href=".*"(|download=".*")>.*<\/a>$/g
       /* var text = data[i].text;
        if (!pattern.test(text)) {
            text = htmlEncode(text);
        }*/
        if (!pattern.test(message)) {
            message = htmlEncode(message);
        }
        if (utoken == $('#utoken').val()) {
            html += '<p class="pull-right" style="text-align:right"><b>' + '<small>' + time + '</small>'
               + '</b> <br/>' + message + '</p><div class="clearfix"></div>';

            $('#cph_body_div_mes').append(html);
            $('#cph_body_div_mes').scrollTop(10000);
        } else {
            var activGrId = $('li[class=active] a').prop('id');
            if (grId == activGrId) {
                html += '<p><b>' + htmlEncode(name) + '<br/><small>' + time + '</small>'
               + '</b> <br/>' + message + '</p>';

                $('#cph_body_div_mes').append(html);
                $('#cph_body_div_mes').scrollTop(10000);
            } else {
                if ($('#cph_body_ul_group li a#' + grId).length == 0) {
                    chat.server.activateAndReturnONeGroup(grId, true);
                }
                setTimeout(function () {
                    par = $('a#' + grId);
                    var mess = par.find('span');
                    if (mess.length == 0) {
                        par.append('<span class="badge pull-right">1</span>');
                        par.parent().prop('class', 'bg-success');
                    } else {
                        mess.text(parseInt(mess.text(), 10) + 1);
                    }
                }, 1000);                
            }
            blinkTitle("*****ЧАТ ЮГУ****", "*****НОВОЕ СООБЩЕНИ****", 1000);
        }
    };

    // Функция, вызываемая при подключении нового пользователя
    chat.client.onConnected = function (token, oldToken) {
        // установка в скрытых полях имени и id текущего пользователя
        $('#cph_body_div_mes').html($('#loadIcon').html());
        $('#utoken').val(token);
        $('#cph_body_div_users').find('div[id=' + oldToken + '] span').css('color', 'green');
        $('#cph_body_div_users').find('div[id=' + oldToken + ']').prop('id', token);
        chat.server.getMsgGroup($('#cph_body_ul_group li[class=active] a').prop('id'));
     //   $('#username').val(userName);
     //   $('#header').html('<h3>Добро пожаловать, ' + userName + '</h3>');

    }

    // Добавляем нового пользователя
    chat.client.onNewUserConnected = function (oldtoken, token, podr, fio) {
        var obj = $('#cph_body_div_users').find('div[id=' + oldtoken + ']');
        if (obj.length == 0) {
            $('#cph_body_div_users').append('<div id="' + token + '" ><span class=\"glyphicon glyphicon-asterisk\" aria-hidden=\"true\" style=\"color:green\"></span> &nbsp<a class="user" onclick="createGroup(this)"  title="' + podr + '">' + fio + '</a></div><br/>');
        } else {
            $(obj).find('span').css('color', 'green');
            $(obj).prop('id', token);
        }
    }

    // Отключение пользователя
    chat.client.onUserDisconnected = function (token) {
        $('#cph_body_div_users').find('div[id=' + token + '] span').css('color', 'red');
    }

    //Получение информации о группе один к одному
    chat.client.onAddGroup = function (groupId, groupName) {
        $('#cph_body_ul_group li[class=active]').prop("class", "");
        var hideImg = '<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>';
        if ($('span.label.gr-active').text() == 'Все') {
            hideImg = '';
        }
        $('#cph_body_ul_group').append(groupName.replace('{hideImg}', hideImg));
        //$('#cph_body_ul_group').append('<li role="presentation" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href="#" id="' + groupId + '" onclick="toggleActiveGroup(this)">' + groupName + hideImg + '</a></li>');
        toggleActiveGroup($('a#' + groupId));
    }

    //Открывает существующую беседу
    chat.client.onOpenChat = function (grName) {
        var obj = $('a#' + grName);
        if (obj.length == 0) {
            $.when(chat.server.toogleModeGroup(grName, true)).then(function () {
                obj = $('a#' + grName);
                toggleActiveGroup(obj);
            });
            /*chat.server.toogleModeGroup(grName, true);
            obj = $('a#' + grName);*/
        } else {
            toggleActiveGroup(obj);
        }
    }

    //Получает отображаемые группы (Все, активные)
    chat.client.sendGroups = function (groups) {
        $('#cph_body_ul_group').html(groups);
        setGroupDescript('Пусто');
        $('#div_user_group_list').html("<span class=\"label\"> </span>");
        $('#cph_body_div_mes').html('');        
    }

    //Отображение пользователей в модальном окне
    chat.client.sendUserlist = function (users, add) {
        $('#div_user_area').html("");
        for (i = 0; i < users.length; i++) {
            $('#div_user_area').append('<div class="input-group"> <span class="input-group-addon"> <input type="checkbox" id="mchb_' + users[i].Token + '" onclick=chb_modal(this) /> </span> <label class="form-control" >' + users[i].FIO + '</label></div><br/>');
        }
        if (add == true) {
            $('#btn_createGroup').css('display', 'none');
            $('#btn_addUSerInGroup').css('display', '');
            $('#tb_groupName').val($('#cph_body_ul_group li[class=active]').text());
        } else {
            $('#btn_createGroup').css('display', '');
            $('#btn_addUSerInGroup').css('display', 'none');
            $('#tb_groupName').val('');
        }
        $('#myModal').modal('show');
    }

    //Получение созданной гуппы с несколькими пользователями
    chat.client.sendPrivateGroup = function (grName, grId, grSingle, utoken) {
        //  $('#cph_body_ul_group li[class=active]').prop("class", "");
        var hideImg = '<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>';
        if ($('span.label.gr-active').text() == 'Все') {
            if (grSingle === true) {
                hideImg = '';
            } else {
                hideImg = '<i aria-hidden=\"true\" class=\"fa fa-sign-out pull-right out-group\" onclick=\"outGroup(this); event.cancelBubble = true\" title=\"Покинуть беседу\"></i>';
            }
        }
        $('#cph_body_ul_group').append('<li role="presentation" class="bg-success" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href="#" id="' + grId + '" onclick="toggleActiveGroup(this)">' + grName + hideImg + '</a></li>');
        if (utoken == $('#utoken').val()) {
            toggleActiveGroup($('a#' + grId));
            $('#myModal').modal('hide');
            clearModal();
        }
    };

    //Получение информации о новых добавленных пользователях в группу
    chat.client.sendNewUsersInPrivateGroup = function (grName, grId, grSingle, utoken, newUsers) {
        var textUser = ""
        for (var i = 0; i < newUsers.length; i++) {
            if (newUsers[i].token == $('#utoken').val()) {
                var hideImg = '<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>';
                if ($('span.label.gr-active').text() == 'Все') {
                    if (grSingle === true) {
                        hideImg = '';
                    } else {
                        hideImg = '<i aria-hidden=\"true\" class=\"fa fa-sign-out pull-right out-group\" onclick=\"outGroup(this); event.cancelBubble = true\" title=\"Покинуть беседу\"></i>';
                    }
                }
                $('#cph_body_ul_group').append('<li role="presentation" class="bg-success" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href="#" id="' + grId + '" onclick="toggleActiveGroup(this)">' + grName + hideImg + '</a></li>');
            }
            textUser += '<label id="info_' + newUsers[i].token + '">' + newUsers[i].fio + ', </label>';
        }

        if ($('#cph_body_ul_group li[class=active] a').prop('id') == grId) {
            var labels = $('div#div_user_group_list span label')
            $(textUser).insertAfter(labels[labels.length - 1]);
        }

        if (utoken == $('#utoken').val()) {
            toggleActiveGroup($('a#' + grId));
            $('#myModal').modal('hide');
            clearModal();
        }
    };

    //Получает html одной активной беседы
    chat.client.sendOneItemGroup = function (item) {
        $('#cph_body_ul_group').append(item);
    }


    //Получает сообщения группы
    chat.client.sendGroupMessage = function (data, group) {
        $('#cph_body_div_mes').text('');
        
        if (data != null) {
            var html = "";
            for (var i = 0; i < data.length; i++) {

                var pattern = /^.*<a.*href=".*"(|download=".*")>.*<\/a>$/g
                var text = data[i].text;
                if (!pattern.test(text)) {
                    text = htmlEncode(text);
                }

                if (data[i].utoken == $('#utoken').val()) {
                    html += '<p class="pull-right" style="text-align:right"><b>' + '<small>' + data[i].time + '</small>'
                       + '</b> <br/>' + text + '</p><div class="clearfix"></div>';
                } else {
                    html += '<p><b>' + htmlEncode(data[i].fio) + '<br/><small>' + data[i].time + '</small>'
                   + '</b> <br/>' + text + '</p>';
                }
            }
            $('#cph_body_div_mes').append(html);
            $('#cph_body_div_mes').scrollTop(10000);
        }
    }

    chat.client.sendUserGroupList = function (users, group) {
        showUserGroupList(users, group);
    }

    //Успешный выход из группы
    chat.client.outGroupOk = function (grId) {
        $('#div_user_group_list').html("<span class=\"label\"> </span>");
        setGroupDescript('Пусто');
        $('#cph_body_div_mes').text('');
        $('a#' + grId).parent().remove();
    }

    //Смена списка пользователей при выходе из группы одного из них
    chat.client.outGroupOkChangeUserList = function (users, group, grid) {
        if ($('#cph_body_ul_group li.active a').prop('id') == grid) {
            
            showUserGroupList(users, group);
        }
    }

    // Открываем соединение
    $.connection.hub.start().done(function () {

        chat.server.connect($('#cph_footer_ulogin').val());

        $('#btn_mes').click(function () {
            // Вызываем у хаба метод Send
            sendMsg();
        });

        $('#tb_mes').keypress(function (evn) {
            if (evn.keyCode == 13) {
                evn.preventDefault();
                sendMsg();
            }
        });

        //Создание чата один к одному
        $('.user').click(function () {
            createGroup(this);
        });

        //Получение всех пользоватлей кроме текущего
        $('#a_newGroup').click(function () {
            var token = [];
            token.push($('#utoken').val());
            chat.server.getUsers(token);
        });

        //Кнопка "Создать" группу
        $('#btn_createGroup').click(function () {
            var group = $('#tb_groupName').val();
            var obj = $('#div_user_area').find('input[type=checkbox]');
            var users = [];
          //  users.push($('#utoken').val());
            for (i = 0; i < obj.length; i++) {
                if ($(obj[i]).prop('checked') == true) {
                    var ids = $(obj[i]).prop('id').split('_')[1];
                    users.push(ids);
                }
            }
            if (group.length > 0 && users.length > 1) {
                chat.server.createPrivateGroup(group, users);
                //$('#tb_groupName').val("");
                //$('#div_user_area').find('input[type=checkbox]').prop('checked', '');
                //$('#div_user_area').find('input[type=checkbox]').parent().parent().find('label').toggleClass("list-group-item-info");
            } else {
                alert("Беседа должна иметь название и состоять минимум из 3-х пользователей");
            }
        });

        //Кнопка "Добавить" пользователя в группу
        $('#btn_addUSerInGroup').click(function () {
            var group = $('#cph_body_ul_group li[class=active] a').prop('id');
            var obj = $('#div_user_area').find('input[type=checkbox]');
            var users = [];
            for (i = 0; i < obj.length; i++) {
                if ($(obj[i]).prop('checked') == true) {
                    var ids = $(obj[i]).prop('id').split('_')[1];
                    users.push(ids);
                }
            }
            if (users.length > 0) {
                chat.server.addUserInPrivateGroup(group, users);
                //$('#tb_groupName').val("");
                //$('#div_user_area').find('input[type=checkbox]').prop('checked', '');
                //$('#div_user_area').find('input[type=checkbox]').parent().parent().find('label').toggleClass("list-group-item-info");
            } else {
                alert("Необходимо выбрать минимум одного пользователя");
            }
        });
    });
});

// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

//Создание группы одиин к одному
function createGroup(obj) {
    var token = $(obj).parent().prop('id');
    if (token.length > 0 && token != $('#utoken').val()) {
        //chat.server.createPrivateChat($('#utoken').val(), token);
        chat.server.createPrivateChat(token);
    }
}

//Отображение всех пользователей группы рядом с названием беседы
function showUserGroupList(users, group) {
    var value = "<span class=\"label\">";
    $('#div_user_group_list').text("");
   
    if (group[0].group == false) {
        for (var i = 0; i < users.length; i++) {
            value += '<label id="info_' + users[i].token + '">' + users[i].fio + ', </label>';
        }
        value += '<i class="fa fa-plus-square add_user" aria-hidden="true" title="Добавить нового пользователя в беседу" onclick="getUserGroup()"></i>';
    }
    value += " </span>";
    $('#div_user_group_list').append(value);
}

//Заменяет название беседы в формее сообщений
function setGroupDescript(name) {
    $('#cph_body_title_group').text(name);
}

//Переключает активные группы
function toggleActiveGroup(obj) {
    if ($(obj).parent().prop("class").includes("active") == false) {
        $('#cph_body_div_mes').html($('#loadIcon').html());
        $('#div_user_group_list').html("<span class=\"label\"> </span>");
        $('#cph_body_ul_group li[class=active]').prop("class", "");
        $(obj).parent().prop("class", "active");
        $(obj).find('span').remove();
        setGroupDescript($('#cph_body_ul_group li[class=active]').text());
       // $('#cph_body_div_mes').text('');
        
        var id = $(obj).prop('id');
        chat.server.getMsgGroup(id);
    }
}

//Отправляет сообщение
function sendMsg() {
    //chat.server.send($('#utoken').val(), $('#tb_mes').val(), $('li[class=active] a').prop('id'));

    var mess = $('#tb_mes').val();

    var allFile = $('#inFiles div a');
    for (var i = 0; i < allFile.length; i++) {
        if (mess.length > 0) {
            mess += "<br/>";
        }
        mess += allFile[i].outerHTML;
    }

    chat.server.send(mess, $('li[class=active] a').prop('id'));
    $('#tb_mes').val('');
    $('#inFiles').html('')
}

//Смена отображаемых бесед (Все, Активные)
function toggleModeGroup(obj) {
    if ($(obj).prop("class").includes("-active") == false) {
        $('#cph_body_ul_group').html($('#loadIcon').html());

        $('span.gr-active').toggleClass("gr");
        $('span.gr-active').toggleClass("gr-active");

        $(obj).toggleClass("gr-active");
        $(obj).toggleClass("gr");

        var groupMode = 0;
        if ($(obj).text() == "Активные") {
            groupMode = 1;
        }
        chat.server.changeGroupMode(groupMode);
    }
}

//Смена статуса у беседы
function changeModeGroup(obj) {
    var id = $(obj).prop('id');
    chat.server.toogleModeGroup(id, false);
}

//Сделать группу не активной
function hideMe(obj) {
    var paren = $(obj).parent();
    chat.server.activateAndReturnONeGroup(paren.prop('id'), false);
    $(paren).parent().remove();
    //chat.server.toogleModeGroup($(obj).parent().prop('id'), true);
}

//Запрашивает всех пользователей кроме тех кто находится в группе
function getUserGroup() {
    var users = $('#div_user_group_list span label');
    var tokens = [];
    for (var i = 0; i < users.length; i++) {
        tokens.push($(users[i]).prop('id').split('_')[1]);
    }
    chat.server.getUsers(tokens);
}

//Сбрасывает значения в модальном окне
function clearModal() {
    $('#tb_groupName').val("");
    $('#div_user_area').find('input[type=checkbox]').prop('checked', '');
    $('#div_user_area').find('input[type=checkbox]').parent().parent().find('label').toggleClass("list-group-item-info");
}

//Нажатие кнопки покинуть беседу
function outGroup(obj) {
    chat.server.leavGroup($(obj).parent().prop('id'));
}