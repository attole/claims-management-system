using ClaimsModule.API.Configuration.Cors;
using ClaimsModule.API.Configuration.Jwt;
using ClaimsModule.API.Configuration.Swagger;
using ClaimsModule.Application;
using ClaimsModule.Infrastructure;
using ClaimsModule.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPersistence()
    .AddInfrastructure()
    .AddJwt()
    .AddFrontendCors();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerDocs();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(FrontendCorsServiceExtensions.FrontendPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseInfrastructure();

app.UseSwaggerDocs();
app.MapControllers();

app.Run();
