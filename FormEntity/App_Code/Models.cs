using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormEntity.Models
{
    public class User
    {
        public int Id { get; set; }
        public string login { get; set; }
        public string fio { get; set; }
        public string podrazdelenie { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public bool online { get; set; }
        public string token { get; set; }

       // public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<UserGroups> ug { get; set; }
       /* public User()
        {
            Groups = new List<Group>();
        } */
    }

    public class Group
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string descript { get; set; }
                                        
        
        public bool single { get; set; }

        //public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Message> msg { get; set; }
        public virtual ICollection<UserGroups> ug { get; set; }
        public Group()
        {
            //Users = new List<User>();
            msg = new List<Message>();
        }


    }
   
    public class UserGroups
    {
        [Key, Column(Order=0)]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Key, Column(Order=1)]
        [ForeignKey("Group")]
        public int GroupsId { get; set; }

        public virtual User User { get; set; }
        public virtual Group Group { get; set; }

        public bool active { get; set; }
        public UserGroups()
        {
            active = true;
        }
    }       

    public class Message
    {
        public int Id { get; set; }
        public User fromUser { get; set; }

        public int GroupId { get; set; }
        public Group toGroup { get; set; }

        public string text { get; set; }
        public DateTime ftime { get; set; }

    }



  /*
    public class Test1
    {
        public int Id { get; set; }
        public string text { get; set; }
        public virtual ICollection<Test12> Test12 { get; set; }
    }


    public class Test2
    {
        public int Id { get; set; }
        public string text { get; set; }
        public virtual ICollection<Test12> Test12 { get; set; }
    }

    public class Test12
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Test1")]
        public int Test1Id { get; set; }
        [Key, Column(Order = 1)]
        [ForeignKey("Test2")]
        public int Test2Id { get; set; }

        public virtual Test1 Test1 { get; set; }
        public virtual Test2 Test2 { get; set; }

        public bool active { get; set; }
    }      */
}
