using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class PracticeService : IPracticeService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IRepository<Attempt> _attemptRepo;
    private readonly IRepository<Rating> _ratingRepo;
    private static readonly Random _random = new();

    public PracticeService(
        IQuestionRepository questionRepo,
        IRepository<Attempt> attemptRepo,
        IRepository<Rating> ratingRepo)
    {
        _questionRepo = questionRepo;
        _attemptRepo = attemptRepo;
        _ratingRepo = ratingRepo;
    }

    /// <summary>
    /// Véletlenszerű kérdés kiválasztása.
    /// </summary>
    public async Task<Question?> GetRandomQuestionAsync()
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();
        if (questions.Count == 0) return null;
        return questions[_random.Next(questions.Count)];
    }

    /// <summary>
    /// Adaptív, súlyozott kérdés kiválasztás.
    /// Súly képlet:
    ///   weight = base(1)
    ///          + (rossz válaszok száma × 2)
    ///          + (régen volt az utolsó próbálkozás → +1..+3)
    ///          + (alacsony átlag rating → +2)
    ///          + (soha nem próbált → +3)
    /// </summary>
    public async Task<Question?> GetAdaptiveQuestionAsync()
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();
        if (questions.Count == 0) return null;

        var weights = new List<(Question question, double weight)>();

        foreach (var q in questions)
        {
            double weight = 1.0;

            var attempts = q.Attempts.ToList();
            if (attempts.Count > 0)
            {
                // Rossz válaszok súlyozása
                int incorrectCount = attempts.Count(a => !a.Correct);
                weight += incorrectCount * 2;

                // Régiség: minél régebben volt kérdezve, annál nagyobb súly
                var lastAttempt = attempts.Max(a => a.AttemptedAt);
                var daysSince = (DateTime.UtcNow - lastAttempt).TotalDays;
                if (daysSince > 30) weight += 3;
                else if (daysSince > 14) weight += 2;
                else if (daysSince > 7) weight += 1;

                // Kevés helyes arány → extra súly
                double accuracy = (double)attempts.Count(a => a.Correct) / attempts.Count;
                if (accuracy < 0.3) weight += 3;
                else if (accuracy < 0.5) weight += 2;
            }
            else
            {
                // Soha nem próbált kérdés – prioritás
                weight += 3;
            }

            // Alacsony átlag rating → extra súly
            var ratings = q.Ratings.ToList();
            if (ratings.Count > 0)
            {
                double avgRating = ratings.Average(r => r.Score);
                if (avgRating < 2.5) weight += 3;
                else if (avgRating < 3.5) weight += 1;
            }

            weights.Add((q, weight));
        }

        // Súlyozott véletlenszerű kiválasztás (weighted random selection)
        double totalWeight = weights.Sum(w => w.weight);
        double randomValue = _random.NextDouble() * totalWeight;
        double cumulative = 0;

        foreach (var (question, weight) in weights)
        {
            cumulative += weight;
            if (randomValue <= cumulative)
                return question;
        }

        return weights.Last().question;
    }

    /// <summary>
    /// Próbálkozás rögzítése.
    /// </summary>
    public async Task RecordAttemptAsync(int questionId, bool correct)
    {
        var attempt = new Attempt
        {
            QuestionId = questionId,
            Correct = correct,
            AttemptedAt = DateTime.UtcNow
        };
        await _attemptRepo.AddAsync(attempt);
    }

    /// <summary>
    /// Kérdés értékelésének rögzítése (1–5).
    /// </summary>
    public async Task RecordRatingAsync(int questionId, int score)
    {
        var rating = new Rating
        {
            QuestionId = questionId,
            Score = Math.Clamp(score, 1, 5),
            CreatedAt = DateTime.UtcNow
        };
        await _ratingRepo.AddAsync(rating);
    }
}
