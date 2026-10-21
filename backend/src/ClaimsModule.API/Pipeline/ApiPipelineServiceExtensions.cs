using System.Text.Json.Serialization;
using ClaimsModule.API.DevAuth;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Pipeline.Correlation;
using ClaimsModule.API.Pipeline.Cors;
using ClaimsModule.API.Pipeline.Health;
using ClaimsModule.API.Pipeline.Idempotency;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.API.Pipeline.Jwt;
using ClaimsModule.API.Pipeline.Swagger;
using ClaimsModule.Infrastructure;

namespace ClaimsModule.API.Pipeline;

public static class ApiPipelineServiceExtensions
{
    public static IServiceCollection AddApiPipeline(this IServiceCollection services)
    {
        services
            .AddCorrelation()
            .AddJwt()
            .AddDevAuth()
            .AddErrors()
            .AddIdempotency()
            .AddFrontendCors()
            .AddHealth()
            .AddSwaggerDocs();
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)));
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));
        services.AddProblemDetails();

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseCorrelation();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseCors(FrontendCorsServiceExtensions.FrontendPolicyName);
        app.UseInfrastructure();
        app.UseSwaggerDocs();
        app.UseAuthentication();
        app.UseCurrentUser();
        app.UseAuthorization();
        app.UseHealth();
        app.MapControllers();

        return app;
    }
}
