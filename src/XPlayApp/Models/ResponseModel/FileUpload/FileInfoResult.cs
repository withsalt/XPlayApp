using App.Data.Enum;
using App.Data.Model.Common.JsonObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPlayApp.Models.ResponseModel.FileUpload
{
    public class FileInfoResult: IChild
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string FileOldName { get; set; }

        public string Path { get; set; }

        public string LogoPath { get; set; }

        public int? Duration { get; set; } = 0;

        public string Extension { get; set; }

        public FileType FileType { get; set; }

        public float Size { get; set; }

        public string Remark { get; set; }

        public long CreateTime { get; set; }

        public string ContentType { get; set; }
    }
}
