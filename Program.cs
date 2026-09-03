using dress_ordering_system.Models;
using dress_ordering_system.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace dress_ordering_system
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register OpenAI Chatbot Service
            builder.Services.AddScoped<OpenAIChatService>();

            // Database
            builder.Services.AddDbContext<myContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("myconnection")));

            // HTTP Client
            builder.Services.AddHttpClient();

            // Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=customer}/{action=Index}/{id?}");

            app.Run();
        }
    }
}