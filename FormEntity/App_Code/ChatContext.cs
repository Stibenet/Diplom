using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
               

namespace FormEntity.Models
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class ChatContext : DbContext
    {
        public ChatContext()
            : base("chat")
        {
            //Database.SetInitializer(new Initialize());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroups> UsGr { get; set; }
        public DbSet<Message> Messages { get; set; }


      /*  public DbSet<Test1> Test1 { get; set; }
        public DbSet<Test2> Test2 { get; set; }
        public DbSet<Test12> Test12 { get; set; }    */

        /* protected override void OnModelCreating(DbModelBuilder modelBuilder)
         {   
             modelBuilder.Entity<User>().HasMany(p=>p.ug)
                 .WithRequired(pc=>pc.User)
                 .HasForeignKey(pc=>pc.UserId);

             modelBuilder.Entity<Group>().HasMany(p => p.ug)
                 .WithRequired(pc => pc.Group)
                 .HasForeignKey(pc => pc.GroupsId);    

         }    */
    }



    public class Initialize : DropCreateDatabaseAlways<ChatContext>
    {
        protected override void Seed(ChatContext context)
        {
            /*
            User u1 = new User {login="sss1", fio="slobodenyuk", online=true };
            User u2 = new User { login = "ovg", fio = "Gon", online = false };
            context.Users.AddRange(new List<User> { u1, u2 });

            Group gr1 = new Group { name = "oib" };
            gr1.Users.Add(u1);
            gr1.Users.Add(u2);

            Group gr2 = new Group { name = "share" };
            gr2.Users.Add(u1);
            context.Groups.AddRange(new List<Group> { gr1, gr2 });
               */


            /*
            Test1 t11 = new Test1 { text = "sss" };

            Test2 t21 = new Test2 { text = "OIB" };
            Test2 t22 = new Test2 { text = "BASE" };

            Test12 t121 = new Test12 { Test1 = t11, Test2 = t21, active = false };
            Test12 t122 = new Test12 { Test1 = t11, Test2 = t22, active = true };

            context.Test12.Add(t121);
            context.Test12.Add(t122);
                                    */
            base.Seed(context);
        }   
    }

}