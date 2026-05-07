using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class DailyPackageService : IDailyPackageService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IStatisticsService _statisticsService;
    private static readonly Random _random = new();

    public DailyPackageService(
        IQuestionRepository questionRepo,
        IStatisticsService statisticsService)
    {
        _questionRepo = questionRepo;
        _statisticsService = statisticsService;
    }

    // Napi gyakorló csomag generálása:
    //   50% – gyenge témák kérdései
    //   20% – új (soha nem próbált) kérdések
    //   30% – vegyes/random kérdések
    public async Task<List<Question>> GenerateDailyPackageAsync(int totalCount = 10)
    {
        var allQuestions = await _questionRepo.GetAllWithDetailsAsync();
        if (allQuestions.Count == 0) return new();
        if (allQuestions.Count <= totalCount) return Shuffle(allQuestions);

        var result = new HashSet<int>(); // kérdés Id-k a duplikáció elkerülésére
        var package = new List<Question>();

        int weakCount = (int)Math.Ceiling(totalCount * 0.5);
        int newCount = (int)Math.Ceiling(totalCount * 0.2);
        // a maradék random lesz

        // 1) Gyenge témákból
        var weakTopics = await _statisticsService.GetWeakestTopicsAsync(3);
        var weakTopicIds = weakTopics.Select(t => t.TopicId).ToHashSet();

        var weakQuestions = allQuestions
            .Where(q => weakTopicIds.Contains(q.TopicId))
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach (var q in weakQuestions)
        {
            if (result.Count >= weakCount) break;
            if (result.Add(q.Id)) package.Add(q);
        }

        // 2) Új, soha nem próbált kérdések
        var newQuestions = allQuestions
            .Where(q => !q.Attempts.Any() && !result.Contains(q.Id))
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach (var q in newQuestions)
        {
            if (package.Count >= weakCount + newCount) break;
            if (result.Add(q.Id)) package.Add(q);
        }

        // 3) Random feltöltés a maradék helyekre
        var randomPool = allQuestions
            .Where(q => !result.Contains(q.Id))
            .OrderBy(_ => _random.Next())
            .ToList();

        foreach (var q in randomPool)
        {
            if (package.Count >= totalCount) break;
            if (result.Add(q.Id)) package.Add(q);
        }

        return Shuffle(package);
    }

    // Fisher-Yates shuffle algoritmus a kérdések véletlenszerű sorrendjéhez
    private static List<Question> Shuffle(List<Question> list)
    {
        var shuffled = new List<Question>(list);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled;
    }
}
