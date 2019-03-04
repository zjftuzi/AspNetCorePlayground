﻿using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestWebApplication.Conventions;
using WeihanLi.AspNetCore.Authentication;
using WeihanLi.AspNetCore.Authentication.HeaderAuthentication;
using WeihanLi.Configuration.EntityFramework;
using WeihanLi.Configuration.Redis;
using WeihanLi.Redis;

namespace TestWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(HeaderAuthenticationDefaults.AuthenticationSchema)
                .AddHeader(HeaderAuthenticationDefaults.AuthenticationSchema, options => { options.AdditionalHeaderToClaims.Add("UserEmail", ClaimTypes.Email); })
                //.AddQuery(QueryAuthenticationDefaults.AuthenticationSchema, options => { options.AdditionalQueryToClaims.Add("UserEmail", ClaimTypes.Email); })
                ;

            services.AddMvc(options =>
                {
                    options.Conventions.Add(new ApiControllerVersionConvention());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersion.Default;
            });

            services.AddDbContext<ConfigurationsDbContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("Configurations"));
                });

            services.AddRedisConfig(config =>
            {
                config.EnableCompress = false;
                config.CachePrefix = "AspNetCorePlayground";
                config.DefaultDatabase = 2;
            });
            var configuration = new ConfigurationBuilder()
                .AddRedis(action =>
                {
                    action.Services = services;
                    action.RedisConfigurationKey = "Configurations";
                })
                .AddEntityFramework(config =>
                {
                    config.Services = services;
                })
                .Build();

            var rootUser = configuration["RootUser"];
            var conn = configuration.GetConnectionString("Abcd");

            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
