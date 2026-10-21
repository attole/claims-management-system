using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace ClaimsModule.API.Pipeline.Swagger;

public static class SwaggerDocsServiceExtensions
{
    private const string BearerSchemeId = "Bearer";
    private const string BearerNote =
        "Authorize with a bearer token. The dev token from POST /api/dev/token simulates a sign-in.";

    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services
            .AddOptions<SwaggerDocsOptions>()
            .BindConfiguration(SwaggerDocsOptions.SectionName);

        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            var bearer = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = BearerNote,
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = BearerSchemeId },
            };

            document.Info.Description = BearerNote;
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes[BearerSchemeId] = bearer;
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement { [bearer] = [] });

            return Task.CompletedTask;
        }));

        return services;
    }

    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SwaggerDocsOptions>>().Value;

        if (!options.Enabled)
        {
            return app;
        }

        app.MapOpenApi().AllowAnonymous();
        app.UseSwaggerUI(ui => ui.SwaggerEndpoint("/openapi/v1.json", "Claims API v1"));

        return app;
    }
}
