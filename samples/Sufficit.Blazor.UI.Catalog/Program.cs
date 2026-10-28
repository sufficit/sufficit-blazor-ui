using Sufficit.Blazor.UI;
using Sufficit.Blazor.UI.Catalog.Components;

var builder = WebApplication.CreateBuilder(args);

// The catalog is exercised from Release builds outside the Development
// environment. Load the development static-web-assets manifest explicitly so
// project and referenced RCL assets keep their physical content roots.
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSufficitUI();

var app = builder.Build();
var pathBase = builder.Configuration["PathBase"];

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}
// Strict-CSP fixture: ?csp=strict serves the policy identity runs in
// production, under which browsers ignore every style attribute. The browser
// tests use it to prove SUI's computed layout survives (StrictCspBrowserTests).
app.Use(async (context, next) =>
{
    if (context.Request.Query["csp"] == "strict")
        context.Response.Headers.Append("Content-Security-Policy", "style-src-attr 'none'");
    await next();
});
app.UseRouting();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
