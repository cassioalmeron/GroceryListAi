var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var grocerylist = postgres.AddDatabase("grocerylistai");

var api = builder.AddProject<Projects.GroceryListAi_Api>("api")
    .WithEnvironment("DATABASE_PROVIDER", "Postgres")
    .WithEnvironment("CONNECTION_STRING", grocerylist)
    .WaitFor(grocerylist);

builder.AddNpmApp("web", "../Web", "dev")
    .WithHttpEndpoint(port: 81)
    .WithExternalHttpEndpoints()
    .WaitFor(api);

builder.Build().Run();
