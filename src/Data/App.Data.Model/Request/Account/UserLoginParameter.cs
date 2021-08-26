using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Model.Request.Account
{
    public class UserLoginParameter
    {
        public UserLoginParameter()
        {
            this.Time = 0;
            this.IsRemenber = false;
        }

        public string UserId { get; set; }

        public string Passwd { get; set; }

        public string Code { get; set; }

        public int Time { get; set; }

        public bool IsRemenber { get; set; } = true;
    }
}
