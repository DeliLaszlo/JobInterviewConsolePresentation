using JobInterviewCore.DTOs;
using JobInterviewCore.Entities;
using JobInterviewCore.Enums;
using JobInterviewCore.Interfaces;
using Spectre.Console;

namespace JobInterviewConsolePresentation;

public class ConsoleApp
{
    private readonly IPracticeService _practiceService;
    private readonly IStatisticsService _statisticsService;
    private readonly IDailyPackageService _dailyPackageService;
    private readonly IImportExportService _importExportService;
    private readonly IWeeklySummaryService _weeklySummaryService;
    private readonly IQuestionService _questionService;
    private readonly IBookmarkService _bookmarkService;

    public ConsoleApp(
        IPracticeService practiceService,
        IStatisticsService statisticsService,
        IDailyPackageService dailyPackageService,
        IImportExportService importExportService,
        IWeeklySummaryService weeklySummaryService,
        IQuestionService questionService,
        IBookmarkService bookmarkService)
    {
        _practiceService = practiceService;
        _statisticsService = statisticsService;
        _dailyPackageService = dailyPackageService;
        _importExportService = importExportService;
        _weeklySummaryService = weeklySummaryService;
        _questionService = questionService;
        _bookmarkService = bookmarkService;
    }

    // =============================================
    //  Fomenu
    // =============================================

    public async Task RunAsync()
    {
        AnsiConsole.Clear();
        DisplayBanner();

        while (true)
        {
            AnsiConsole.WriteLine();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold dodgerblue2]=== Fomenu ===[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(Color.DodgerBlue2))
                    .AddChoices(
                        "1. Gyakorlo mod",
                        "2. Kerdes hozzaadasa",
                        "3. Statisztikak",
                        "4. Konyvjelzok",
                        "5. Import / Export",
                        "6. Napi csomag",
                        "7. Heti osszefoglalo",
                        "8. Kilepes"));

            AnsiConsole.Clear();
            DisplayBanner();

            switch (choice)
            {
                case "1. Gyakorlo mod":
                    await PracticeModeAsync();
                    break;
                case "2. Kerdes hozzaadasa":
                    await AddQuestionAsync();
                    break;
                case "3. Statisztikak":
                    await ShowStatisticsMenuAsync();
                    break;
                case "4. Konyvjelzok":
                    await ShowBookmarksAsync();
                    break;
                case "5. Import / Export":
                    await ImportExportMenuAsync();
                    break;
                case "6. Napi csomag":
                    await DailyPackageAsync();
                    break;
                case "7. Heti osszefoglalo":
                    await WeeklySummaryMenuAsync();
                    break;
                case "8. Kilepes":
                    AnsiConsole.MarkupLine("[grey]Viszlat![/]");
                    return;
            }
        }
    }

    private static void DisplayBanner()
    {
        AnsiConsole.Write(
            new FigletText("Interju Gyakorlo")
                .Color(Color.DodgerBlue2)
                .Centered());

        AnsiConsole.Write(
            new Rule("[grey]Allasinterju Kerdesbank es Gyakorlo Rendszer[/]")
                .RuleStyle("dodgerblue2")
                .Centered());
    }

    // =============================================
    //  Gyakorlo mod
    // =============================================

    private async Task PracticeModeAsync()
    {
        var mode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Valassz gyakorlo modot:[/]")
                .HighlightStyle(new Style(Color.DodgerBlue2))
                .AddChoices(
                    "Veletlen mod",
                    "Adaptiv mod (gyengesegek alapjan)",
                    "<< Vissza"));

        if (mode.StartsWith("<<")) return;
        bool adaptive = mode.StartsWith("Adaptiv");

        AnsiConsole.MarkupLine(adaptive
            ? "\n[bold mediumpurple]Adaptiv mod - a gyenge pontjaid alapjan valasztok kerdeseket[/]\n"
            : "\n[bold dodgerblue2]Veletlen mod - barmi johet![/]\n");

        int questionCount = 0;
        int correctCount = 0;

        while (true)
        {
            var question = adaptive
                ? await _practiceService.GetAdaptiveQuestionAsync()
                : await _practiceService.GetRandomQuestionAsync();

            if (question == null)
            {
                AnsiConsole.MarkupLine("[red]Nincs elerheto kerdes! Adj hozza kerdeseket eloszor.[/]");
                return;
            }

            questionCount++;

            // Kerdes megjelenitese
            var diffColor = question.Difficulty switch
            {
                Difficulty.Easy => "green",
                Difficulty.Medium => "yellow",
                Difficulty.Hard => "red",
                _ => "white"
            };

            var diffText = question.Difficulty switch
            {
                Difficulty.Easy => "Konnyu",
                Difficulty.Medium => "Kozepes",
                Difficulty.Hard => "Nehez",
                _ => "?"
            };

            var panel = new Panel(
                new Markup($"[bold white]{Markup.Escape(question.Text)}[/]"))
            {
                Header = new PanelHeader(
                    $"[bold blue] #{question.Id} [/]| {Markup.Escape(question.Topic.Name)} | [{diffColor}]{diffText}[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.DodgerBlue2),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(panel);

            // Gondolkodas
            AnsiConsole.MarkupLine("\n[grey italic]Gondolkodj a valaszon, majd nyomj [bold]ENTER[/]-t a valasz megjelenitsehez...[/]");
            Console.ReadLine();

            // Valasz megjelenitese
            var answerPanel = new Panel(
                new Markup($"[bold green]{Markup.Escape(question.Answer)}[/]"))
            {
                Header = new PanelHeader("[bold green] Valasz [/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(answerPanel);
            AnsiConsole.WriteLine();

            // Onertekeles
            var correct = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Tudtad a helyes valaszt?[/]")
                    .HighlightStyle(new Style(Color.DodgerBlue2))
                    .AddChoices("Igen, tudtam", "Nem tudtam"));

            bool isCorrect = correct.StartsWith("Igen");
            if (isCorrect) correctCount++;

            await _practiceService.RecordAttemptAsync(question.Id, isCorrect);

            if (isCorrect)
                AnsiConsole.MarkupLine("[bold green]Szep munka![/]");
            else
                AnsiConsole.MarkupLine("[bold yellow]Ne csugged, gyakorolj tovabb![/]");

            // Ertekeles
            var rating = AnsiConsole.Prompt(
                new TextPrompt<int>("[grey]Ertekeld a kerdes nehezsget/minoseget (1-5):[/]")
                    .DefaultValue(3)
                    .Validate(r => r is >= 1 and <= 5
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]1 es 5 kozotti erteket adj meg![/]")));

            await _practiceService.RecordRatingAsync(question.Id, rating);

            // Konyvjelzo
            if (AnsiConsole.Confirm("[grey]Hozzaadod a konyvjelzokhz?[/]", false))
            {
                await _bookmarkService.AddBookmarkAsync(question.Id);
                AnsiConsole.MarkupLine("[green]Konyvjelzo mentve![/]");
            }

            AnsiConsole.Write(new Rule().RuleStyle("grey"));

            // Folytatas
            if (!AnsiConsole.Confirm("[bold]Kovetkezo kerdes?[/]", true))
                break;

            AnsiConsole.WriteLine();
        }

        // Osszesites
        AnsiConsole.WriteLine();
        var summaryPanel = new Panel(
            new Markup(
                $"[bold]Kerdesek szama:[/] {questionCount}\n" +
                $"[bold]Helyes valaszok:[/] [green]{correctCount}[/]\n" +
                $"[bold]Pontossag:[/] {(questionCount > 0 ? Math.Round((double)correctCount / questionCount * 100, 1) : 0)}%"))
        {
            Header = new PanelHeader("[bold dodgerblue2] Gyakorlas osszegzese [/]"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.DodgerBlue2),
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(summaryPanel);
    }

    // =============================================
    //  Kerdes hozzaadasa
    // =============================================

    private async Task AddQuestionAsync()
    {
        AnsiConsole.Write(new Rule("[bold dodgerblue2] Uj kerdes hozzaadasa [/]").RuleStyle("dodgerblue2"));

        // Temakor kivalasztasa
        var topics = await _questionService.GetAllTopicsAsync();
        var topicChoices = topics.Select(t => t.Name).ToList();
        topicChoices.Add("[+] Uj temakor letrehozasa");

        var topicChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Valassz temakort:[/]")
                .HighlightStyle(new Style(Color.DodgerBlue2))
                .AddChoices(topicChoices));

        Topic topic;
        if (topicChoice == "[+] Uj temakor letrehozasa")
        {
            var topicName = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Uj temakor neve:[/]")
                    .Validate(n => !string.IsNullOrWhiteSpace(n)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]A nev nem lehet ures![/]")));

            topic = await _questionService.AddTopicAsync(topicName);
            AnsiConsole.MarkupLine($"[green]Temakor letrehozva: {Markup.Escape(topic.Name)}[/]");
        }
        else
        {
            topic = topics.First(t => t.Name == topicChoice);
        }

        // Nehezseg
        var diffChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Nehezsegi szint:[/]")
                .HighlightStyle(new Style(Color.DodgerBlue2))
                .AddChoices(
                    "Konnyu (Easy)",
                    "Kozepes (Medium)",
                    "Nehez (Hard)"));

        var difficulty = diffChoice switch
        {
            "Konnyu (Easy)" => Difficulty.Easy,
            "Kozepes (Medium)" => Difficulty.Medium,
            "Nehez (Hard)" => Difficulty.Hard,
            _ => Difficulty.Medium
        };

        // Kerdes szovege
        var text = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Kerdes szovege:[/]")
                .Validate(t => !string.IsNullOrWhiteSpace(t) && t.Length >= 10
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]A kerdes legalabb 10 karakter legyen![/]")));

        // Valasz szovege
        var answer = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Valasz szovege:[/]")
                .Validate(a => !string.IsNullOrWhiteSpace(a) && a.Length >= 5
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]A valasz legalabb 5 karakter legyen![/]")));

        var question = await _questionService.AddQuestionAsync(text, answer, topic.Id, difficulty);

        AnsiConsole.MarkupLine($"\n[bold green]Kerdes sikeresen hozzaadva! (ID: {question.Id})[/]");

        // Meg egyet?
        if (AnsiConsole.Confirm("[grey]Szeretnel meg egy kerdest hozzaadni?[/]", false))
        {
            AnsiConsole.Clear();
            DisplayBanner();
            await AddQuestionAsync();
        }
    }

    // =============================================
    //  Statisztikak
    // =============================================

    private async Task ShowStatisticsMenuAsync()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold dodgerblue2]=== Statisztikak ===[/]")
                    .HighlightStyle(new Style(Color.DodgerBlue2))
                    .AddChoices(
                        "Temakori teljesitmeny",
                        "Leggyengebb temak (TOP 5)",
                        "Legrosszabb kerdesek (TOP 5)",
                        "Legjobb kerdesek (TOP 5)",
                        "Osszes kerdes statisztikaja",
                        "<< Vissza a fomenube"));

            if (choice.StartsWith("<<")) return;

            AnsiConsole.WriteLine();

            switch (choice)
            {
                case "Temakori teljesitmeny":
                    await ShowTopicPerformanceAsync();
                    break;
                case "Leggyengebb temak (TOP 5)":
                    await ShowWeakestTopicsAsync();
                    break;
                case "Legrosszabb kerdesek (TOP 5)":
                    await ShowMostMissedAsync();
                    break;
                case "Legjobb kerdesek (TOP 5)":
                    await ShowBestQuestionsAsync();
                    break;
                case "Osszes kerdes statisztikaja":
                    await ShowAllQuestionStatsAsync();
                    break;
            }

            AnsiConsole.WriteLine();
        }
    }

    private async Task ShowTopicPerformanceAsync()
    {
        var stats = await _statisticsService.GetTopicStatsAsync();

        if (stats.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Meg nincsenek statisztikak. Kezdj el gyakorolni![/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.DodgerBlue2))
            .Title("[bold dodgerblue2]Temakoronkenti teljesitmeny[/]")
            .AddColumn(new TableColumn("[bold]Temakor[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Probalkozasok[/]").Centered())
            .AddColumn(new TableColumn("[bold]Helyes[/]").Centered())
            .AddColumn(new TableColumn("[bold]Pontossag[/]").Centered())
            .AddColumn(new TableColumn("[bold]Szint[/]").Centered());

        foreach (var stat in stats)
        {
            var (color, bar) = GetPerformanceIndicator(stat.AccuracyPercentage, stat.TotalAttempts);
            table.AddRow(
                $"[bold]{Markup.Escape(stat.TopicName)}[/]",
                stat.TotalAttempts.ToString(),
                stat.CorrectAttempts.ToString(),
                $"[{color}]{stat.AccuracyPercentage}%[/]",
                bar);
        }

        AnsiConsole.Write(table);
    }

    private async Task ShowWeakestTopicsAsync()
    {
        var stats = await _statisticsService.GetWeakestTopicsAsync(5);
        if (stats.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Meg nincsenek statisztikak.[/]");
            return;
        }

        AnsiConsole.Write(new Rule("[bold red]Leggyengebb temak[/]").RuleStyle("red"));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.Red))
            .AddColumn("[bold]#[/]")
            .AddColumn("[bold]Temakor[/]")
            .AddColumn("[bold]Pontossag[/]")
            .AddColumn("[bold]Probalkozasok[/]");

        int rank = 1;
        foreach (var stat in stats)
        {
            table.AddRow(
                $"[red]{rank++}.[/]",
                Markup.Escape(stat.TopicName),
                $"[red]{stat.AccuracyPercentage}%[/]",
                stat.TotalAttempts.ToString());
        }

        AnsiConsole.Write(table);
    }

    private async Task ShowMostMissedAsync()
    {
        var stats = await _statisticsService.GetMostMissedQuestionsAsync(5);
        if (stats.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Meg nincsenek statisztikak.[/]");
            return;
        }

        AnsiConsole.Write(new Rule("[bold red]Leggyakrabban elhibazott kerdesek[/]").RuleStyle("red"));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.Red))
            .AddColumn("[bold]#[/]")
            .AddColumn(new TableColumn("[bold]Kerdes[/]").Width(50))
            .AddColumn("[bold]Tema[/]")
            .AddColumn("[bold]Pontossag[/]");

        int rank = 1;
        foreach (var stat in stats)
        {
            var shortText = stat.QuestionText.Length > 60
                ? stat.QuestionText[..57] + "..."
                : stat.QuestionText;
            table.AddRow(
                $"[red]{rank++}.[/]",
                Markup.Escape(shortText),
                Markup.Escape(stat.TopicName),
                $"[red]{stat.AccuracyPercentage}%[/]");
        }

        AnsiConsole.Write(table);
    }

    private async Task ShowBestQuestionsAsync()
    {
        var stats = await _statisticsService.GetBestQuestionsAsync(5);
        if (stats.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Meg nincsenek statisztikak.[/]");
            return;
        }

        AnsiConsole.Write(new Rule("[bold green]Legjobban teljesitett kerdesek[/]").RuleStyle("green"));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.Green))
            .AddColumn("[bold]#[/]")
            .AddColumn(new TableColumn("[bold]Kerdes[/]").Width(50))
            .AddColumn("[bold]Tema[/]")
            .AddColumn("[bold]Pontossag[/]");

        int rank = 1;
        foreach (var stat in stats)
        {
            var shortText = stat.QuestionText.Length > 60
                ? stat.QuestionText[..57] + "..."
                : stat.QuestionText;
            table.AddRow(
                $"[green]{rank++}.[/]",
                Markup.Escape(shortText),
                Markup.Escape(stat.TopicName),
                $"[green]{stat.AccuracyPercentage}%[/]");
        }

        AnsiConsole.Write(table);
    }

    private async Task ShowAllQuestionStatsAsync()
    {
        var stats = await _statisticsService.GetQuestionStatsAsync();
        if (stats.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nincsenek kerdesek az adatbazisban.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.DodgerBlue2))
            .Title("[bold dodgerblue2]Osszes kerdes statisztikaja[/]")
            .AddColumn("[bold]ID[/]")
            .AddColumn(new TableColumn("[bold]Kerdes[/]").Width(40))
            .AddColumn("[bold]Tema[/]")
            .AddColumn("[bold]Nehezseg[/]")
            .AddColumn("[bold]Proba[/]")
            .AddColumn("[bold]Pontossag[/]")
            .AddColumn("[bold]Atl.Rating[/]");

        foreach (var stat in stats)
        {
            var diffColor = stat.Difficulty switch
            {
                "Easy" => "green",
                "Medium" => "yellow",
                "Hard" => "red",
                _ => "white"
            };
            var accColor = stat.TotalAttempts == 0 ? "grey"
                : stat.AccuracyPercentage >= 70 ? "green"
                : stat.AccuracyPercentage >= 40 ? "yellow"
                : "red";

            var shortText = stat.QuestionText.Length > 45
                ? stat.QuestionText[..42] + "..."
                : stat.QuestionText;

            table.AddRow(
                stat.QuestionId.ToString(),
                Markup.Escape(shortText),
                Markup.Escape(stat.TopicName),
                $"[{diffColor}]{stat.Difficulty}[/]",
                stat.TotalAttempts.ToString(),
                stat.TotalAttempts > 0 ? $"[{accColor}]{stat.AccuracyPercentage}%[/]" : "[grey]--[/]",
                stat.AverageRating > 0 ? $"{stat.AverageRating:F1}" : "[grey]--[/]");
        }

        AnsiConsole.Write(table);
    }

    private static (string color, string bar) GetPerformanceIndicator(double accuracy, int attempts)
    {
        if (attempts == 0) return ("grey", "[grey].....[/]");

        return accuracy switch
        {
            >= 80 => ("green", "[green]#####[/]"),
            >= 60 => ("dodgerblue2", "[dodgerblue2]####.[/]"),
            >= 40 => ("yellow", "[yellow]###..[/]"),
            >= 20 => ("darkorange", "[darkorange]##...[/]"),
            _ => ("red", "[red]#....[/]"),
        };
    }

    // =============================================
    //  Konyvjelzok
    // =============================================

    private async Task ShowBookmarksAsync()
    {
        var bookmarks = await _bookmarkService.GetAllBookmarksAsync();

        if (bookmarks.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nincsenek konyvjelzoid. A gyakorlas kozben tudsz hozzaadni![/]");
            return;
        }

        AnsiConsole.Write(new Rule("[bold dodgerblue2] Konyvjelzok [/]").RuleStyle("dodgerblue2"));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.DodgerBlue2))
            .AddColumn("[bold]ID[/]")
            .AddColumn(new TableColumn("[bold]Kerdes[/]").Width(50))
            .AddColumn("[bold]Tema[/]")
            .AddColumn("[bold]Mentve[/]");

        foreach (var bm in bookmarks)
        {
            table.AddRow(
                bm.Question.Id.ToString(),
                Markup.Escape(bm.Question.Text),
                Markup.Escape(bm.Question.Topic.Name),
                bm.CreatedAt.ToLocalTime().ToString("yyyy.MM.dd HH:mm"));
        }

        AnsiConsole.Write(table);

        // Torles lehetosege
        if (AnsiConsole.Confirm("\n[grey]Szeretnel konyvjelzot torolni?[/]", false))
        {
            var qId = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold]Add meg a kerdes ID-jat:[/]"));

            if (await _bookmarkService.RemoveBookmarkAsync(qId))
                AnsiConsole.MarkupLine("[green]Konyvjelzo torolve![/]");
            else
                AnsiConsole.MarkupLine("[red]Nem talalhato konyvjelzo ezzel az ID-val.[/]");
        }
    }

    // =============================================
    //  Import / Export
    // =============================================

    private async Task ImportExportMenuAsync()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold dodgerblue2]=== Import / Export ===[/]")
                .HighlightStyle(new Style(Color.DodgerBlue2))
                .AddChoices(
                    "Kerdesek exportalasa (JSON)",
                    "Kerdesek importalasa (JSON)",
                    "<< Vissza"));

        if (choice.StartsWith("<<")) return;

        if (choice.StartsWith("Kerdesek exportalasa"))
        {
            var filePath = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Exportalasi utvonal (fajlnev):[/]")
                    .DefaultValue("questions_export.json"));

            // Ha mappat adott meg, fajlnevet fuzunk hozza
            if (Directory.Exists(filePath))
                filePath = Path.Combine(filePath, "questions_export.json");

            try
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(new Style(Color.DodgerBlue2))
                    .StartAsync("Exportalas...", async _ =>
                    {
                        await _importExportService.ExportQuestionsAsync(filePath);
                    });

                AnsiConsole.MarkupLine($"[bold green]Kerdesek exportalva: {Markup.Escape(Path.GetFullPath(filePath))}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Exportalasi hiba: {Markup.Escape(ex.Message)}[/]");
            }
        }
        else
        {
            var filePath = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Import fajl utvonala:[/]")
                    .Validate(p => !string.IsNullOrWhiteSpace(p)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Add meg az utvonalat![/]")));

            var (imported, skipped, errors) = (0, 0, new List<string>());

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(new Style(Color.DodgerBlue2))
                .StartAsync("Importalas...", async _ =>
                {
                    (imported, skipped, errors) = await _importExportService.ImportQuestionsAsync(filePath);
                });

            AnsiConsole.MarkupLine($"[green]Importalva: {imported}[/]  [yellow]Kihagyva: {skipped}[/]");

            if (errors.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[red bold]Hibak:[/]");
                foreach (var error in errors)
                    AnsiConsole.MarkupLine($"  [red]- {Markup.Escape(error)}[/]");
            }
        }
    }

    // =============================================
    //  Napi csomag
    // =============================================

    private async Task DailyPackageAsync()
    {
        AnsiConsole.Write(new Rule("[bold dodgerblue2] Napi Gyakorlo Csomag [/]").RuleStyle("dodgerblue2"));

        List<Question> package = null!;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(new Style(Color.DodgerBlue2))
            .StartAsync("Napi csomag generalasa...", async _ =>
            {
                package = await _dailyPackageService.GenerateDailyPackageAsync();
            });

        if (package.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nincs eleg kerdes a napi csomaghoz.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[bold green]{package.Count} kerdes generalva![/]\n");
        AnsiConsole.MarkupLine("[grey]50% gyenge temak - 20% uj kerdesek - 30% vegyes[/]\n");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Color.DodgerBlue2))
            .AddColumn("[bold]#[/]")
            .AddColumn(new TableColumn("[bold]Kerdes[/]").Width(50))
            .AddColumn("[bold]Tema[/]")
            .AddColumn("[bold]Nehezseg[/]");

        int idx = 1;
        foreach (var q in package)
        {
            var diffColor = q.Difficulty switch
            {
                Difficulty.Easy => "green",
                Difficulty.Medium => "yellow",
                Difficulty.Hard => "red",
                _ => "white"
            };
            table.AddRow(
                $"{idx++}.",
                Markup.Escape(q.Text),
                Markup.Escape(q.Topic.Name),
                $"[{diffColor}]{q.Difficulty}[/]");
        }

        AnsiConsole.Write(table);

        // Gyakorlas inditasa a csomagbol
        if (AnsiConsole.Confirm("\n[bold]Elkezded gyakorolni a napi csomagot?[/]", true))
        {
            await PracticeFromPackageAsync(package);
        }
    }

    private async Task PracticeFromPackageAsync(List<Question> package)
    {
        int correct = 0;
        int total = package.Count;

        for (int i = 0; i < total; i++)
        {
            var question = package[i];

            AnsiConsole.Write(new Rule($"[grey]Kerdes {i + 1}/{total}[/]").RuleStyle("grey"));

            var diffColor = question.Difficulty switch
            {
                Difficulty.Easy => "green",
                Difficulty.Medium => "yellow",
                Difficulty.Hard => "red",
                _ => "white"
            };

            var panel = new Panel(
                new Markup($"[bold white]{Markup.Escape(question.Text)}[/]"))
            {
                Header = new PanelHeader(
                    $"[bold blue] #{question.Id} [/]| {Markup.Escape(question.Topic.Name)} | [{diffColor}]{question.Difficulty}[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.DodgerBlue2),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(panel);

            AnsiConsole.MarkupLine("\n[grey italic]Gondolkodj, majd nyomj ENTER-t a valasz megjelenitsehez...[/]");
            Console.ReadLine();

            // Valasz megjelenitese
            var answerPanel = new Panel(
                new Markup($"[bold green]{Markup.Escape(question.Answer)}[/]"))
            {
                Header = new PanelHeader("[bold green] Valasz [/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(2, 1)
            };
            AnsiConsole.Write(answerPanel);
            AnsiConsole.WriteLine();

            var answer = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Tudtad a valaszt?[/]")
                    .HighlightStyle(new Style(Color.DodgerBlue2))
                    .AddChoices("Igen", "Nem"));

            bool isCorrect = answer == "Igen";
            if (isCorrect) correct++;

            await _practiceService.RecordAttemptAsync(question.Id, isCorrect);

            var rating = AnsiConsole.Prompt(
                new TextPrompt<int>("[grey]Ertekeles (1-5):[/]")
                    .DefaultValue(3)
                    .Validate(r => r is >= 1 and <= 5
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]1-5![/]")));

            await _practiceService.RecordRatingAsync(question.Id, rating);
            AnsiConsole.WriteLine();
        }

        // Osszesites
        var summaryPanel = new Panel(
            new Markup(
                $"[bold]Kerdesek:[/] {total}\n" +
                $"[bold]Helyes:[/] [green]{correct}[/] / {total}\n" +
                $"[bold]Pontossag:[/] {(total > 0 ? Math.Round((double)correct / total * 100, 1) : 0)}%"))
        {
            Header = new PanelHeader("[bold dodgerblue2] Napi csomag eredmenye [/]"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.DodgerBlue2),
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(summaryPanel);
    }

    // =============================================
    //  Heti osszefoglalo
    // =============================================

    private async Task WeeklySummaryMenuAsync()
    {
        AnsiConsole.Write(new Rule("[bold dodgerblue2] Heti Osszefoglalo [/]").RuleStyle("dodgerblue2"));

        WeeklySummaryDto? summary = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(new Style(Color.DodgerBlue2))
            .StartAsync("Osszefoglalo generalasa...", async _ =>
            {
                summary = await _weeklySummaryService.GenerateWeeklySummaryAsync();
            });

        if (summary == null) return;

        // Megjelnites
        var accColor = summary.AccuracyPercentage >= 70 ? "green"
            : summary.AccuracyPercentage >= 40 ? "yellow"
            : "red";

        var panel = new Panel(new Markup(
            $"[bold]Idoszak:[/]              {summary.WeekStart:yyyy.MM.dd} - {summary.WeekEnd:yyyy.MM.dd}\n" +
            $"[bold]Osszes probalkozas:[/]   {summary.TotalAttempts}\n" +
            $"[bold]Helyes valaszok:[/]      [green]{summary.CorrectAnswers}[/]\n" +
            $"[bold]Pontossag:[/]            [{accColor}]{summary.AccuracyPercentage}%[/]\n" +
            $"[bold]Egyedi kerdesek:[/]      {summary.UniqueQuestionsAttempted}\n" +
            $"[bold]Leggyengebb tema:[/]     [red]{Markup.Escape(summary.WeakestTopic)}[/]\n" +
            $"[bold]Legerosebb tema:[/]      [green]{Markup.Escape(summary.BestTopic)}[/]"))
        {
            Header = new PanelHeader("[bold dodgerblue2] Az elmult 7 nap [/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.DodgerBlue2),
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(panel);

        // Temakor tablazat
        if (summary.TopicBreakdown.Count > 0)
        {
            AnsiConsole.WriteLine();
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Color.Grey))
                .Title("[grey]Temakor bontas[/]")
                .AddColumn("[bold]Tema[/]")
                .AddColumn("[bold]Proba[/]")
                .AddColumn("[bold]Helyes[/]")
                .AddColumn("[bold]%[/]");

            foreach (var t in summary.TopicBreakdown.Where(t => t.TotalAttempts > 0))
            {
                var tc = t.AccuracyPercentage >= 70 ? "green" : t.AccuracyPercentage >= 40 ? "yellow" : "red";
                table.AddRow(
                    Markup.Escape(t.TopicName),
                    t.TotalAttempts.ToString(),
                    t.CorrectAttempts.ToString(),
                    $"[{tc}]{t.AccuracyPercentage}%[/]");
            }

            AnsiConsole.Write(table);
        }

        // Email kuldes
        AnsiConsole.WriteLine();
        if (AnsiConsole.Confirm("[grey]Szeretned elkuldeni emailben?[/]", false))
        {
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Email cim:[/]")
                    .Validate(e => e.Contains('@')
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Ervenytelen email cim![/]")));

            try
            {
                var sent = await _weeklySummaryService.SendWeeklyEmailAsync(email);
                if (sent)
                    AnsiConsole.MarkupLine("[bold green]Email sikeresen elkuldve![/]");
                else
                    AnsiConsole.MarkupLine("[yellow]Az SMTP nincs konfiguraslva. Allitsd be az appsettings.json-ben![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Email kuldesi hiba: {Markup.Escape(ex.Message)}[/]");
            }
        }
    }
}
