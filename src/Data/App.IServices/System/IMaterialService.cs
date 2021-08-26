using App.IServices.Interface;
using App.Data.Entity.System;
using App.Data.Model.Request.Upload;
using System.Threading.Tasks;

namespace App.IServices.System
{
    public interface IMaterialService : IBaseServices<Material>
    {
        Task<bool> SaveFileInfo(FileUploadParam uploadParam);
    }
}
