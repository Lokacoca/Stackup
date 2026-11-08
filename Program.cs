using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register IHttpContextAccessor for Session access
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// ?? Disable Browser Link in development mode (Move this before app.Build())
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options => options.AllowSynchronousIO = true);

    builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcViewOptions>(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = false;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    // Show developer exception page for debugging
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseStaticFiles();
app.UseRouting();
app.UseSession();  // Session middleware must be before Authorization
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "callback",
    pattern: "oauth2-callback",
    defaults: new { controller = "Home", action = "OAuthCallback" }
);

app.Run();
