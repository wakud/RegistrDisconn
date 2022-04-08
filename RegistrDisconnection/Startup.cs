//using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistrDisconnection.Data;
using RegistrDisconnection.MyClasses;
using System;

namespace RegistrDisconnection
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
            //отримуємо рядок підключення до БД
            string connection = Utils.Decrypt(Configuration.GetConnectionString("TEPConnection"));
            BillingUtils.Configuration = Configuration;

            //створюємо БД 
            _ = services.AddDbContext<MainContext>(options => options.UseSqlServer(connection));

            // авторизація користувача
            _ = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });

            //робимо перевірку на адміністратора
            _ = services.AddAuthorization(opts =>
              {
                  opts.AddPolicy("OnlyForAdministrator", policy =>
                  {
                      _ = policy.RequireClaim("PravaId", "1");
                  });
              });

            _ = services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // если приложение в процессе разработки
            //if (env.IsDevelopment())
            //{
            //    // то выводим информацию об ошибке, при наличии ошибки
            //}
            //else
            //{
            //    _ = app.UseExceptionHandler("/Home/Error");
            //    _ = app.UseHsts();
            //}

            _ = app.UseDeveloperExceptionPage();
            _ = app.UseHttpsRedirection();  //перенаправляет все запросы HTTP на HTTPS
            _ = app.UseStaticFiles();       //предоставляет поддержку обработки статических файлов
            _ = app.UseRouting();           // добавляем возможности маршрутизации
            _ = app.UseAuthentication();    // аутентификация
            _ = app.UseAuthorization();     // авторизация

            _ = app.UseEndpoints(endpoints =>
              {
                  _ = endpoints.MapControllerRoute(
                      name: "default",
                      pattern: "{controller=Home}/{action=Index}/{id?}"
                  );

                  _ = endpoints.MapControllerRoute(
                      name: "users",
                      pattern: "{controller=Users}/{action=Index}/{id?}"
                  );
                  _ = endpoints.MapControllerRoute(
                      name: "active",
                      pattern: "{controller=Poper}/{action=SetActivePerson}/{id?}"
                  );
              });

            //наповнюємо табл юзерів  і права
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
            DbInitialization.Initial(context);
        }
    }
}
