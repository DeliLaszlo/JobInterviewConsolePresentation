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

    // Véletlenszerű kérdés kiválasztása, kivéve tabu listán szereplő kérdések
    public async Task<Question?> GetRandomQuestionAsync(IEnumerable<int> tabooIds)
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();
        var available = questions.Where(q => !tabooIds.Contains(q.Id)).ToList();
        if (available.Count == 0) return null;
        return available[_random.Next(available.Count)];
    }

    // Adaptív, súlyozott kérdés kiválasztás, kivéve tabu listán szereplő kérdések
    // Súly képlet:
    //   weight = base(1)
    //          + (utolsó 3 próbálkozásból rossz válaszok száma)
    //          + (régen volt az utolsó próbálkozás: +1..+3)
    //          + (kevés helyes arány: +2..+3)
    //          + (alacsony átlag rating: +1..+3)
    //          + (soha nem próbált: +5)
    public async Task<Question?> GetAdaptiveQuestionAsync(IEnumerable<int> tabooIds)
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();
        var available = questions.Where(q => !tabooIds.Contains(q.Id)).ToList();
        if (available.Count == 0) return null;

        var weights = new List<(Question question, double weight)>();

        foreach (var q in available)
        {
            double weight = 1.0;

            var attempts = q.Attempts.ToList();
            if (attempts.Count > 0)
            {
                // Rossz válaszok
                var recentAttempts = attempts.OrderByDescending(a => a.AttemptedAt).Take(3).ToList();
                int incorrectCount = recentAttempts.Count(a => !a.Correct);
                weight += incorrectCount * 1;

                // Régiség
                var lastAttempt = attempts.Max(a => a.AttemptedAt);
                var daysSince = (DateTime.UtcNow - lastAttempt).TotalDays;
                if (daysSince > 30) weight += 3;
                else if (daysSince > 14) weight += 2;
                else if (daysSince > 7) weight += 1;

                // Kevés helyes arány
                double accuracy = (double)attempts.Count(a => a.Correct) / attempts.Count;
                if (accuracy < 0.3) weight += 3;
                else if (accuracy < 0.5) weight += 2;
            }
            else
            {
                // Soha nem próbált kérdés
                weight += 5;
            }

            // Alacsony átlag rating
            var ratings = q.Ratings.ToList();
            if (ratings.Count > 0)
            {
                double avgRating = ratings.Average(r => r.Score);
                if (avgRating < 2.5) weight += 3;
                else if (avgRating < 3.5) weight += 1;
            }

            weights.Add((q, weight));
        }

        // Súlyozott véletlenszerű kiválasztás
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

    // Próbálkozás rögzítése.
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

    // Kérdés értékelésének rögzítése (1–5).
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
