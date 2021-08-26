using App.Data.Entity.Interface;
using App.Util.Date;
using SqlSugar;
using System.Collections.Generic;

namespace App.Data.Entity.User
{
    [SugarTable("User_Info")]
    public class UserInfo : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 32)]
        public string UserId { get; set; }

        [SugarColumn(Length = 50)]
        public string Name { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Email { get; set; }

        [SugarColumn(Length = 32, IsNullable = true)]
        public string Phone { get; set; }

        [SugarColumn(Length = 4, IsNullable = true)]
        public string Sex { get; set; }

        [SugarColumn(Length = 13, IsNullable = true)]
        public string QQ { get; set; }

        [SugarColumn(Length = 60, IsNullable = true)]
        public string WechatName { get; set; }

        /// <summary>
        /// 生日 时间戳
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? Brithday { get; set; }

        [SugarColumn(Length = 255, IsNullable = true)]
        public string Address { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Logo { get; set; }

        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; set; }

        public long UpdateTime { get; set; }

        public long CreateTime { get; set; } = TimeUtil.Timestamp();

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }

        [SugarColumn(IsIgnore = true)]
        public UserPasswd UserPasswd { get; set; }
    }
}
