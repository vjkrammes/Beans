using AspNetCoreRateLimit;

using Beans.API.Infrastructure;
using Beans.Models;
using Beans.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 
// Configure services
//

builder.Services.AddControllers();
builder.Services.ConfigureServices(builder.Configuration);

var origins = builder.Configuration.GetSection("CORSOrigins").Get<string[]>();
if (origins is null || !origins.Any())
{
    builder.Services.AddCors(
        options => options.AddPolicy("DefaultCORS",
            builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
}
else
{
    builder.Services.AddCors(
        options => options.AddPolicy("DefaultCORS",
            builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
}

var settings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new();

var app = builder.Build();

//
// Configure middleware pipeline
//

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        Console.WriteLine($"Endpoint: {context.GetEndpoint()?.DisplayName ?? "* null *"}");
        await next(context);
    });
}
else
{
    app.UseHsts();
}

app.UseCors("DefaultCORS");

app.UseHttpsRedirection();
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.ConfigureEndpoints();

if (settings.UpdateDatabase)
{
    await UpdateDatabase(app.Services.GetRequiredService<IDatabaseBuilder>());
}

app.Run();

static async Task UpdateDatabase(IDatabaseBuilder builder) => await builder.BuildDatabaseAsync(false);