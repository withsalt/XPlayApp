using App.Data.Entity.Interface;
using SqlSugar;

namespace App.Data.Entity.System
{
    [SugarTable("Sys_MenuPermission")]
    public class MenuPermission : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int RoleId { get; set; }

        public int MenuId { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Remark { get; set; }

        public long CreateTime { get; set; }
    }
}
