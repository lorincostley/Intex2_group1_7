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

internal class Program
{
    private static void Main(string[] args)
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

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddRoles<IdentityRole>();

        // Add role-based policy
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdministratorRole",
                 policy => policy.RequireRole("Administrator"));
        });

        builder.Services.AddRazorPages();

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

        builder.Services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.SlidingExpiration = true;
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


        //string modelPath = "C:\\Users\\kbangerter\\source\\repos\\lorincostley\\Intex2_group1_7\\Intex2\\gradient_model.onnx";

        //// Load the model
        //var sessionOptions = new Microsoft.ML.OnnxRuntime.SessionOptions();
        //using (var session = new InferenceSession(modelPath, sessionOptions))
        //{
        //    // Model loaded successfully, you can use the session for inference
        //    // For example, you can run inference on input data
        //    // var inputTensor = ...; // Prepare input tensor
        //    // var results = session.Run(...); // Perform inference
        //}

        //Console.WriteLine("Model loaded successfully.");




        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseSession();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();



        app.MapControllerRoute("pagenumandtype", "{projectType}/{pageNum}", new { Conroller = "Home", action = "Index" });
        app.MapControllerRoute("projectType", "{projectType}", new { Controller = "Home", action = "Index", pageNum = 1 });
        app.MapControllerRoute("pagination", "Projects/{pageNum}", new { Controller = "Home", action = "Index" });
        app.MapDefaultControllerRoute();

        app.MapRazorPages();

        app.Run();
    }
}
