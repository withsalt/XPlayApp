using App.IRepository.Interface;
using App.Repository.Interface;
using Microsoft.Extensions.Logging;

using App.IRepository.System;
using App.Data.Entity.System;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using App.Util.Date;
using App.Repository.Util;
using Microsoft.Extensions.Caching.Memory;

namespace App.Repository.System
{
    public class ConfigInfoRepository : BaseRepository<ConfigInfo>, IConfigInfoRepository
    {
        private readonly ILogger<ConfigInfoRepository> _logger;
        private readonly IMemoryCache _cache;

        public ConfigInfoRepository(ILoggerFactory logger
            , IBaseDbContext context
            , IMemoryCache redis) : base(logger, context)
        {
            _logger = logger.CreateLogger<ConfigInfoRepository>();
            _cache = redis ?? throw new ArgumentNullException(nameof(IMemoryCache));
        }

        public new async Task<ConfigInfo> Query(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }
                ConfigInfo cache = _cache.Get<ConfigInfo>(key);
                if (cache != null)
                {
                    return cache;
                }
                List<ConfigInfo> infos = await Query(c => c.Key == key);
                if (infos != null && infos.Count > 0)
                {
                    var item = infos[0];
                    if (item != null)
                    {
                        _cache.Set(key, item, TimeSpan.FromDays(7));
                        return item;
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Query config by key '{key}' failed, {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Update(string key, ConfigInfo info)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }
                List<ConfigInfo> infos = await Query(c => c.Key == key);
                if (infos == null || infos.Count == 0)
                {
                    return false;
                }
                ConfigInfo updateEntity = infos[0];
                updateEntity.Content = info.Content;
                updateEntity.UpdateUser = info.UpdateUser;
                updateEntity.UpdateTime = TimeUtil.Timestamp();

                int count = await db.Updateable(updateEntity)
                    .UpdateColumns(u => new { u.Content, u.UpdateUser, u.UpdateTime })
                    .ExecuteCommandAsync();
                if (count > 0)
                {
                    if (info.IsCache)
                    {
                        _cache.Set(key, info, TimeSpan.FromDays(7));
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update config by key '{key}' failed, {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Add(string key, ConfigInfo info)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (string.IsNullOrEmpty(info.Content))
                {
                    throw new ArgumentNullException(nameof(info.Content));
                }
                if (string.IsNullOrEmpty(info.CreateUser))
                {
                    throw new ArgumentNullException(nameof(info.CreateUser));
                }
                List<ConfigInfo> infos = await Query(c => c.Key == key);
                if (infos == null || infos.Count > 0)
                {
                    return await Update(key, info);
                }
                info.CreateTime = TimeUtil.Timestamp();
                //save to db
                ConfigInfo saveResult = await db.Insertable(info)
                    .ExecuteReturnEntityAsync();
                if (saveResult != null)
                {
                    //save to cache
                    if (info.IsCache)
                    {
                        _cache.Set(key, info, TimeSpan.FromDays(7));
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update config by key '{key}' failed, {ex.Message}");
                return false;
            }
        }
    }
}
