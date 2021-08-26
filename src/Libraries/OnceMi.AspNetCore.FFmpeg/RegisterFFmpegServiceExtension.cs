using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OnceMi.AspNetCore.FFmpeg
{
    public static class RegisterFFmpegServiceExtension
    {
        public static IServiceCollection RegisterFFmpegService(this IServiceCollection services)
        {
            services.AddScoped<FFmpegService>();
            return services;
        }

        public static IApplicationBuilder UseFFmpeg(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                FFmpegService ffmpeg = scope.ServiceProvider.GetRequiredService<FFmpegService>();
                if (ffmpeg == null)
                {
                    throw new Exception("Get ffmpeg service failed.");
                }

                if (!ffmpeg.Init())
                {
                    throw new Exception("Init ffmpeg service failed.");
                }
            }
            return app;
        }
    }
}
