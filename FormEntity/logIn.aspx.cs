using FormEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class logIn : System.Web.UI.Page
{
    string url;
    protected void Page_Load(object sender, EventArgs e)
    {
        url = Request.Url.AbsoluteUri;
        int indx = url.LastIndexOf('/') + 1;
        url = url.Remove(indx) + "Default.aspx?login=";

    }

    protected void btn_auth_Click(object sender, EventArgs e)
    {
        String login = tb_login.Text.Replace("-", "").Replace("*", "");
        if (login.Contains("\\"))
        {
            login = login.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
        }
        String pass = tb_pass.Text.Replace("-", "").Replace("*", "").Replace("\\", "\\\\");
        if (login.Length > 0 && pass.Length > 0)
        {
            ControlData contrl = new ControlData();
            bool auth = contrl.Authenticate(login, pass);
            if (auth)
            {
                url += login;
                Response.Redirect(url, true);
            }
            else
            {
                err.InnerText = "Неверный логин или пароль";
                err.Visible = true;
            }
        }
        else
        {
            err.InnerText = "Заполните поля";
            err.Visible = true;
        }
    }
}