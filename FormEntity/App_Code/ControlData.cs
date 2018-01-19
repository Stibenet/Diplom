using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Collections;

namespace FormEntity.Models
{
    public class ControlData
    {

        /// <summary>
        /// Добавление пользователя в локальную БД
        /// </summary>
        /// <param name="_login">Логин пользователя</param>
        public void addUser(string _login)
        {
            using (ChatContext db = new ChatContext())
            {
                Dictionary<string, string> infoAD = getInfoAD(_login);
                User newUser = new User();
                newUser.login = _login;
                newUser.fio = SplitFIO(infoAD["fio"], true, false);
                newUser.podrazdelenie = infoAD["podr"];
                newUser.mail = infoAD["mail"];
                newUser.phone = infoAD["phone"];
                newUser.online = true;
                db.Users.Add(newUser);
                db.SaveChanges();
            }
        }


        /// <summary>
        /// Обновление таблицы User
        /// </summary>
        /// <param name="_id">ID пользователя</param>
        /// <param name="_updateProp">Словарь с обновляемыми полями и значениями</param>
        public void updateUser(int _id, Dictionary<string, object> _updateProp)
        {
            using (ChatContext db = new ChatContext())
            {
                User curUser = db.Users.Where(u => u.Id == _id).First();
                foreach (string prop in _updateProp.Keys)
                {
                    curUser.GetType().GetProperty(prop).SetValue(curUser, _updateProp[prop]);   
                }                                                                          
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Авторизация через AD
        /// </summary>
        /// <param name="_login">Логин в домене ugrasu</param>
        /// <param name="_pass">Пароль</param>
        /// <returns></returns>
        public bool Authenticate(string _login, string _pass)
        {
            bool auth = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://ugrasu", _login, _pass);
                object nativeObj = entry.NativeObject;
                auth = true;
            }
            catch (DirectoryServicesCOMException) { }
            return auth;
        }



        /// <summary>
        /// Обновление статуса пользователя в сети (Online)
        /// </summary>
        /// <param name="_id">Ид пользователя</param>
        /// <param name="online">Новый статус</param>
        public void updateStatus(int _id, bool _online = true)
        {
            Dictionary<string, object> upVal = new Dictionary<string, object>();
            upVal.Add("online", _online);
            updateUser(_id, upVal);
        }

        
        /// <summary>
        /// Получчение информации из ActiveDirectory
        /// </summary>
        /// <param name="_login">login</param>
        /// <returns></returns>
        private Dictionary<string,string> getInfoAD(string _login)
        {
            Dictionary<string,string> data = new Dictionary<string,string>();
            DirectoryEntry entry = new DirectoryEntry("LDAP://ugrasu");
            DirectorySearcher searcher = new DirectorySearcher();
            searcher.SearchRoot = entry;
            searcher.Filter = "(sAMAccountName=" + _login + ")";
            SearchResult results = searcher.FindOne();
            DirectoryEntry ent = results.GetDirectoryEntry();

            string mail = "Не указан",
                   phone = "Не указан";
            if (ent != null)
            {
                data.Add("fio", ent.Name.Split('=')[1]);
                data.Add("podr", ent.Parent.Name.Split('=')[1]);
                if (ent.Properties["mail"].Value != null)
                {
                    mail = ent.Properties["mail"].Value.ToString();
                }
                data.Add("mail", mail);

                if (ent.Properties["telephoneNumber"].Value != null)
                {
                    phone = ent.Properties["telephoneNumber"].Value.ToString();
                
                }
                data.Add("phone", phone);
            }    
            return data;
        }

        /// <summary>
        /// Преобразование ФИО 
        /// </summary>
        /// <param name="FIO">ФИО</param>
        /// <param name="ChangeRegister">Сменить регистр в строчные</param>
        /// <param name="FullName">Оставить полное ИО</param>
        /// <param name="Reverce">Инициалы перед фамилией</param>
        /// <returns></returns>
        private string SplitFIO(String FIO, bool ChangeRegister, bool FullName, bool Reverce = false)
        {
            string[] ar = FIO.Split(' ');
            string newfio = "";
            if (ChangeRegister)
            {
                for (int i = 0; i < ar.Length; i++)
                {
                    newfio += ChangeFRegister(ar[i]) + " ";
                }
            }
            else
            {
                newfio = FIO;
            }

            if (!FullName)
            {
                ar = newfio.Split(' ');
                if (Reverce)
                {
                    newfio = ar[1].Substring(0, 1) + "." + ar[2].Substring(0, 1) + ". " + ar[0];
                }
                else
                {
                    newfio = ar[0] + " " + ar[1].Substring(0, 1) + "." + ar[2].Substring(0, 1) + ".";
                }
            }
            return newfio;
        }

        private string ChangeFRegister(String st)
        {
            return st.Substring(0, 1).ToUpper() + st.Substring(1, st.Length - 1).ToLower();
        }

    }
}