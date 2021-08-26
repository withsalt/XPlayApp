using App.Data.Entity.Interface;
using SqlSugar;

namespace App.Data.Entity.User
{
    [SugarTable("User_LoginHistory")]
    public class LoginHistory : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 32)]
        public string Userid { get; set; }

        public int TypeId { get; set; }

        [SugarColumn(Length = 32, IsNullable = true)]
        public string LoginId { get; set; }

        public long LoginTime { get; set; }
    }
}
