using ASPWeb.Interfaces;
using ASPWeb.Logging;
using ASPWeb.Models;
using ASPWeb.Repository;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ASPWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSession(options => 
            {
                options.IdleTimeout = TimeSpan.FromMinutes(600);
            });
            builder.Services.AddDbContext<ApplicationContext>();
            builder.Services.AddTransient<IWorkersRep, WorkersRepository>();
            builder.Services.AddTransient<IGroupsRep, GroupsRepository>();
            builder.Services.AddTransient<DataManager>();
           

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapRazorPages();

            app.UseSession();

            app.Run();
        }
    }
}