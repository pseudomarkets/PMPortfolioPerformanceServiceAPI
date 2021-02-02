using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PMMarketDataService.DataProvider.Client.Implementation;
using PMPortfolioPerformanceServiceAPI.Models;

namespace PMPortfolioPerformanceServiceAPI
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
            // Inject database context
            services.AddDbContext<PseudoMarketsDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));
            services.AddSingleton<MongoClient>(new MongoClient(Configuration.GetConnectionString("MongoDb")));

            // Inject Market Data Service Provider
            services.AddSingleton<MarketDataServiceClient>(new MarketDataServiceClient(new HttpClient(),
                Configuration.GetValue<string>("ServiceConfig:InternalServiceUsername"),
                Configuration.GetValue<string>("ServiceConfig:InternalServicePassword"),
                Configuration.GetValue<string>("ServiceConfig:MarketDataServiceUrl")));

            // Add basic auth
            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            // Configure all other services
            services.Configure<ServiceConfig>(Configuration.GetSection("ServiceConfig"));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Pseudo Markets Portfolio Performance API",
                    Description = "Account and position level performance reporting service",
                    Contact = new OpenApiContact
                    {
                        Name = "Shravan Jambukesan",
                        Email = "shravan@shravanj.com",
                        Url = new Uri("https://shravanj.com"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/pseudomarkets/PMPortfolioPerformanceServiceAPI/blob/master/LICENSE.txt"),
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pseudo Markets Portfolio Performance API");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
