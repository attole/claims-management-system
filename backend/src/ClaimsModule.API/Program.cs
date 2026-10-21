using ClaimsModule.API.Pipeline;
using ClaimsModule.Application;
using ClaimsModule.Infrastructure;
using ClaimsModule.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPersistence()
    .AddInfrastructure()
    .AddApiPipeline();

var app = builder.Build();

app.UseApiPipeline();

app.Run();

public partial class Program;
