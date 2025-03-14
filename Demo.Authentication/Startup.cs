using Demo.Authentication.Authentication;
using Demo.Authentication.Authorization;
using Demo.Authentication.Data;
using Demo.Authentication.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Authentication
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
            services.AddControllersWithViews();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ValidAudience = "api1",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(5)
                    };
                })
                .AddScheme<AuthSchemeOptions, AuthSchemeHandler>("MyCustomScheme", options => { });

            //services.AddAuthorization();
            services.AddAuthorization(op =>
            {
               op.AddPolicy(Policies.RequireAge18Plus,
                    new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new MinAgeRequirement(18))
                        .Build());
            });
            /*
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.RequireAge18Plus,
                    policy => policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == "age"
                            && int.Parse(c.Value) > 18)));
            });
            */
            services.AddTransient<IAuthorizationHandler, AgeAuthorizationHandler>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<Database>();
            
            // Add Cors
            services.AddCors(o => o.AddPolicy("Cors", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
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
            
            app.UseCors("Cors");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Добавляем аутентификацию и авторизацию в обработку запросов
            app.UseAuthentication(); //Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler
            app.UseAuthorization();
            app.UseCustomMiddleware();
            
            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");
                });
        }
    }
}
