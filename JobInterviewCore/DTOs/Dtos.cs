namespace JobInterviewCore.DTOs;

public class QuestionExportDto
{
    public string Text { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

public class ImportData
{
    public List<QuestionExportDto> Questions { get; set; } = new();
}

public class TopicStatDto
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double AccuracyPercentage => TotalAttempts > 0
        ? Math.Round((double)CorrectAttempts / TotalAttempts * 100, 1)
        : 0;
}

public class QuestionStatDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public double AccuracyPercentage => TotalAttempts > 0
        ? Math.Round((double)CorrectAttempts / TotalAttempts * 100, 1)
        : 0;
    public double AverageRating { get; set; }
}

public class WeeklySummaryDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int TotalAttempts { get; set; }
    public int CorrectAnswers { get; set; }
    public double AccuracyPercentage => TotalAttempts > 0
        ? Math.Round((double)CorrectAnswers / TotalAttempts * 100, 1)
        : 0;
    public int UniqueQuestionsAttempted { get; set; }
    public string WeakestTopic { get; set; } = "N/A";
    public string BestTopic { get; set; } = "N/A";
    public List<TopicStatDto> TopicBreakdown { get; set; } = new();
}
