using App.Config;
using App.Data.Entity.Interface;
using App.Data.Entity.System;
using App.Data.Entity.User;
using App.Util.Date;
using App.Util.Json;
using App.Util.Security;
using App.Util.User;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace App.Repository.Util
{
    public class DbSeed
    {
        public async Task<bool> Seed(CustumDbContext db)
        {
            try
            {
                Type[] types = Assembly.LoadFile($"{AppContext.BaseDirectory}App.Data.Entity.dll").GetTypes();
                Type entityType = typeof(IEntity);

                List<Type> allEntities = new List<Type>();
                List<Type> mapEntities = new List<Type>();

                foreach (var item in types)
                {
                    if (item.GetInterfaces().Where(p => p.Name == entityType.Name).Count() > 0)
                    {
                        allEntities.Add(item);
                    }
                }
                //创建数据库
                db.Context.DbMaintenance.CreateDatabase();

                try
                {
                    List<DbTableInfo> tables = db.Context.DbMaintenance.GetTableInfoList();
                    foreach (var item in allEntities)
                    {
                        IEnumerable<CustomAttributeData> attr = item.CustomAttributes;
                        if (attr.Count() <= 0)
                        {
                            mapEntities.Add(item);
                            continue;
                        }
                        CustomAttributeData attrData = attr.Where(p => p.AttributeType == typeof(SugarTable)).SingleOrDefault();
                        if (attrData == null)
                        {
                            mapEntities.Add(item);
                            continue;
                        }
                        List<CustomAttributeTypedArgument> args = attrData.ConstructorArguments.ToList();
                        if (args == null || args.Count <= 0)
                            continue;
                        if (tables.Where(t => args.Where(arg => arg.ArgumentType == typeof(string) && arg.Value.ToString().Equals(t.Name, StringComparison.OrdinalIgnoreCase)).Count() > 0).Count() <= 0)
                            mapEntities.Add(item);
                    }
                }
                catch { }
                if (mapEntities.Count > 0 && mapEntities.Count == allEntities.Count)
                {
                    //创建表，并初始化数据
                    db.Context.CodeFirst.InitTables(mapEntities.ToArray());

                    List<Role> roles = CreateDefaultRoles();
                    await db.Role.Add(roles);
                    //user
                    List<UserInfo> users = CreateDefaultUserInfo(roles);
                    await db.UserInfo.Add(users);
                    foreach (var item in users)
                    {
                        await db.UserPasswd.Add(item.UserPasswd);
                    }
                    //await db.SystemLog.Add(CreateSystemLog());
                    await db.ConfigInfo.Add(CreateDefaultConfigInfo());
                }
                else if (mapEntities.Count > 0)
                {
                    //创建表
                    db.Context.CodeFirst.InitTables(mapEntities.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        private List<ConfigInfo> CreateDefaultConfigInfo()
        {
            List<ConfigInfo> result = new List<ConfigInfo>()
            {
                new ConfigInfo()
                {
                    Key = "tests",
                    Content = "{}",
                    IsCache = true,
                    CreateUser = "admin",
                    CreateTime = TimeUtil.Timestamp(),
                },
            };
            return result;
        }

        private List<SystemLog> CreateSystemLog()
        {
            return new List<SystemLog>()
            {
                new SystemLog()
                {
                    LogType = "system",
                    Describe = "firest inti db.",
                    Stack = "",
                    Localtion = "CustumDbDataInitializer",
                    CreateTime = TimeUtil.Timestamp()
                }
            };
        }

        private List<Role> CreateDefaultRoles()
        {
            List<Role> result = new List<Role>()
            {
                new Role()
                {
                    Name = "管理员",
                    Describe = "管理员",
                    CreateTime = TimeUtil.Timestamp(),
                    IsActive = true,
                    IsDelete = false
                }
            };
            return result;
        }

        private List<UserInfo> CreateDefaultUserInfo(List<Role> roles)
        {
            Role adminsRole = roles.Where(r => r.Name == "管理员").SingleOrDefault();
            if (adminsRole == null)
            {
                throw new Exception("Create default admin info failed. admin role cannot find.");
            }
            List<UserInfo> result = new List<UserInfo>()
            {
                new UserInfo()
                {
                    UserId = "admin",
                    Name = "Admin",
                    Email = "admin@geeiot.net",
                    Phone = "09812800010",
                    IsAdmin = false,
                    CreateTime = TimeUtil.Timestamp(),
                    UpdateTime = TimeUtil.Timestamp(),
                    IsActive = true,
                    IsDelete = false,
                    UserPasswd = new UserPasswd
                    {
                        UserId = "admin",
                        Password = UserUtil.PasswdAddSalt(Encrypt.MDString("123"),ConfigManager.Instance.AppSettings.PasswdSalt),
                        Token = Encrypt.MDString(Guid.NewGuid().ToString()),
                        CreateTime = TimeUtil.Timestamp(),
                        UpdateTime = TimeUtil.Timestamp(),
                    }
                }
            };
            return result;
        }
    }
}
