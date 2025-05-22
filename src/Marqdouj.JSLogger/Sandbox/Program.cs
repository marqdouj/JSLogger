using Microsoft.FluentUI.AspNetCore.Components;
using Sandbox.Components;
using Marqdouj.JSLogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

/*
For the purpose of this demo, both type of loggers are configured. 
normally you would only configure one type of logger service.
*/
builder.AddLoggerModule(null);
builder.AddLoggerService(null); /*See `App.Razor` for how to add the global script required for this service */

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
