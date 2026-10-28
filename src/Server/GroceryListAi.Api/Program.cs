using DotNetEnv;
using GroceryListAi.Api;
using GroceryListAi.Core;
using GroceryListAi.Core.Llm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.SemanticKernel;
using Serilog;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var corsOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

const string CorsPolicyName = "Configured";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicyName, policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var databaseProvider = Enum.Parse<DatabaseProvider>(
    Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? nameof(DatabaseProvider.Sqlite),
    ignoreCase: true);

builder.Services.AddDbContext<GroceryListAiDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
        ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is required.");

    if (databaseProvider != DatabaseProvider.Postgres)
    {
        options.UseSqlite(connectionString);
        return;
    }

    // The shared migrations' snapshot is generated against SQLite, so Postgres always reports a false model diff.
    options.UseNpgsql(connectionString)
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddInfrastructuralServices();
builder.Services.AddScoped<GroceryListPlugin>();
builder.Services.AddScoped(serviceProvider =>
{
    var kernel = KernelFactory.Create(LlmSettings.FromEnvironment());
    kernel.Plugins.AddFromObject(serviceProvider.GetRequiredService<GroceryListPlugin>(), "GroceryList");
    return kernel;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GroceryListAiDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSerilogRequestLogging();
app.UseCors(CorsPolicyName);

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { message = "Grocery List API is running" }));

app.Run();

public partial class Program;
