using App.IRepository.Interface;
using App.Data.Entity.System;
using System.Threading.Tasks;

namespace App.IRepository.System
{
    public interface IConfigInfoRepository : IBaseRepository<ConfigInfo>
    {
        new Task<ConfigInfo> Query(string key);

        Task<bool> Update(string key, ConfigInfo info);

        Task<bool> Add(string key, ConfigInfo info);
    }
}
