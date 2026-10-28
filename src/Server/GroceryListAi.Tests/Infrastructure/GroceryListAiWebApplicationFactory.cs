using GroceryListAi.Core;
using GroceryListAi.Core.Llm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace GroceryListAi.Tests.Infrastructure;

public class GroceryListAiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>>? _chatResponder;

    public GroceryListAiWebApplicationFactory(
        Func<Kernel?, IAsyncEnumerable<StreamingChatMessageContent>>? chatResponder = null)
    {
        _connection.Open();
        _chatResponder = chatResponder;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<GroceryListAiDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<GroceryListAiDbContext>>();

            services.AddDbContext<GroceryListAiDbContext>(options =>
                options.UseSqlite(_connection));

            if (_chatResponder is null)
                return;

            services.RemoveAll<Kernel>();
            services.AddScoped(serviceProvider =>
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.Services.AddSingleton<IChatCompletionService>(
                    new FakeChatCompletionService(_chatResponder));

                var kernel = kernelBuilder.Build();
                kernel.Plugins.AddFromObject(serviceProvider.GetRequiredService<GroceryListPlugin>(), "GroceryList");

                return kernel;
            });
        });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}
