using System;
using Chatty.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vektora.Authentication;

namespace Chatty
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Heroku Postgre Connection string alınması
            services.AddScoped<PostgreHerokuConnectionProvider>();

            //Heroku MongoDB Connection string alınması. Conversation Repository bu veritabanını kullanıyor.
            services.AddScoped<MongoDBHerokuConnectionProvider>();

            services.AddScoped<IConversationRepository, ConversationRepository>();

            //Heroku Postge Connection string set edilmesi.
            services.AddDbContext<KaleUstaPortalContext>(opt => opt.UseNpgsql(services.BuildServiceProvider().GetService<PostgreHerokuConnectionProvider>().GetConnectionString(AvailablePostgreConnectionStrings.Default)));

            services.AddVektoraAuthentication<IdentityUser, IdentityRole, KaleUstaPortalContext>(cookieOptionAction: options =>
                {
                    options.Cookie.IsEssential = true;
                    options.Cookie.HttpOnly = false;
                    options.Cookie.Name = "auth-cookie";
                });
            services.Add(ServiceDescriptor.Singleton<IUserIdProvider, UserIdProvider>());
            services.AddSignalR().AddJsonProtocol();
            services.AddCors(options => options.AddPolicy("Default", builder =>
                    builder
                        .AllowAnyOrigin()  //.WithOrigins("http://localhost:5000")
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyHeader()
            ));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseCors("Default");
            app.UseAuthentication();
            app.UseSignalR(routes =>
            {
                routes.MapHub<MessageHub>("/message-hub", options =>
                {
                    options.WebSockets.CloseTimeout = TimeSpan.FromMinutes(5);
                });
            });
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }

    public class KaleUstaPortalContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public KaleUstaPortalContext(DbContextOptions<KaleUstaPortalContext> options) : base(options) { }
    }
}
