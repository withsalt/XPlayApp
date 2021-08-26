using App.Data.Entity.Interface;
using App.Util.Date;
using SqlSugar;

namespace App.Data.Entity.System
{
    [SugarTable("Sys_ConfigInfo")]
    public class ConfigInfo : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 66)]
        public string Key { get; set; }

        [SugarColumn(ColumnDataType = "text")]
        public string Content { get; set; } = "{}";

        /// <summary>
        /// 是否保存在缓存当中
        /// </summary>
        public bool IsCache { get; set; }

        [SugarColumn(Length = 32)]
        public string CreateUser { get; set; }

        public long CreateTime { get; set; } = TimeUtil.Timestamp();

        [SugarColumn(Length = 32, IsNullable = true)]
        public string UpdateUser { get; set; }

        [SugarColumn(IsNullable = true)]
        public long? UpdateTime { get; set; }
    }
}
