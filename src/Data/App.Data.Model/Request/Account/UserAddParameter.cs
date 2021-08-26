using System;
using System.Collections.Generic;
using System.Text;

namespace App.Data.Model.Request.Account
{
    public class UserAddParameter
    {
        public string Name { get; set; }

        public string OpenId { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Passwd { get; set; }

        public int CreateTime { get; set; }

        public string Token { get; set; }

        public string Path { get; set; }
    }
}
