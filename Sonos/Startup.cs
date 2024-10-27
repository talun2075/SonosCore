using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Sonos.Classes;
using SonosSQLiteWrapper;
using SonosSQLiteWrapper.Interfaces;
using HomeLogging;
using Sonos.Classes.Interfaces;
using SonosUPnP;
using SonosUPNPCore;
using SonosUPNPCore.Interfaces;
using System.Diagnostics;

namespace Sonos
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
            services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.IncludeFields = true); 
            services
                .AddTransient<ISonosPlayer, SonosPlayer>()
                .AddSingleton<ILogging, Logging>()
                .AddSingleton<ISonosHelper, SonosHelper>()
                .AddSingleton<IStreamDeckResponse, StreamDeckResponse>()
                .AddSingleton<IMusicPictures, MusicPictures>()
                .AddSingleton<ISonosDiscovery, SonosDiscovery>()
                .AddSingleton<ISonosPlayerPrepare, SonosPlayerPrepare>()
                .AddSingleton<ISQLiteWrapper, SQLiteWrapper>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseDefaultFiles();//=> für index.html Dateien; muss vor UseStaticFiles stehen
            var stfo = new StaticFileOptions();
            if (Debugger.IsAttached)
            {
                app.UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(Configuration["MusicHashFolder"]),
                    RequestPath = new PathString("/hashimages"),
                    EnableDirectoryBrowsing = true
                });
            }

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
