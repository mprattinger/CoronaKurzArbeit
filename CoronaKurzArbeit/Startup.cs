using Blazored.Modal;
using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoronaKurzArbeit
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            var kaSettings = new KurzarbeitSettingsConfiguration();
            Configuration.Bind("KurzarbeitSettings", kaSettings);
            services.AddSingleton(kaSettings);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddBlazoredModal();

            services.AddScoped<IAppState, AppState>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IFeiertagService, FeiertagService>();
            services.AddScoped<ICoronaService, CoronaService>();
            services.AddScoped<ITimeBookingsService, TimeBookingsService>();
            services.AddScoped<IActualWorkTimeService, ActualWorkTimeService>();
            services.AddScoped<ITargetWorkTimeService, TargetWorkTimeService>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
