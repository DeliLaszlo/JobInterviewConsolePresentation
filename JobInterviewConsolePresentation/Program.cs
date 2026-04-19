using JobInterviewBusinessLogic.Services;
using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;
using JobInterviewDataAccess;
using JobInterviewDataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace JobInterviewConsolePresentation;

public class Program
{
    public static async Task Main(string[] args)
    {
        // ── Konfiguráció ──
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // ── DI konténer ──
        var services = new ServiceCollection();

        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("JobInterviewDataAccess")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();

        // Services
        services.AddScoped<IPracticeService, PracticeService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IDailyPackageService, DailyPackageService>();
        services.AddScoped<IImportExportService, ImportExportService>();
        services.AddScoped<IWeeklySummaryService, WeeklySummaryService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IBookmarkService, BookmarkService>();

        // Configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Console App
        services.AddScoped<ConsoleApp>();

        var provider = services.BuildServiceProvider();

        // -- Adatbazis inicializalas --
        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(new Style(Color.DodgerBlue2))
                .StartAsync("Adatbazis inicializalasa...", async _ =>
                {
                    using var scope = provider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await context.Database.EnsureCreatedAsync();

                    // Schema frissites: ha a Question tablaban nincs Answer oszlop,
                    // akkor ujra kell epiteni az adatbazist (fejlesztesi fazisban)
                    try
                    {
                        var conn = context.Database.GetDbConnection();
                        await conn.OpenAsync();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Questions' AND COLUMN_NAME = 'Answer'";
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && (int)result == 0)
                        {
                            await context.Database.EnsureDeletedAsync();
                            await context.Database.EnsureCreatedAsync();
                        }
                    }
                    catch { /* Ha a tabla meg nem letezik, az EnsureCreated mar letrehozta */ }

                    await DbSeeder.SeedAsync(context);
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[bold red]Adatbazis kapcsolodasi hiba![/]");
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            AnsiConsole.MarkupLine("\n[yellow]Ellenorizd:[/]");
            AnsiConsole.MarkupLine("[yellow]  1. Fut-e az SQL Server Express (SQLEXPRESS) szolgaltatas[/]");
            AnsiConsole.MarkupLine("[yellow]  2. Helyes-e a connection string az appsettings.json-ben[/]");
            AnsiConsole.MarkupLine("\n[grey]Nyomj ENTER-t a kilepeshez...[/]");
            Console.ReadLine();
            return;
        }

        // ── Alkalmazás indítása ──
        using (var scope = provider.CreateScope())
        {
            var app = scope.ServiceProvider.GetRequiredService<ConsoleApp>();
            await app.RunAsync();
        }
    }
}
