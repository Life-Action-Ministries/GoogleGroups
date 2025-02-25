
using Google.Apis.Auth.AspNetCore3;
using GoogleGroups;
using Microsoft.AspNetCore.Authentication.Cookies;
using static GoogleGroups.Helper;

InitializeDatabase();

string? ClientId = CredVal("ClientID");
string? ClientSecret = CredVal("ClientSecret");

var builder = WebApplication.CreateBuilder();

builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Helper.CnnVal("Redis");
    options.InstanceName = "GoogleGroups_";
});

if (System.IO.File.Exists("./wwwroot/Data/credentials.p12") && Helper.CredVal("ClientID") != "")
{
    builder.Services.AddAuthentication(o =>
    {
        o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
        o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
        o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogleOpenIdConnect(options =>
    {
        options.ClientId = ClientId;
        options.ClientSecret = ClientSecret;
        //options.Scope.Add("openid");
        //options.Scope.Add("profile");
        //options.Scope.Add("email");
        //options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
    });
}

builder.Services.AddDistributedMemoryCache(); // <- This service

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = "local.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMvc();

var app = builder.Build();

//app.Services.GetService<IDistributedCache>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "setup",
    pattern: "{controller=Setup}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
