using App.IRepository.Interface;
using App.IRepository.System;
using App.IRepository.User;
using App.IServices.System;
using App.IServices.User;
using App.Repository;
using App.Repository.System;
using App.Repository.User;
using App.Services.System;
using App.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace App.AspNetCore.Extensions
{
    public static class RegisterDbServiceExtension
    {
        public static IServiceCollection RegisterDbService(this IServiceCollection services)
        {
            #region DbContext

            services.AddScoped<IBaseDbContext, CustumDbContext>();

            #endregion

            #region Entity

            services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
            services.AddScoped<ILoginHistoryService, LoginHistoryService>();

            services.AddScoped<IUserInfoRepository, UserInfoRepository>();
            services.AddScoped<IUserInfoService, UserInfoService>();

            services.AddScoped<IUserPasswdRepository, UserPasswdRepository>();
            services.AddScoped<IUserPasswdService, UserPasswdService>();

            services.AddScoped<IUserRolesRepository, UserRolesRepository>();
            services.AddScoped<IUserRolesService, UserRolesService>();

            services.AddScoped<IUserValidateLogRepository, UserValidateLogRepository>();
            services.AddScoped<IUserValidateLogService, UserValidateLogService>();

            services.AddScoped<IMenuPermissionRepository, MenuPermissionRepository>();
            services.AddScoped<IMenuPermissionService, MenuPermissionService>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddScoped<ISystemLogRepository, SystemLogRepository>();
            services.AddScoped<ISystemLogService, SystemLogService>();

            services.AddScoped<IConfigInfoRepository, ConfigInfoRepository>();
            services.AddScoped<IConfigInfoService, ConfigInfoService>();

            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<IMaterialService, MaterialService>();

            #endregion

            return services;
        }
    }
}
