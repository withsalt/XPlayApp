using App.Data.Entity.Interface;
using SqlSugar;

namespace App.Data.Entity.System
{
    [SugarTable("Sys_Role")]
    public class Role : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 50)]
        public string Name { get; set; }

        [SugarColumn(Length = 100,IsNullable = true)]
        public string Describe { get; set; }

        public long  CreateTime { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
