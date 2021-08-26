using App.Data.Entity.Interface;
using App.Data.Enum;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Entity.System
{
    [SugarTable("Sys_Material")]
    public class Material : IEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 50)]
        public string FileName { get; set; }

        [SugarColumn(Length = 300)]
        public string FileOldName { get; set; }

        [SugarColumn(Length = 300)]
        public string Path { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string LogoPath { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? Duration { get; set; } = 0;

        [SugarColumn(Length = 30, IsNullable = true)]
        public string Extension { get; set; }

        public FileType FileType { get; set; }

        public int Height { get; set; } = 0;

        public int Width { get; set; } = 0;

        public float Size { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Remark { get; set; }

        public long CreateTime { get; set; }
    }
}
