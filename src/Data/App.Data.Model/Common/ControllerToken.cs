using System;
using System.Collections.Generic;
using System.Text;

namespace App.Data.Model.Common
{
    public class ControllerToken
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        public string Source { get; set; }

        public long UpdateTime { get; set; }
    }
}
