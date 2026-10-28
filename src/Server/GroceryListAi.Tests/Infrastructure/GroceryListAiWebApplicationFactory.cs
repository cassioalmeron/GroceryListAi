using GroceryListAi.Core;
using GroceryListAi.Core.Models;
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
    public const int GeneralListId = 1;

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
            services.AddScoped(_ =>
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.Services.AddSingleton<IChatCompletionService>(
                    new FakeChatCompletionService(_chatResponder));

                return kernelBuilder.Build();
            });
        });

    public async Task<int> CreateListAsync(string name)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GroceryListAiDbContext>();

        var list = new List { Name = name };
        dbContext.Lists.Add(list);
        await dbContext.SaveChangesAsync();

        return list.Id;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}
