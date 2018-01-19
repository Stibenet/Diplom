<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" MasterPageFile="~/master.master" %>

<asp:Content runat="server" ContentPlaceHolderID="cph_header">

</asp:Content>


<asp:Content runat="server" ContentPlaceHolderID="cph_body">
    
    <script src="Scripts/jquery.signalR-2.2.1.min.js"></script>
    <!--Ссылка на автоматически сгенерированный скрипт хаба SignalR -->
    <script src="signalr/hubs"></script>
    <script src="Scripts/util.js"></script>
    <script src="Scripts/fileupload.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#tb_mes').emojiPicker();

            /*
            $('li[role=presentation]').mouseenter(function () {
                $(this).find('i').css("display", 'block');
            });
            $('li[role=presentation]').mouseleave(function () {
                $(this).find('i').css("display", 'none');
            });*/
        });

        function chb_modal(obj) {
            $(obj).parent().parent().find('label').toggleClass("list-group-item-info");
        };

        function showHide(obj) {
            $(obj).find('i').css("display", 'block');
        }
        function hideHide(obj) {
            $(obj).find('i').css("display", 'none');
        }

    </script>

    <style>
        .user {
            cursor:pointer;
        }
        small {
            color:grey;
        }
        .badge {
            background-color: #5cb85c;
        }
        span.gr:hover {
            cursor:pointer;         
        }
        span.gr {
            background-color: #5bc0de;
        }
        span.gr-active {
            cursor:default;
            background-color: #5cb85c;
        }
        .emojiPickerIcon {
            top: 60px;
            right: -35px;
            width: 35px !important;
            height: 35px !important;
            background-color:transparent !important;
        }
        .emojiPicker {
            height: 210px !important;
        }
        .emojiPicker div.sections {
            height: 210px !important;
            overflow-x:hidden !important;
        }
        textarea {
            resize:none;
        }
        
        i.add_user:hover {
            cursor:pointer;
            transform :scale(1.5);
        }
        i.add_user {
            margin-left:2%;
        }
        div#div_user_group_list {
            max-height:62vh;
            overflow:hidden;
        }
        div#div_user_group_list span{
            white-space:pre-wrap;
        }
        i.hide-group, i.out-group {
            display:none;
            cursor:pointer;
        }
        i.hide-group:hover, i.out-group:hover {
            cursor:pointer;
            transform :scale(1.5);
        }
    </style>
    <br />
    <div class="row" style="width:1024px; margin:auto">
        <div class="col-xs-12">
            <div class="row">
                
                <div class="col-xs-3">
                    <div class="panel panel-primary">
                        <div class="panel-heading">Пользователи</div>
                        <div class="panel-body" runat="server" id="div_users">
                  
                        </div>
                    </div>
                </div>
        
                <div class="col-xs-6">
                    <div class="panel panel-primary">
                        <div class="panel-heading">
                            <span class="label label-success pull-left" runat="server" id="title_group">
                                Пусто
                            </span>
                            <div id="div_user_group_list">
                                <label></label>
                            </div>
                        </div>
                        <div class="panel-body default-skin" runat="server" id="div_mes" style="max-height:55vh; min-height:55vh; overflow-y:auto;">
                        </div>
              
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-xs-9 field">
                                    <%--<textarea id="tb_mes" rows="3" class="form-control" placeholder="Сообщение"></textarea>--%>
                                     <textarea id="tb_mes" rows="3" class="emojiable-option form-control " placeholder="Сообщение"></textarea>
                                </div>
                                <div class="col-xs-2">
                                    <input type="button" id="btn_mes" class="btn btn-success" value="Отправить"  />
                                </div>
                                <div id="dropZone">      
                                    Перетащить файлы сюда
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-xs-12" id="inFiles">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
        
                <div class="col-xs-3">
                    <div class="panel panel-primary">
                        <div class="panel-heading">Беседы
                            <%--<a href="#" id="a_newGroup" data-toggle="modal" data-target="#myModal" class="pull-right">--%>
                            <a href="#" id="a_newGroup"  class="pull-right">
                                <span class="glyphicon glyphicon-plus-sign" aria-hidden="true" style="color:white"></span></a>
                        </div>
                        <div class="panel-body" runat="server" id="div_group" style="padding-top:0px">
                            <div class="row" style="margin-bottom: 15px; background-color:#8cb2d41a; padding-bottom: 4px;">
                                <div class="col-xs-5 col-xs-offset-1">
                                    <span class="label gr-active" onclick="toggleModeGroup(this)">Активные</span>
                                </div>
                                <div class="col-xs-5 col-xs-offset-1"><span class="label gr" onclick="toggleModeGroup(this)">Все</span></div>
                            </div>

                            <ul class="nav nav-pills nav-stacked default-skin" runat="server" id="ul_group" style="max-height:60vh; overflow-y:auto;">
                            </ul>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>

    <%-- Модалка для создания группы --%>
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <div class="row">
                        <div class="col-xs-6 col-xs-offset-3">
                            <input id="tb_groupName" type="text" class="form-control" placeholder="Название беседы" />
                        </div>
                    </div>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-xs-12" id="div_user_area" style="max-height:600px; overflow-y:scroll">
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Отмена</button>
                    <button type="button" class="btn btn-primary" id="btn_createGroup">Создать</button>
                    <button type="button" class="btn btn-info" id="btn_addUSerInGroup" style="display:none">Добавить</button>
                </div>
            </div>
        </div>
    </div>






    <style>
       /* .dropZone {    
            color: #555;
            font-size: 18px;
            text-align: center;    
    
            width: 400px;
            padding: 50px 0;
            margin: 50px auto;
    
            background: #eee;
            border: 1px solid #ccc;
    
            -webkit-border-radius: 5px;
            -moz-border-radius: 5px;
            border-radius: 5px;
           
        }
        */

        #dropZone {
            background: #e9e9e9;
            color: #555;
            border: solid 1px #ccc;
            width: 90px;
            float: right;
            height: 42px;
            margin: 10px;
            text-align: center;  
            padding: 3px;
            font-size:12px;
        }
        
        #dropZone.hover {
            background: #ecf6ff;
            border-color: #aaa;
        }

        #dropZone.hoverOk {
            background: #e2fbc7;
            border-color: #aaa;
        }

        .dropZone.error {
            background: #faa;
            border-color: #f00;
        }

        /*.dropZone.drop {
            background: #afa;
            border-color: #0f0;
        }*/

        .remFile{
            color: #e31c1c; 
            margin-left: 1%;   
        }

        .remFile:hover {
            cursor:pointer;
            transform:scale(1.5);
        }

    </style>


  <%--  <div id="dropZone">      
        Для загрузки, перетащите файл сюда.
    </div>--%>
     
    <div id="loadIcon" style="display:none">
        <div style="text-align:center; margin-top:25%;">
            <i class="fa fa-spinner fa-pulse fa-3x fa-fw"></i>
            <span class="sr-only">Загрузка...</span>
        </div>
    </div>


    <div id="itemFile" style="display:none">
        <div id="div_id">
            <a href="hrefFile" download="filesave">Вложенный файл</a><i class="fa fa-times remFile" aria-hidden="true" title="Удалить" onclick="remFile(this)"></i>
        </div>
    </div>

</asp:Content>


<asp:Content runat="server" ContentPlaceHolderID="cph_footer">

    <input id="utoken" type="hidden" />
    <input runat="server" id="ulogin" type="hidden" />
</asp:Content>

