using App.Data.Entity.Interface;
using App.Util.Date;
using SqlSugar;

namespace App.Data.Entity.User
{
    [SugarTable("User_Roles")]
    public class UserRoles : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 32)]
        public string UserId { get; set; }

        public int RoleId { get; set; }

        public long CreateTime { get; set; } = TimeUtil.Timestamp();

    }
}
