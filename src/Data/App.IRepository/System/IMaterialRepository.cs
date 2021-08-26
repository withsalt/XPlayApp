using App.IRepository.Interface;
using App.Data.Entity.System;
using App.Data.Model.Request.Upload;
using System.Threading.Tasks;

namespace App.IRepository.System
{
    public interface IMaterialRepository : IBaseRepository<Material>
    {
        Task<bool> SaveFileInfo(FileUploadParam uploadParam);
    }
}
