using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using NLog.Web;
using NLog;
using CarrotSystem.Services;
using CarrotSystem.Helpers;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.EntityFrameworkCore;
using CarrotSystem.Data;
using System;
using CarrotSystem.Areas.Identity.Data;
using CarrotSystem.Models.Context;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Error("Init Server");

try
{
    var builder = WebApplication.CreateBuilder(args);
    //builder.Configuration.AddJsonFile("appsettings.json");
    //var connectionString = builder.Configuration.GetConnectionString("CarrotSystemContextConnection") ?? throw new InvalidOperationException("Connection string 'CarrotSystemContextConnection' not found.");

    // Add services to the container.
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

    // Database Manager
    builder.Services.AddScoped<IContextService, ContextService>();

    // Event Writer
    builder.Services.AddScoped<IEventWriter, EventWriter>();

    // System Service
    builder.Services.AddTransient<ISystemService, SystemService>();

    // Reports Service
    builder.Services.AddTransient<IReportsService, ReportsService>();

    // API Service
    builder.Services.AddTransient<IAPIService, APIService>();

    // Calculation Service
    builder.Services.AddTransient<ICalcService, CalcService>();

    //// Email Settings
    //builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    //builder.Services.AddTransient<IEmailService, EmailService>();

    // MYOB Service
    builder.Services.AddTransient<IMYOBService, MYOBService>();

    // XERO Configuration
    builder.Services.Configure<XeroConfiguration>(builder.Configuration.GetSection("XeroConfiguration"));

    // Create PDF
    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    builder.Services.AddTransient<IGenPDFService, GenPDFService>();

    // Ajax Request
    builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

    // Add DbContext and Identity

    builder.Services.AddDbContext<MPSContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CarrotSystemContextConnection")));

    builder.Services.AddDefaultIdentity<CarrotSystemUser>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<MPSContext>();



    builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromDays(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddMemoryCache();

    builder.Services.AddRazorPages(); // Add Razor Pages services

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseExceptionHandler("/Accounts/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Dashboard}/{id?}");
        endpoints.MapRazorPages(); // Map Razor Pages endpoints
    });
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of Exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}


//using DinkToPdf;
//using DinkToPdf.Contracts;
//using Microsoft.AspNetCore.HttpOverrides;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.DataProtection;
//using NLog.Web;
//using NLog;
//using CarrotSystem.Services;
//using CarrotSystem.Helpers;
//using Xero.NetStandard.OAuth2.Client;
//using Xero.NetStandard.OAuth2.Config;
//using Microsoft.EntityFrameworkCore;
//using CarrotSystem.Data;

//var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
//logger.Error("Init Server");

//try
//{
//    var builder = WebApplication.CreateBuilder(args);

//    //Add services to the container.
//    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//    //Database Manager
//    builder.Services.AddScoped<IContextService, ContextService>();

//    //Event Writer
//    builder.Services.AddScoped<IEventWriter, EventWriter>();

//    //System Service
//    builder.Services.AddTransient<ISystemService, SystemService>();

//    //Reports Service
//    builder.Services.AddTransient<IReportsService, ReportsService>();

//    //API Service
//    builder.Services.AddTransient<IAPIService, APIService>();

//    //Calculation Service
//    builder.Services.AddTransient<ICalcService, CalcService>();

//    //Email Settings
//    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//    builder.Services.AddTransient<IEmailService, EmailService>();

//    //MYOB Service
//    builder.Services.AddTransient<IMYOBService, MYOBService>();

//    //XERO Congfiguration
//    builder.Services.Configure<XeroConfiguration>(builder.Configuration.GetSection("XeroConfiguration"));

//    //Create PDF
//    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
//    builder.Services.AddTransient<IGenPDFService, GenPDFService>();

//    //Ajax Request
//    builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

//    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "_mps_auth";
//        options.Cookie.HttpOnly = false;
//        options.LoginPath = new PathString("/Accounts/login");
//        options.LogoutPath = new PathString("/Accounts/logout");
//        options.AccessDeniedPath = new PathString("/Accounts/login");
//        options.ExpireTimeSpan = TimeSpan.FromDays(1);
//        options.SlidingExpiration = true;
//    });

//    builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//    builder.Services.AddSession(options =>
//    {
//        options.IdleTimeout = TimeSpan.FromDays(1); 
//        options.Cookie.HttpOnly = true;
//        options.Cookie.IsEssential = true;
//    });

//    builder.Services.AddMemoryCache();
//    //builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));

//    //Create PDF
//    builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
//    builder.Services.AddTransient<IGenPDFService, GenPDFService>();

//    //Data Protection Configuration
//    /*
//    var keyFolder = Path.Combine(builder.Environment.ContentRootPath, "Data", "UserKeys");
//    builder.Services.AddDataProtection()
//        .PersistKeysToFileSystem(new DirectoryInfo(keyFolder))
//        .SetDefaultKeyLifetime(TimeSpan.FromDays(30));
//    */

//    var app = builder.Build();

//    if(!app.Environment.IsDevelopment())
//    {
//        app.UseDeveloperExceptionPage();
//        app.UseExceptionHandler("/Accounts/Error");
//        app.UseHsts();
//    }

//    app.UseHttpsRedirection();
//    app.UseStaticFiles();
//    app.UseRouting();
//    app.UseSession();

//    app.UseAuthentication();
//    app.UseAuthorization();

//    app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
//    app.UseForwardedHeaders(new ForwardedHeadersOptions
//    {
//        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
//        ForwardedHeaders.XForwardedProto
//    });

//    app.UseEndpoints(endpoints =>
//    {
//        endpoints.MapControllerRoute(
//            name: "default",
//            pattern: "{controller=Accounts}/{action=Login}/{id?}");
//    });

//    app.Run();
//}
//catch (Exception exception)
//{
//    // NLog: catch setup errors
//    logger.Error(exception, "Stopped program because of Exception");
//    throw;
//}
//finally
//{
//    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
//    NLog.LogManager.Shutdown();
//}
//var connectionString = builder.Configuration.GetConnectionString("CarrotSystemContextConnection") ?? throw new InvalidOperationException("Connection string 'CarrotSystemContextConnection' not found.");

//builder.Services.AddDbContext<CarrotSystemContext>(options =>
//    options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<CarrotSystemUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<CarrotSystemContext>();
