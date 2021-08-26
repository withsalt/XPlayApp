using System;
using App.Config;
using App.IRepository.Interface;
using App.Repository;
using App.Repository.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.AspNetCore.Services
{
    public static class DbSeedService
    {
        public static IApplicationBuilder UseDbSeed(this IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    ILoggerFactory loggerFac = (ILoggerFactory)app.ApplicationServices.GetRequiredService(typeof(ILoggerFactory));
                    var logger = loggerFac.CreateLogger(nameof(DbSeedService));

                    ConfigManager config = (ConfigManager)app.ApplicationServices.GetRequiredService(typeof(ConfigManager));
                    if (config == null)
                    {
                        throw new Exception("Get config failed.");
                    }
                    if (!config.AppSettings.IsDebug || !config.AppSettings.IsSeedDatabase)
                    {
                        logger.LogInformation("Skip database seed. If you wangt to seed db, please use debug mode and set 'IsSeedDatabase' as true");
                        return app;
                    }
                    CustumDbContext db = (CustumDbContext)serviceScope.ServiceProvider.GetService<IBaseDbContext>();
                    DbSeed seed = new DbSeed();
                    bool seedResult = seed.Seed(db)
                        .GetAwaiter()
                        .GetResult();
                    if (!seedResult)
                    {
                        throw new Exception("Seed db failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Seed db failed. {ex.Message}", ex);
            }
            return app;
        }
    }
}
