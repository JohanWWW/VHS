using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VHS.Backend.Apis;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.HostedServices;
using VHS.Backend.HostedServices.Interfaces;
using VHS.Backend.Repositories;
using VHS.Backend.Repositories.Interfaces;
using VHS.VehicleTest;

namespace VHS.Backend
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VHS.Backend", Version = "v1" });
            });

            // Singletons
            var vehicleSimImplementation = new VehicleSimulatorBackgroundService();
            _ = vehicleSimImplementation.StartAsync();

            services.AddSingleton<IAuthorizationClientApi, AuthorizationApi>();
            services.AddSingleton<IUserAccountClientApi, UserAccountApi>();
            services.AddSingleton<IVehicleClientApi, FakeVehicleHookApi>();

            services.AddSingleton<IVehicle, CloudCar>();
            services.AddSingleton<IDriveLogRepository, FakeDriveLogDB>();

            services.AddSingleton<IVehicleSimulatorBackgroundService>(factory => vehicleSimImplementation);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VHS.Backend v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
