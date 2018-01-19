using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FormEntity.Models;
using System.Collections;
using System.DirectoryServices;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Params["login"] == null || Request.Params["login"].Length == 0 )
        {
            Response.Write("Ошибка запуска!! Перезайдите");
            Response.End();
        }

        string 
            //mylogin = "n_bogdanov",
            //mylogin = "ovg",
            //mylogin = "sss",
              mylogin = Request.Params["login"].ToString(),
                podr = "", status = "";
        User myUser;
        List<User> oUsers;
        ControlData contrl = new ControlData();
        ulogin.Value = mylogin;
        
        using (ChatContext cc = new ChatContext())
        {              
            //myUser = cc.Users.Include("Group").Where(u=>u.login.Equals(mylogin)).FirstOrDefault();

            myUser = cc.Users.Where(u=>u.login.Equals(mylogin)).FirstOrDefault();

            if (myUser == null)
            {
                contrl.addUser(mylogin);
                myUser = cc.Users.Where(u => u.login.Equals(mylogin)).FirstOrDefault();
            }

            oUsers = cc.Users.SortBy("fio").ToList();
            

            //Отображение всех пользователей
            if (oUsers != null)
            {
                div_users.InnerHtml = "";
                string myColor = "";
                foreach (User us in oUsers)
                {
                    podr = us.podrazdelenie == null ? "Неизвестно" : us.podrazdelenie;
                    status = us.online ? "green" : "red";

                    if (us == myUser)
                    {
                        myColor = "style=\"color:green\"";
                    }
                    else
                    {
                        myColor = "";
                    }
                    div_users.InnerHtml += "<div id=\"" + us.token + "\" > <span class=\"glyphicon glyphicon-asterisk\" aria-hidden=\"true\" style=\"color:" + status + "\"></span> &nbsp  <a class=\"user\"  title=\"" + podr + "; Почта:" + us.mail + "; Тел:" + us.phone +"\" " + myColor + ">" + us.fio + "</a></div><br/>";
                }
            }

            IEnumerable<Group> gr = cc.Groups.ToList();

            //Отображение групп пользователя
            var grs = myUser.ug.Where(ugg=>ugg.active).Select(g=>g.Group).ToList();
            /*var sd= from use in cc.Users
                    join grou in cc.Groups          */
           
            if (grs != null)
            {
                //div_group.InnerHtml = "<ul class=\"nav nav-pills nav-stacked\">";
                bool first = true;
                string descr = "";
                foreach (var na in grs)
                {
                    if (na.single)
                    {
                        descr = na.ug.Select(us=>us.User).ToList().Where(u => u.Id != myUser.Id).First().fio;
                    }
                    else
                    {
                        descr = na.descript;
                    }

                    if (first)
                    {
                        ul_group.InnerHtml += "<li role=\"presentation\" class=\"active\" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href=\"#\" id=\"" + na.Id + "\" onclick=\"toggleActiveGroup(this)\">" + descr + "<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>" + "</a></li>";
                        title_group.InnerText = descr;
                        first = false;
                    }
                    else
                    {
                        ul_group.InnerHtml += "<li role=\"presentation\" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href=\"#\" id=\"" + na.Id + "\" onclick=\"toggleActiveGroup(this)\">" + descr + "<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>" + "</a></li>";
                    }                    
                }
            }
        }

        /*   Авторизация
         * bool auth = contrl.Authenticate("sss", "pass");
        pp.InnerText = auth.ToString();
          */

        /* foreach (string s in derevo)
        {
            pp.InnerText += s + "; ";
        }*/
        

    }
}