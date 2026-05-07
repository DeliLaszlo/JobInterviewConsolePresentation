using JobInterviewCore.DTOs;
using JobInterviewCore.Entities;
using JobInterviewCore.Enums;

namespace JobInterviewCore.Interfaces;

public interface IPracticeService
{
    Task<Question?> GetRandomQuestionAsync();
    Task<Question?> GetAdaptiveQuestionAsync();
    Task RecordAttemptAsync(int questionId, bool correct);
    Task RecordRatingAsync(int questionId, int score);
}

public interface IStatisticsService
{
    Task<List<QuestionStatDto>> GetQuestionStatsAsync();
    Task<List<TopicStatDto>> GetTopicStatsAsync();
    Task<List<TopicStatDto>> GetWeakestTopicsAsync(int count = 5);
    Task<List<QuestionStatDto>> GetMostMissedQuestionsAsync(int count = 5);
    Task<List<QuestionStatDto>> GetBestQuestionsAsync(int count = 5);
}

public interface IDailyPackageService
{
    Task<List<Question>> GenerateDailyPackageAsync(int totalCount = 10);
}

public interface IImportExportService
{
    Task ExportQuestionsAsync(string filePath);
    Task<(int imported, int skipped, List<string> errors)> ImportQuestionsAsync(string filePath);
}

public interface IWeeklySummaryService
{
    Task<WeeklySummaryDto> GenerateWeeklySummaryAsync();
    Task<bool> SendWeeklyEmailAsync(string recipientEmail);
}

public interface IQuestionService
{
    Task<Question> AddQuestionAsync(string text, string answer, int topicId, Difficulty difficulty);
    Task<List<Question>> GetAllQuestionsAsync();
    Task<List<Topic>> GetAllTopicsAsync();
    Task<Topic> AddTopicAsync(string name);
    Task<int> GetTotalQuestionCountAsync();
    Task<bool> DeleteQuestionAsync(int id);
}

public interface IBookmarkService
{
    Task<Bookmark> AddBookmarkAsync(int questionId);
    Task<bool> RemoveBookmarkAsync(int questionId);
    Task<List<Bookmark>> GetAllBookmarksAsync();
    Task<bool> IsBookmarkedAsync(int questionId);
}
