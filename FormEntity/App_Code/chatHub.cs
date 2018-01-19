using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FormEntity.Models;

/// <summary>
/// Сводное описание для Hub
/// </summary>
public class chatHub : Hub
{
    User myUser, mUser;
    ControlData cd;

    public chatHub()
    {
        cd = new ControlData();
    }

    public void Send(string _mes, int _grId)
    {

        if (_mes.Length > 0)
        {
            string _token = Context.ConnectionId;
            using (ChatContext cc = new ChatContext())
            {
                mUser = cc.Users.Where(u => u.token.Equals(_token, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (mUser.ug.Select(gr=>gr.Group).ToList().Any(gr => gr.Id == _grId))
                {    
                    Group gr = cc.Groups.Where(g => g.Id == _grId).First();

                    Message msg = new Message()
                    {
                        fromUser = mUser,
                        toGroup = gr,
                        GroupId = gr.Id,
                        text = _mes,
                        ftime = DateTime.Now
                    };
                    gr.msg.Add(msg);
                    cc.SaveChanges();
                    Clients.Group(gr.name).messageReceived(mUser.fio, _mes, msg.ftime.ToString("dd.MM.yy  HH:mm"), mUser.token, gr.Id);
                    // Clients.All.messageReceived(myUser.fio, _mes);
                }
            }
        }
    }

    public void Connect(string _login)
    {  
        string token, oldToken;
        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(u => u.login.Equals(_login, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            oldToken = myUser.token;

            token = Context.ConnectionId;
            Dictionary<string, object> upVal = new Dictionary<string, object>();
            upVal.Add("token", token);
            cd.updateUser(myUser.Id, upVal);

            cd.updateStatus(myUser.Id);

            // Посылаем сообщение всем пользователям, кроме текущего
           // Clients.All.onNewUserConnected(oldToken, token, myUser.podrazdelenie, myUser.fio);
            Clients.AllExcept(token).onNewUserConnected(oldToken, token, myUser.podrazdelenie, myUser.fio);
            // Посылаем сообщение текущему пользователю
            Clients.Caller.onConnected(token, oldToken);
        }
        initGroups();
    }

    // Отключение пользователя
    public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
    {
        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(u => u.token.Equals(Context.ConnectionId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            cd.updateStatus(myUser.Id, false);
            Clients.All.onUserDisconnected(myUser.token);
        }
        return base.OnDisconnected(stopCalled);
    }

    /// <summary>
    /// Создание группы один к одному
    /// </summary>
    /// <param name="_token">Токен собеседника</param>
    public void CreatePrivateChat(string _token)
    {
        User sobesednik;
        Group newGroup;
        string _myToken = Context.ConnectionId;
        using (ChatContext cc = new ChatContext())
        {
            mUser = cc.Users.Where(u => u.token == _myToken).FirstOrDefault();
            sobesednik = cc.Users.Where(x => x.token == _token).First();

            //Проверка на существующий чат
            if (mUser.ug.Select(gr => gr.Group).ToList().Any(gr => gr == sobesednik.ug.Select(g => g.Group).ToList().Where(g => g.Id == gr.Id && gr.single).FirstOrDefault()))
            {
                Group grr = mUser.ug.Select(gr => gr.Group).ToList().Where(gr => gr == sobesednik.ug.Select(g => g.Group).ToList().Where(g => g.Id == gr.Id && gr.single).FirstOrDefault()).First();
                Clients.Caller.onOpenChat(grr.Id);
                return;
            }

            newGroup = new Group() { name = generateName() };
            newGroup.single = true;

            UserGroups usgr1 = new UserGroups { User = mUser, Group = newGroup };
            UserGroups usgr2 = new UserGroups { User = sobesednik, Group = newGroup };

            cc.UsGr.Add(usgr1);
            cc.UsGr.Add(usgr2);

            /*newGroup.Users.Add(sobesednik);
            newGroup.Users.Add(mUser);
            mUser.Groups.Add(newGroup);
            sobesednik.Groups.Add(newGroup);*/

            cc.SaveChanges();
        }

        List<string> tc = new List<string> { mUser.token, sobesednik.token };
        addInGroup(newGroup);

        string item = "<li role=\"presentation\" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href=\"#\" id=\"" + newGroup.Id + "\" onclick=\"toggleActiveGroup(this)\">{groupName}{hideImg}</a></li>";

        Clients.Caller.onAddGroup(newGroup.Id, item.Replace("{groupName}", sobesednik.fio));
        Clients.Client(sobesednik.token).onAddGroup(newGroup.Id, item.Replace("{groupName}", mUser.fio));

       
       // addInGroup(tc, newGroup.name);
        //Groups.Add
    }

    /// <summary>
    /// Возвращает всех пользователей кроме текущих токенов
    /// </summary>
    /// <param name="_mytoken">Список токенов которые не возвращаются</param>
    public void getUsers(List<string> _mytoken)
    {
        bool add = false;
        if (_mytoken.Count > 1)
        {
            add = true;
        }
        using (ChatContext cc = new ChatContext())
        {
            var allUs = cc.Users.Where(u => !_mytoken.Contains(u.token)).Select(us => new {Token = us.token, FIO = us.fio}).ToArray();
       //     var allUs = cc.Users.Where(u => u.token != _mytoken).Select(us => new {Token = us.token, FIO = us.fio}).ToList();
            Clients.Caller.sendUserlist(allUs, add);
        }
    }

    /// <summary>
    /// Создание группы с несколькими пользователями
    /// </summary>
    /// <param name="_grName">Название группы</param>
    /// <param name="_users">Лист пользователей</param>
    public void createPrivateGroup(string _grName, List<string> _users) 
    {
        if (_grName != null && _grName.Length > 0 && _users.Count > 1)
        {
            Group nGr = new Group()
            {
                name = generateName(),
                descript = _grName,
                single = false
            };
            using (ChatContext cc = new ChatContext())
            {
                foreach (string user in _users)
                {
                    User us = cc.Users.Where(u => u.token == user).First();
                    if (us != null)
                    {

                        UserGroups usgr = new UserGroups { User = us, Group = nGr };
                        cc.UsGr.Add(usgr);
                        /*us.Groups.Add(nGr);
                        nGr.Users.Add(us); */
                          
                        cc.SaveChanges();
                    }
                }

                User Ius = cc.Users.Where(u => u.token == Context.ConnectionId).First();
                UserGroups usgr1 = new UserGroups { User = Ius, Group = nGr };
                cc.UsGr.Add(usgr1);
                /*Ius.Groups.Add(nGr);
                nGr.Users.Add(Ius);*/

                cc.SaveChanges();
            }
            addInGroup(nGr);
            System.Threading.Thread.Sleep(10);   //КОСТЫЛЬ
            Clients.Group(nGr.name).sendPrivateGroup(nGr.descript, nGr.Id, nGr.single, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Добавляет новых пользователей в группу
    /// </summary>
    /// <param name="_grId">ID группы</param>
    /// <param name="_users">Токены добавляемых пользователей</param>
    public void addUserInPrivateGroup(int _grId, List<String> _users)
    {
        if (_grId != null && _users != null && _users.Count > 0)
        {
            using (ChatContext cc = new ChatContext())
            {
                
                Group group = cc.Groups.Where(gr => gr.Id == _grId).First();
                if (group != null && group.ug.Select(u=>u.User).ToList().Any(us=>us.token.Equals(Context.ConnectionId)))
                {
                    List<User> newUsers = new List<User>();
                    String _mes = "Я пригласил: ";
                    foreach (string user in _users)
                    {
                        User us = cc.Users.Where(u => u.token.Equals(user)).First();
                        if (us != null)
                        {
                            UserGroups usgr = new UserGroups { User = us, Group = group };
                            cc.UsGr.Add(usgr);
                            /*us.Groups.Add(group);
                            group.Users.Add(us); */
                            cc.SaveChanges();

                            newUsers.Add(us);
                            _mes += us.fio + ", ";
                        }
                    }
                    //Оповещение всех пользователей о добавлении новых
                    mUser = cc.Users.Where(use => use.token.Equals(Context.ConnectionId)).First();
                    Send(_mes, group.Id);
                    //Clients.Group(group.name).messageReceived(mUser.fio, _mes, DateTime.Now.ToString("dd.MM.yy  HH:mm"), mUser.token, group.Id);
                    
                    //Оповещение новых пользователй
                    addInGroup(group);
                    var usersTok = newUsers.Select(us => new { token = us.token, fio = us.fio });
                    System.Threading.Thread.Sleep(10);   //КОСТЫЛЬ
                    Clients.Group(group.name).sendNewUsersInPrivateGroup(group.descript, group.Id, group.single, Context.ConnectionId, usersTok);
                }
            }
        }
    }

    /// <summary>
    /// Возвращает 50 последних сообщений группы
    /// </summary>
    /// <param name="_idGr">ИД группы</param>
    public void getMsgGroup(string _idGr)
    {
        if (_idGr != null)
        {
            bool access = false;
            using (ChatContext cc = new ChatContext())
            {
                mUser = cc.Users.Where(u => u.token.Equals(Context.ConnectionId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (mUser.ug.Select(gr=>gr.Group).ToList().Any(gr => gr.Id.ToString().Equals(_idGr)))
                {
                    //Тут выбрать всех пользователей группы и передать в showUserGroupList       !!!ПРОВЕРИТЬ
                    //var UserInGroup = cc.Users.Include("Groups").Where(us => us.ug.Select(gr => gr.Group).ToList().Any(gr => gr.Id.ToString().Equals(_idGr))).Select(use => new { fio = use.fio, token = use.token }).ToList();
                    var UserInGroup = cc.Users.Where(us => us.ug.Select(gr => gr.Group).ToList().Any(gr => gr.Id.ToString().Equals(_idGr))).Select(use => new { fio = use.fio, token = use.token }).ToList();

                    var grSingle = cc.Groups.Where(gr => gr.Id.ToString().Equals(_idGr)).Select(g => new { group = g.single });
                    //List<var>  = cc.Groups.Where(gr => gr.Id.Equals(_idGr)).Select(use => new { fio = use.Users }).ToList();
                    Clients.Caller.sendUserGroupList(UserInGroup, grSingle);

                    //int len = cc.Groups.Include("Users").Where(gr => gr.Id.ToString().Equals(_idGr) && gr.ug.Select(u => u.User).ToList().Any(us => us.token.Equals(Context.ConnectionId))).ToArray().Length;
                    int len = cc.Groups.Where(gr => gr.Id.ToString().Equals(_idGr) && gr.ug.Select(u => u.User).ToList().Any(us => us.token.Equals(Context.ConnectionId))).ToArray().Length;
                    if (len > 0)
                    {
                        access = true;
                    }
                }
                //
                // msgs = cc.Messages.Include("toGroup").Include("fromUser").Where(ms => ms.GroupId == idGr).ToList();
            }
            if (access)
            {
                List<Message> msgs = new List<Message>();
                int idGr = int.Parse(_idGr);
                using (ChatContext cc = new ChatContext())
                {
                    msgs = cc.Messages.Include("toGroup").Include("fromUser").Where(ms => ms.GroupId == idGr).ToList();
                }

                if (msgs.Count > 0)
                {
                    var data = msgs.Skip(msgs.Count - 50).Select(ms => new
                    {
                        text = ms.text,
                        time = ms.ftime.ToString("dd.MM.yy  HH:mm"),
                        fio = ms.fromUser.fio,
                        utoken = ms.fromUser.token
                    });

                    Clients.Caller.sendGroupMessage(data, msgs[0].toGroup.descript);
                }
                else
                {
                    Clients.Caller.sendGroupMessage(null);
                }
            }
        }
        else
        {
            Clients.Caller.sendGroupMessage(null);
        }
    }

    /// <summary>
    /// Возвращает группы в зависимости от режима
    /// </summary>
    /// <param name="_mode">0-Все; 1-Активные</param>
    public void changeGroupMode(int _mode)
    {
        bool all = _mode == 0 ? true : false;
        string myToken = Context.ConnectionId,
            descr = "";
        String groups = "", toogleMode = "", hideGroup = "<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i>";
        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(us=>us.token.Equals(myToken)).First();
            if(myUser != null){

                Group[] ggs;
                bool allGroup = false;
                if (all)
                {
                    ggs = myUser.ug.Select(u => u.Group).ToArray();
                    toogleMode = " changeModeGroup(this)";
                    //hideGroup = "";
                    allGroup = true;
                }
                else
                {
                    ggs = myUser.ug.Where(uug => uug.active).Select(u => u.Group).ToArray();
                }

                foreach (var grs in ggs)
                {
                    if (grs.single)
                    {
                        descr = grs.ug.Select(us => us.User).ToList().Where(u => u.Id != myUser.Id).First().fio;
                        if (allGroup)
                        {
                            hideGroup = "";
                        }
                    }
                    else
                    {
                        descr = grs.descript;
                        if (allGroup)
                        {
                            hideGroup = "<i class=\"fa fa-sign-out pull-right out-group\" aria-hidden=\"true\" title=\"Покинуть беседу\" onclick=\"outGroup(this); event.cancelBubble = true\"></i>";
                        }
                    }
                    groups += "<li role=\"presentation\" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href=\"#\" id=\"" + grs.Id + "\" onclick=\"toggleActiveGroup(this);" + toogleMode + "\">" + descr + hideGroup + "</a></li>";
                }
                Clients.Caller.sendGroups(groups);
            }
        }
    }


    /// <summary>
    /// Изменяет на противоположное занчение активность беседы
    /// </summary>
    /// <param name="_id">ID группы</param>
    public void toogleModeGroup(int _id, bool activ)
    {
        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(us=>us.token.Equals(Context.ConnectionId)).First();
            bool now = myUser.ug.Where(uug => uug.GroupsId == _id).FirstOrDefault().active;
            if (activ || (!activ && !now))
            {
                myUser.ug.Where(uug => uug.GroupsId == _id).FirstOrDefault().active = !now;
            }
            cc.SaveChanges();
            if (activ)
            {
                changeGroupMode(1);
            }
        }
    }

    /// <summary>
    /// Делает одну группу активной и возвращает её
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_active"></param>
    public void activateAndReturnONeGroup(int _id, bool _active)
    {
        UserGroups gr;
        string descr = "descr",
             grId = "-1";

        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(us => us.token.Equals(Context.ConnectionId)).First();
            gr = myUser.ug.Where(uug => uug.GroupsId == _id).FirstOrDefault();
            gr.active = _active;
            grId = gr.Group.Id.ToString();

            if (_active)
            {
                if (gr.Group.single)
                {

                    descr = gr.Group.ug.Select(us => us.User).ToList().Where(u => u.Id != myUser.Id).First().fio;
                }
                else
                {
                    descr = gr.Group.descript;
                }
                string itemGroup = "<li role=\"presentation\" onmouseover=\"showHide(this)\" onmouseout=\"hideHide(this)\"><a href=\"#\" id=\"" + grId + "\" onclick=\"toggleActiveGroup(this); changeModeGroup(this)\">" + descr + "<i aria-hidden=\"true\" class=\"fa fa-low-vision pull-right hide-group\" onclick=\"hideMe(this); event.cancelBubble = true\" title=\"Скрыть беседу\"></i></a></li>";
                Clients.Caller.sendOneItemGroup(itemGroup);
            }
            cc.SaveChanges();
        }
    }

    /// <summary>
    /// Выход из беседы
    /// </summary>
    /// <param name="_id">ID группы</param>
    public void leavGroup(int _id)
    {
        string myToken = Context.ConnectionId;
        string myFio = "";
        int grId = -1;

        using (ChatContext cc = new ChatContext())
        {
            myUser = cc.Users.Where(us => us.token.Equals(myToken)).First();
            if (myUser != null)
            {
                myFio = myUser.fio;
                UserGroups ug = myUser.ug.Where(uug => uug.GroupsId == _id).FirstOrDefault();
                if (!ug.Group.single)
                {
                    string grName = ug.Group.name;
                    grId = ug.Group.Id;

                    var grSingle = new List<object>();
                    grSingle.Add(new { group = ug.Group.single });
                    
                    Groups.Remove(myToken, grName);
                    Send("Я покидаю вашу беседу", grId);
                    myUser.ug.Remove(ug);
                    cc.SaveChanges();
                    Clients.Caller.outGroupOk(grId);

                    var UserInGroup = cc.Users.Where(us => us.ug.Select(gr => gr.Group).ToList().Any(gr => gr.Id == grId)).Select(use => new { fio = use.fio, token = use.token }).ToList();
                    Clients.Group(grName).outGroupOkChangeUserList(UserInGroup, grSingle, grId);

                    
                    //Clients.Group(grName).messageReceived(myFio, "Я покидаю вашу беседу", DateTime.Now.ToString("dd.MM.yy  HH:mm"), myToken, grId);
                }
            }
        }
    }


    /// <summary>
    /// Добавление пользователей в группу
    /// </summary>
    /// <param name="_tokens">Токены пользователей</param>
    /// <param name="_nameGr">Имя группы</param>
    private void addInGroup(Group _group)
    {
        foreach (User us in _group.ug.Select(u=>u.User).ToList())
        {
            Groups.Add(us.token, _group.name);
        }   
    }


    /// <summary>
    /// Генерирует рандомное имя из 20 символов
    /// </summary>
    /// <returns></returns>
    private string generateName()
    {
        string val = "";
        Random rnd = new Random();
        for (int i = 0; i < 20; i++)
        {
            switch(rnd.Next(3))
            {
                case 0:
                    val += (char)rnd.Next(97, 123);
                    break;
                case 1:
                    val += (char)rnd.Next(65, 91);
                    break;
                case 2:
                    val += (char)rnd.Next(48, 58);
                    break;
            }
            
        }

        return val;
    }

    /// <summary>
    /// Инициализация имеющихся групп
    /// </summary>
    private void initGroups()
    {
        List<Group> allGroup;
        using (ChatContext cc = new ChatContext())
        {
            //allGroup = cc.Groups.Include("Users").ToList();
            allGroup = cc.Groups.ToList();
        

            if (allGroup.Count > 0)
            {
                foreach (Group gr in allGroup)
                {
                    foreach (User us in gr.ug.Select(u=>u.User).ToList())
                    {
                        Groups.Add(us.token, gr.name);
                    }
                }
            }
        }
    }

    

}