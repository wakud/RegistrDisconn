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
            //�������� ����� ���������� �� ��
            string connection = Utils.Decrypt(Configuration.GetConnectionString("TEPConnection"));
            BillingUtils.Configuration = Configuration;

            //��������� �� 
            _ = services.AddDbContext<MainContext>(options => options.UseSqlServer(connection));

            // ����������� �����������
            _ = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });

            //������ �������� �� ������������
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
            // ���� ���������� � �������� ����������
            //if (env.IsDevelopment())
            //{
            //    // �� ������� ���������� �� ������, ��� ������� ������
            //}
            //else
            //{
            //    _ = app.UseExceptionHandler("/Home/Error");
            //    _ = app.UseHsts();
            //}

            _ = app.UseDeveloperExceptionPage();
            _ = app.UseHttpsRedirection();  //�������������� ��� ������� HTTP �� HTTPS
            _ = app.UseStaticFiles();       //������������� ��������� ��������� ����������� ������
            _ = app.UseRouting();           // ��������� ����������� �������������
            _ = app.UseAuthentication();    // ��������������
            _ = app.UseAuthorization();     // �����������

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

            //���������� ���� �����  � �����
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
            DbInitialization.Initial(context);
        }
    }
}
