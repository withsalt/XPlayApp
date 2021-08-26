using App.Data.Entity.Interface;
using App.Util.Date;
using SqlSugar;

namespace App.Data.Entity.User
{
    [SugarTable("User_Password")]
    public class UserPasswd : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 32)]
        public string UserId { get; set; }

        [SugarColumn(Length = 64)]
        public string Password { get; set; }

        [SugarColumn(Length = 64)]
        public string Token { get; set; }

        public long CreateTime { get; set; } = TimeUtil.Timestamp();

        public long UpdateTime { get; set; }
    }
}
