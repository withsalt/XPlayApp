using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace App.AspNetCore.Services
{
    public static class ConfigLoaderService
    {
        public static IApplicationBuilder UseConfig(this IApplicationBuilder app)
        {
            try
            {
                ConfigManager config = app.ApplicationServices.GetService<ConfigManager>();
                if (config == null)
                {
                    throw new Exception("Get config manager service failed.");
                }
                config.Load();
                return app;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
