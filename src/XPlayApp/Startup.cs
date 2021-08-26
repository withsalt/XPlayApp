using App.AspNetCore.Extensions;
using App.AspNetCore.Services;
using App.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using OnceMi.AspNetCore.FFmpeg;
using System;
using System.Runtime.InteropServices;
using XPlayApp.Services.XPlay;

namespace XPlayApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region ≈‰÷√º”‘ÿ

            services.AddSingleton<ConfigManager>();

            #endregion

            services.AddMemoryCache();

            #region RedisSession
            // π”√Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
            });
            #endregion

            #region Platform IO Config 
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);
            }
            else
            {
                services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
            }
            #endregion

            #region RepositoryAndService

            services.RegisterDbService();

            #endregion

            #region AutoMapper

            services.AddMapper();

            #endregion

            //ffmpeg
            services.RegisterFFmpegService();
            //Xplay
            services.RegisterXPlayService();

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();

            app.UseConfig();
            app.UseDbSeed();   //Seed DB
            app.UseFFmpeg();   //ffmpeg
            app.UseXPlay();    //xplay

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Video}/{action=Index}/{id?}");
            });
        }
    }
}
