using HouseTag_Web.IService;
using HouseTag_Web.Models;
using HouseTag_Web.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using System.Text;

namespace Tag_Web
{
    public class Startup
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<AppSettings>(Configuration);
            services.AddMemoryCache();

            services.AddSingleton<IMainService, MainService>();
            services.AddSingleton<IHouseAjkService, HouseAjkService>();
            services.AddSingleton<IHouseFtxService, HouseFtxService>();
            services.AddSingleton<IProxyService, ProxyService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            app.UseExceptionHandler(x =>
            {
                //将异常消息以json方式返回
                x.Run(async context =>
                {
                    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                    //记录错误日志
                    _logger.Error(ex);
                    var msg = JsonConvert.SerializeObject(new { status = 0, data = "", error = ex.Message });
                    context.Response.ContentType = "application/json;charset=utf-8";
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(msg);
                });
            });
            //ip
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto });
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
