using App.Data.Entity.Interface;
using SqlSugar;

namespace App.Data.Entity.System
{
    [SugarTable("Sys_Logs")]
    public class SystemLog : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 32)]
        public string LogType { get; set; }

        [SugarColumn(Length = 150, IsNullable = true)]
        public string Describe { get; set; }

        [SugarColumn(Length = 150, IsNullable = true)]
        public string Localtion { get; set; }

        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string Stack { get; set; }

        public long CreateTime { get; set; }
    }
}
