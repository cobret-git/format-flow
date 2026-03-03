using FormatFlow.WebApp.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();