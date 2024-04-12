using Intex2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Intex2.Models;
using Microsoft.AspNetCore.Builder;
using System.Drawing.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Elfie.Serialization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<ILegoRepository, EFLegoRepository>();

        builder.Services.AddRazorPages();

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession();

        builder.Services.AddScoped(sp => SessionCart.GetCart(sp));
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        builder.Configuration.AddUserSecrets<Program>();
        var connectionSTring = builder.Configuration["ConnectionStrings:ApplicationConnection"];

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

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

            // Connect to client
            KeyVaultSecret ConnectionSecret = client.GetSecret("ConnectionSecret");

            builder.Services.AddDbContext<LegoContext>(options =>
                options.UseSqlServer(ConnectionSecret.Value, options =>
                    options.EnableRetryOnFailure())
                );
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionSecret.Value, options =>
                    options.EnableRetryOnFailure())
                );

        }
        else
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

            KeyVaultSecret ConnectionSecret = client.GetSecret("ConnectionSecret");

            builder.Services.AddDbContext<LegoContext>(options =>
                options.UseSqlServer(ConnectionSecret.Value, options =>
                    options.EnableRetryOnFailure())
                );
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionSecret.Value, options =>
                    options.EnableRetryOnFailure())
                );
        }

        builder.Services.AddRazorPages();

        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();


        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        });

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential 
            // cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.ConsentCookieValue = "true";
        });

        // Add role-based policy
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole",
                 policy => policy.RequireRole("Admin"));
        });

        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(60);
        });

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

        app.UseSession();
        app.UseCookiePolicy();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self';" +
            "script-src 'self' https://stackpath.bootstrapcdn.com/ 'sha256-m1igTNlg9PL5o60ru2HIIK6OPQet2z9UgiEAhCyg/RU=';" +
            "img-src data: https:;" +
            "style-src https://stackpath.bootstrapcdn.com/ 'self' 'unsafe-inline';" +
            "connect-src 'self' wss: http://localhost:52827 ws://localhost:52827");
            await next();
        });

        //app.MapControllerRoute("pagenumandtypeandcolor", "{projectType}/{color}/{pageNum}", new { controller = "Home", action = "Index" });

        //app.MapControllerRoute("pagenumandtype", "{projectType}/{pageNum}", new { controller = "Home", action = "Index" });

        //app.MapControllerRoute("pagenumandcolor", "{color}/{pageNum}", new { controller = "Home", action = "Index" });

        //app.MapControllerRoute("projectType", "{projectType}", new { controller = "Home", action = "Index", pageNum = 1 });

        //app.MapControllerRoute("pagination", "Projects/{pageNum}", new { controller = "Home", action = "Index" });



        app.MapControllerRoute("pagenumandtype", "{projectType}/{pageNum}", new { Conroller = "Home", action = "Index" });
        app.MapControllerRoute("projectType", "{projectType}", new { Controller = "Home", action = "Index", pageNum = 1 });
        app.MapControllerRoute("pagination", "Projects/{pageNum}", new { Controller = "Home", action = "Index" });
        app.MapDefaultControllerRoute();



        app.MapRazorPages();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    IdentityRole roleRole = new IdentityRole(role);
                    await roleManager.CreateAsync(roleRole);
                }
            }
        }

        app.Run();
    }
}
