using dpOnDotnet.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using dpOnDotnet.Components.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<LibraryConfiguration>();
builder.Services.AddSingleton<GlobalState>();
builder.Services.AddScoped<ISitemapService, SitemapService>(); // Change to AddScoped
builder.Services.AddHttpClient<ISitemapService, SitemapService>(); // Register HttpClient
builder.Services.AddHostedService<SitemapTask>();
builder.Services.AddScoped<ISitemapService, SitemapService>();
builder.Services.AddHostedService<SitemapTask>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
