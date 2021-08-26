using App.Config;
using App.Data.Entity.Interface;
using App.IRepository.Interface;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Repository.Interface
{
    public class BaseDbContext : IBaseDbContext
    {
        private readonly ILogger<BaseDbContext> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConfigManager _config;

        public SqlSugarClient Context { get; private set; }

        public BaseDbContext(ILoggerFactory logger
            , ConfigManager config)
        {
            _logger = logger == null ? throw new ArgumentNullException(nameof(ILoggerFactory)) : logger.CreateLogger<BaseDbContext>();
            _loggerFactory = logger;
            _config = config ?? throw new ArgumentNullException(nameof(ConfigManager));

            Context = CreateDbClient();
        }

        private SqlSugarClient CreateDbClient()
        {
            SqlSugarClient _dbBase = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = _config.ConnectionStrings.SqliteDbConnectionString,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            if (_config.AppSettings.IsDebug)
            {
                _dbBase.Aop.OnLogExecuting = (sql, pars) => //SQL执行中事件
                {
                    Parallel.For(0, 1, e =>
                    {
                        _logger.LogInformation($"【SqlLog】\n【查询】：\n{sql}\n{GetParas(pars)}");
                    });
                };
            }
            return _dbBase;
        }

        private string GetParas(SugarParameter[] pars)
        {
            if (pars.Length == 0)
                return null;
            StringBuilder sb = new StringBuilder();
            sb.Append("【参数】：\n");
            foreach (var param in pars)
            {
                sb.Append($"{param.ParameterName}:{param.Value}\n");
            }
            return sb.ToString();
        }

        public virtual IBaseRepository<T> GetRepository<T>() where T : class, IEntity, new()
        {
            try
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                Type entityType = typeof(T);

                foreach (var item in types)
                {
                    if (item.Name.EndsWith("Repository") && item.Name.Replace("Repository", "") == entityType.Name)
                    {
                        return (IBaseRepository<T>)Activator.CreateInstance(item, new object[] { _loggerFactory, this });
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Get {nameof(T)} repository failed. {ex.Message}", ex);
                return null;
            }
        }
    }
}
