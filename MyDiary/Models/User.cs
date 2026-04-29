using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Models
{
    internal class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }  // stores bcrypt hash
    }
}
