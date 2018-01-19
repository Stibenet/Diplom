<%@ Page Language="C#" Debug="false" AutoEventWireup="true" CodeFile="logIn.aspx.cs" Inherits="logIn"  MasterPageFile="~/master.master"%>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="cph_body">
    
    <div class="row">
        <div class="col-xs-4 col-xs-offset-4" style="background-color:#d7f5da; word-wrap:break-word">
            <div class="row">
                <div class="col-xs-12">
                    <h3 style="text-align:center; color:#5bc0de">Авторизация</h3>
                </div>
            </div>
            <br />
            <div class="row">
                <div class="col-xs-10 col-xs-offset-1" >
                    <asp:TextBox runat="server" ID="tb_login" CssClass="form-control" placeholder="Логин"></asp:TextBox>
                </div>
            </div>
            <br />
            <div class="row">
                <div class="col-xs-10 col-xs-offset-1" >
                    <asp:TextBox runat="server" ID="tb_pass" CssClass="form-control" placeholder="Пароль" TextMode="Password"></asp:TextBox>
                </div>
            </div>
            <br />
            <div class="row" style="text-align:center">
                <div class="col-xs-12" >
                    <asp:Button runat="server" ID="btn_auth" CssClass="btn btn-info" Text="Войти" OnClick="btn_auth_Click" />
                </div>
            </div>
            <br />
            <h4 runat="server" id="err" style="text-align:center; color:#da1616" visible="false">Заполните поля</h4>
        </div>
    </div>

</asp:Content>