using App.Data.Model.Common.JsonObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Data.Model.Response.User
{
    public class UserInfoModel : IChild
    {
        public string Uid { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Sex { get; set; }

        public string QQ { get; set; }

        public string Wechat { get; set; }

        public int? Age { get; set; }

        public string Address { get; set; }

        public string Logo { get; set; }

        public int CreateTime { get; set; }

        public int UpdateTime { get; set; }

        public bool IsVip { get; set; }

        public bool IsDeveloper { get; set; }

        public bool IsAdmin { get; set; }

        public string Key { get; set; }

        public string Token { get; set; }

        public string Passwd { get; set; }

        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public bool IsLogin { get; set; } = false;

        public bool IsDelete { get; set; }

        public bool IsActive { get; set; }
    }
}
