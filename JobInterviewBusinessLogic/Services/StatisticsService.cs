using JobInterviewCore.DTOs;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IQuestionRepository _questionRepo;

    public StatisticsService(IQuestionRepository questionRepo)
    {
        _questionRepo = questionRepo;
    }

    /// <summary>
    /// Kérdésenkénti statisztikák: pontosság, átlag rating stb.
    /// </summary>
    public async Task<List<QuestionStatDto>> GetQuestionStatsAsync()
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();

        return questions
            .Select(q => new QuestionStatDto
            {
                QuestionId = q.Id,
                QuestionText = q.Text,
                TopicName = q.Topic.Name,
                Difficulty = q.Difficulty.ToString(),
                TotalAttempts = q.Attempts.Count,
                CorrectAttempts = q.Attempts.Count(a => a.Correct),
                AverageRating = q.Ratings.Any()
                    ? Math.Round(q.Ratings.Average(r => r.Score), 1)
                    : 0
            })
            .OrderBy(q => q.AccuracyPercentage)
            .ToList();
    }

    /// <summary>
    /// Témakörönkénti összesített teljesítmény.
    /// </summary>
    public async Task<List<TopicStatDto>> GetTopicStatsAsync()
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();

        return questions
            .GroupBy(q => new { q.TopicId, q.Topic.Name })
            .Select(g => new TopicStatDto
            {
                TopicId = g.Key.TopicId,
                TopicName = g.Key.Name,
                TotalAttempts = g.Sum(q => q.Attempts.Count),
                CorrectAttempts = g.Sum(q => q.Attempts.Count(a => a.Correct))
            })
            .OrderBy(t => t.AccuracyPercentage)
            .ToList();
    }

    /// <summary>
    /// Leggyengébb témakörök (TOP N, legalább 1 próbálkozás).
    /// </summary>
    public async Task<List<TopicStatDto>> GetWeakestTopicsAsync(int count = 5)
    {
        var stats = await GetTopicStatsAsync();
        return stats
            .Where(t => t.TotalAttempts > 0)
            .OrderBy(t => t.AccuracyPercentage)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Legtöbbször elhibázott kérdések.
    /// </summary>
    public async Task<List<QuestionStatDto>> GetMostMissedQuestionsAsync(int count = 5)
    {
        var stats = await GetQuestionStatsAsync();
        return stats
            .Where(q => q.TotalAttempts > 0)
            .OrderBy(q => q.AccuracyPercentage)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Legjobban teljesített kérdések.
    /// </summary>
    public async Task<List<QuestionStatDto>> GetBestQuestionsAsync(int count = 5)
    {
        var stats = await GetQuestionStatsAsync();
        return stats
            .Where(q => q.TotalAttempts > 0)
            .OrderByDescending(q => q.AccuracyPercentage)
            .Take(count)
            .ToList();
    }
}
