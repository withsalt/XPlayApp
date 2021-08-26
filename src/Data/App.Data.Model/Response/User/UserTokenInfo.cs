using System;
using System.Collections.Generic;
using System.Text;

namespace App.Data.Model.Response.User
{
    public class UserTokenInfo
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        public long Time { get; set; }

        public string Describe
        {
            get
            {
                return "USER LOGIN TOKEN CACHE";
            }
        }
    }
}
