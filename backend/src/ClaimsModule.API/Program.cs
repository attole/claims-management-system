using ClaimsModule.API.Correlation;
using ClaimsModule.API.Cors;
using ClaimsModule.API.DevAuth;
using ClaimsModule.API.Identity;
using ClaimsModule.API.Jwt;
using ClaimsModule.API.Swagger;
using ClaimsModule.Application;
using ClaimsModule.Infrastructure;
using ClaimsModule.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPersistence()
    .AddInfrastructure()
    .AddCorrelation()
    .AddJwt()
    .AddDevAuth()
    .AddFrontendCors();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerDocs();

var app = builder.Build();

app.UseCorrelation();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(FrontendCorsServiceExtensions.FrontendPolicyName);
app.UseInfrastructure();
app.UseSwaggerDocs();
app.UseAuthentication();
app.UseCurrentUser();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
