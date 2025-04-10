using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerSideProg.Codes;
using ServerSideProg.Components;
using ServerSideProg.Components.Account;
using ServerSideProg.Data;
using ServerSideProg.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<SymmetriskEncryptionHandler>();
builder.Services.AddScoped<HashingHandler>();
builder.Services.AddScoped<AsymmetriskEncryption>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var todoConnection = builder.Configuration.GetConnectionString("Todoconnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlServer(todoConnection));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("RequireAdministartorRole", policy =>
    {
        policy.RequireRole("Admin");
    });
});
//.Replace("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
string kestrelCertUrl = builder.Configuration.GetValue<string>("KestrelCertUrl").Replace("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
string kestrelCertKey = builder.Configuration.GetValue<string>("KestrelCertKey").Replace("%USERPROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

builder.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate:Path").Value = kestrelCertUrl;
builder.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate:Password").Value = kestrelCertKey;

builder.WebHost.UseKestrel(options =>
    {
        options.Configure(builder.Configuration.GetSection("Kestrel"))
        .Endpoint("HTTPS", https =>
        {
            https.HttpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        });
    });

//builder.WebHost.ConfigureKestrel((context, serverOptions) =>
//{
//    serverOptions.Listen(IPAddress.Loopback, 7000);

//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
