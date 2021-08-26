using System;
using System.Collections.Generic;
using System.Text;

namespace App.Data.Model.Request.Password
{
    public class ResetPasswordParams
    {
        public string UserId { get; set; }

        public string OldPassword { get; set; }

        public string Password { get; set; }

        public string RePassword { get; set; }
    }
}
