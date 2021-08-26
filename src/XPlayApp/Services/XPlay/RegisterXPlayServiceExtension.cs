using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPlayApp.Services.XPlay
{
    public static class RegisterXPlayServiceExtension
    {
        public static IServiceCollection RegisterXPlayService(this IServiceCollection services)
        {
            services.AddSingleton<XPlayService>();
            return services;
        }

        public static IApplicationBuilder UseXPlay(this IApplicationBuilder app)
        {
            XPlayService xplay = app.ApplicationServices.GetService<XPlayService>();
            if (xplay == null)
            {
                throw new Exception("Get ffmpeg service failed.");
            }

            xplay.Connect().GetAwaiter().GetResult();
            return app;
        }
    }
}
