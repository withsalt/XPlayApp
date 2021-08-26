using App.AutoMapper;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace App.AspNetCore.Extensions
{
    public static class AutoMapperExtension
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserInfoMapper());
            });
            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            return services;
        }
    }
}
