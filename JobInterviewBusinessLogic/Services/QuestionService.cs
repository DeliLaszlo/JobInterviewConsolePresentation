using JobInterviewCore.Entities;
using JobInterviewCore.Enums;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IRepository<Topic> _topicRepo;

    public QuestionService(
        IQuestionRepository questionRepo,
        IRepository<Topic> topicRepo)
    {
        _questionRepo = questionRepo;
        _topicRepo = topicRepo;
    }

    /// <summary>
    /// Új kérdés hozzáadása.
    /// </summary>
    public async Task<Question> AddQuestionAsync(string text, string answer, int topicId, Difficulty difficulty)
    {
        var question = new Question
        {
            Text = text,
            Answer = answer,
            TopicId = topicId,
            Difficulty = difficulty,
            CreatedAt = DateTime.UtcNow
        };

        await _questionRepo.AddAsync(question);
        return question;
    }

    /// <summary>
    /// Összes kérdés lekérdezése részletekkel.
    /// </summary>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        return await _questionRepo.GetAllWithDetailsAsync();
    }

    /// <summary>
    /// Összes témakör lekérdezése.
    /// </summary>
    public async Task<List<Topic>> GetAllTopicsAsync()
    {
        return await _topicRepo.GetAllAsync();
    }

    /// <summary>
    /// Új témakör hozzáadása.
    /// </summary>
    public async Task<Topic> AddTopicAsync(string name)
    {
        var topic = new Topic { Name = name };
        await _topicRepo.AddAsync(topic);
        return topic;
    }

    /// <summary>
    /// Kérdés törlése azonosító alapján.
    /// </summary>
    public async Task<bool> DeleteQuestionAsync(int id)
    {
        var question = await _questionRepo.GetByIdAsync(id);
        if (question == null) return false;

        await _questionRepo.DeleteAsync(question);
        return true;
    }
}
