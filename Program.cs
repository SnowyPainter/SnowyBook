using System.Net;
using Harbor.Cargo;
using Harbor.Ship;
using Microsoft.AspNetCore.Diagnostics;

namespace SnowyBook
{
    public class Program
    {
        public static string DefaultSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static LocalShip Localship;
        public static DataCargo ReviseLog = new DataCargo();
        public static void AddLog(string who, string what)
        {
            var d = new Data { Content = $"{who}:{what}" };
            ReviseLog.Load(d);
        }
        static void Main()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllersWithViews();

            Localship = new LocalShip(
                new Dictionary<CargoType, string>() //Hidden folders
                {
                    {CargoType.GenericObject, $@"{DefaultSavePath}/g"},
                    {CargoType.Text, $@"{DefaultSavePath}/t"},
                    {CargoType.Voice, $@"{DefaultSavePath}/v"},
                    {CargoType.Log, $@"{DefaultSavePath}/l"}
                },
                $@"{DefaultSavePath}\d" //Public folder
            );

            var app = builder.Build();

            //error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        ctx.Response.ContentType = "text/html";
                        var ex = ctx.Features.Get<IExceptionHandlerFeature>();
                        if (ex != null)
                        {
                            var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                            await ctx.Response.WriteAsync(err).ConfigureAwait(false);
                        }
                    });
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}"
            );
            app.MapControllerRoute(
                name: "edit",
                pattern: "{controller=Home}/{action=Edit}/{doc?}/{id?}"
            );
            app.MapControllerRoute(
                name: "save",
                pattern: "{controller=Home}/{action=Save}/{doc?}"
            );
            app.MapControllerRoute(
                name: "pullaway",
                pattern: "{controller=Home}/{action=Pullaway}/"
            );
            app.Run();
        }
    }
}