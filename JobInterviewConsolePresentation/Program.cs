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
        // Konfiguráció
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Dependency Injection
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("JobInterviewDataAccess")));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IPracticeService, PracticeService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IDailyPackageService, DailyPackageService>();
        services.AddScoped<IImportExportService, ImportExportService>();
        services.AddScoped<IWeeklySummaryService, WeeklySummaryService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<ConsoleApp>();
        var provider = services.BuildServiceProvider();


        // Adatbázis inicializálás 
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

        // Alkalmazás indítása 
        using (var scope = provider.CreateScope())
        {
            var app = scope.ServiceProvider.GetRequiredService<ConsoleApp>();
            await app.RunAsync();
        }
    }
}
