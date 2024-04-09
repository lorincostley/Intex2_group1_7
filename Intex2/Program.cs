using Intex2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var services = builder.Services;
var configuration = builder.Configuration;

if (builder.Environment.IsDevelopment())
{
    // If a development env, then use the secrets.json for key vault authentication
    builder.Configuration.AddUserSecrets<Program>();

    var clientId = configuration["KeyVault:ClientId"];
    var tenantId = configuration["KeyVault:TenantId"];
    var clientSecret = configuration["KeyVault:ClientSecret"];

    // Creates a secret client
    var client = new SecretClient(
        new Uri("https://intex2oauthkeyvaultpt2.vault.azure.net/"),
        new ClientSecretCredential(tenantId, clientId, clientSecret)
    );

    // Authenticates with Google
    KeyVaultSecret GoogleClientSecret = client.GetSecret("GoogleClientSecret");
    services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = GoogleClientSecret.Value;
    });
} else
{
   SecretClientOptions options = new SecretClientOptions()
    {
        Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
             }
    };
    var client = new SecretClient(new Uri("https://intex2oauthkeyvaultpt2.vault.azure.net/"), new DefaultAzureCredential(), options);
    KeyVaultSecret GoogleClientSecret = client.GetSecret("GoogleClientSecret");
    services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = GoogleClientSecret.Value;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
