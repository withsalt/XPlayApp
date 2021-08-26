using App.Data.Entity.Interface;
using App.Util.Date;
using SqlSugar;

namespace App.Data.Entity.User
{
    /// <summary>
    /// 记录用户当天密码输入错误次数
    /// </summary>
    [SugarTable("User_ValidateLog")]
    public class UserValidateLog : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 当天记录时间，Ex：20180910
        /// </summary>
        [SugarColumn(Length = 8)]
        public string ShortTime { get; set; }

        public long TryTime { get; set; } = TimeUtil.Timestamp();

        [SugarColumn(Length = 32)]
        public string TryUserId { get; set; }

        public long CreateTime { get; set; } = TimeUtil.Timestamp();
    }
}
