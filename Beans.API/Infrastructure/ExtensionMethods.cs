using AspNetCoreRateLimit;

using Beans.API.Authorization;
using Beans.API.Endpoints;
using Beans.API.Models;
using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Models;
using Beans.Repositories;
using Beans.Repositories.Interfaces;
using Beans.Services;
using Beans.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

using System.IdentityModel.Tokens.Jwt;

namespace Beans.API.Infrastructure;

public static class ExtensionMethods
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // rate limiting

        services.AddOptions();
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimit"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // IDatabase and IDatabaseBuilder

        var dbsettings = configuration.GetSection("Database").Get<DatabaseSettings>();
        if (dbsettings is null)
        {
            dbsettings = new();
        }
        services.AddSingleton<IDatabase>(x => new Database(dbsettings.Server, dbsettings.Name, dbsettings.Auth));
        services.AddSingleton<IDatabaseBuilder, DatabaseBuilder>();

        // app settings

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        var settings = configuration.GetSection("AppSettings").Get<AppSettings>();
        if (settings is null)
        {
            settings = new();
        }

        // authentication and authorization

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Auth0:Authority"];
                options.Audience = configuration["Auth0:Audience"];
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.ADMIN_REQUIRED, 
                policy => policy.Requirements.Add(new AdminRequirement(true)));
            options.AddPolicy(Constants.NON_ADMIN_REQUIRED, 
                policy => policy.Requirements.Add(new AdminRequirement(false)));
        });
        services.AddScoped<IAuthorizationHandler, AdminHandler>();

        // miscellaneous services

        services.AddSingleton<IBreakpointManager, BreakpointManager>();
        services.AddSingleton<IColorService, ColorService>();
        services.AddTransient<IConfigurationFactory, ConfigurationFactory>();
        services.AddHttpContextAccessor();
        services.AddTransient<HttpStatusCodeTranslator, HttpStatusCodeTranslator>();
        services.AddTransient<INormalRandom, NormalRandom>();
        services.AddTransient<ITimeSpanConverter, TimeSpanConverter>();
        services.AddTransient<IUriHelper, UriHelper>();

        // data respositories

        services.AddTransient<IBeanRepository, BeanRepository>();
        services.AddTransient<IHoldingRepository, HoldingRepository>();
        services.AddTransient<ILogRepository, LogRepository>();
        services.AddTransient<IMovementRepository, MovementRepository>();
        services.AddTransient<INoticeRepository, NoticeRepository>();
        services.AddTransient<IOfferRepository, OfferRepository>();
        services.AddTransient<ISaleRepository, SaleRepository>();
        services.AddTransient<ISettingsRepository, SettingsRepository>();
        services.AddTransient<IUserRepository, UserRepository>();

        // data seeders

        services.AddTransient<IBeanSeeder, BeanSeeder>();
        services.AddTransient<ILogSeeder, LogSeeder>();
        services.AddTransient<IMovementSeeder, MovementSeeder>();
        services.AddTransient<INoticeSeeder, NoticeSeeder>();
        services.AddTransient<ISettingsSeeder, SettingsSeeder>();

        // data services

        services.AddTransient<IBeanService, BeanService>();
        services.AddTransient<IHoldingService, HoldingService>();
        services.AddTransient<ILogService, LogService>();
        services.AddTransient<IMovementService, MovementService>();
        services.AddTransient<INoticeService, NoticeService>();
        services.AddTransient<IOfferService, OfferService>();
        services.AddTransient<ISaleService, SaleService>();
        services.AddTransient<ISettingsService, SettingsService>();
        services.AddTransient<IUserService, UserService>();

        return services;
    }

    public static void ConfigureEndpoints(this WebApplication app)
    {
        app.ConfigureBeanEndpoints();
        app.ConfigureHoldingEndpoints();
        app.ConfigureLogEndpoints();
        app.ConfigureMovementEndpoints();
        app.ConfigureNoticeEndpoints();
        app.ConfigureOfferEndpoints();
        app.ConfigureSaleEndpoints();
        app.ConfigureSettingsEndpoints();
        app.ConfigureUserEndpoints();
    }

    public static string? GetTokenString(this HttpRequest request) => request?.Headers["Authorization"].FirstOrDefault()?.Split(new char[] { ' ' }).Last();

    public static string? GetTokenString(this HttpContext context) => context?.Request.GetTokenString();

    public static JwtSecurityToken? GetToken(this HttpRequest request)
    {
        var token = request.GetTokenString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }

    public static JwtSecurityToken? GetToken(this HttpContext context) => context?.Request.GetToken();
}
