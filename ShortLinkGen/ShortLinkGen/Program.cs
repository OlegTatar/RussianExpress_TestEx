using Microsoft.OpenApi.Models;
using ShortLinkGen.BusinessLogic;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
IConfiguration Configuration = builder.Configuration;

await ConfigureServices(builder.Services, builder, Configuration);

var app = builder.Build();
ConfigureMiddleWare(app, builder);

app.Run();

async Task ConfigureServices(IServiceCollection services, WebApplicationBuilder builder, IConfiguration configuration)
{
    services.AddControllers();

    services.AddCors(options => options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed((host) => true)
                .AllowCredentials();
        }));

    services.AddHttpContextAccessor();

    services.AddSwaggerGen(config =>
    {
        config.SwaggerDoc("v1.0", new OpenApiInfo() { Title = "«Генератор коротких ссылок для Русский Экспресс»", Version = "v1.0" });
    });

    var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new Exception("Не удалось получить строку подключения для Redis");

    var redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
    builder.Services.AddSingleton(redisConnection);
    builder.Services.AddTransient<ShortLinkProcessor>();
}

void ConfigureMiddleWare(WebApplication app, WebApplicationBuilder builder)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.DefaultModelsExpandDepth(-1);
        config.SwaggerEndpoint("/swagger/v1.0/swagger.json", nameof(ShortLinkGen));
    });

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseCors("AllowAll");

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    app.MapGet("/{path}", (string path, ShortLinkProcessor shortLinkProcessor) =>
    {
        var shortUrl = shortLinkProcessor.Get(path);
        if (shortUrl == null || string.IsNullOrEmpty(shortUrl.Source))
            return Results.NotFound();

        return Results.Redirect(shortUrl.Source);
    });

    app.Run();
}
