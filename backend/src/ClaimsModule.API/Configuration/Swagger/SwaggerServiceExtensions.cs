using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Configuration.Swagger;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services
            .AddOptions<SwaggerDocsOptions>()
            .BindConfiguration(SwaggerDocsOptions.SectionName);

        services.AddOpenApi();

        return services;
    }

    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SwaggerDocsOptions>>().Value;

        if (!options.Enabled)
        {
            return app;
        }

        app.MapOpenApi();
        app.UseSwaggerUI(ui => ui.SwaggerEndpoint("/openapi/v1.json", "Claims API v1"));

        return app;
    }
}
