using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using PublicShareOwnerControl.Authorization;
using PublicShareOwnerControl.DB;
using PublicShareOwnerControl.HostedServices;
using Swashbuckle.AspNetCore.Swagger;

namespace PublicShareOwnerControl
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
            //services.AddMvcCore().AddAuthorization().AddJsonFormatters();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            SetupDatabase(services);

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = Configuration["IdentityServerBaseAddress"];
            //        options.Audience = "BankingService";
            //    });

            //services.AddAuthorization(options =>
            //{
            //    //options.AddPolicy("BankingService.UserActions", policy =>
            //    //    policy.Requirements.Add(new HasScopeRequirement("BankingService.UserActions", Configuration["IdentityServerBaseAddress"])));
            //    //options.AddPolicy("BankingService.broker&taxer", policy =>
            //    //    policy.Requirements.Add(new HasScopeRequirement("BankingService.broker&taxer", Configuration["IdentityServerBaseAddress"])));
            //});

            services.AddHostedService<RequestStatsService>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            services.AddHealthChecks().AddDbContextCheck<PublicShareOwnerContext>(tags: new[] { "ready" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, PublicShareOwnerContext context)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                context.Database.Migrate();
            }

            SetupReadyAndLiveHealthChecks(app);

            app.UseMetricServer();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void SetupDatabase(IServiceCollection services)
        {
            services.AddDbContext<PublicShareOwnerContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("PublicShareOwnerDatabase")));
        }
        private static void SetupReadyAndLiveHealthChecks(IApplicationBuilder app)
        {
            // The readiness check uses all registered checks with the 'ready' tag.
            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                // Exclude all checks and return a 200-Ok.
                Predicate = (_) => false
            });
        }
    }
}
