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

    // Új kérdés hozzáadása.
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

    // Összes kérdés lekérdezése részletekkel.
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        return await _questionRepo.GetAllWithDetailsAsync();
    }

    // Kérdések darabszámának lekérdezése.
    public async Task<int> GetTotalQuestionCountAsync()
    {
        return await _questionRepo.GetTotalQuestionCountAsync();
    }

    // Összes témakör lekérdezése.
    public async Task<List<Topic>> GetAllTopicsAsync()
    {
        return await _topicRepo.GetAllAsync();
    }

    // Új témakör hozzáadása.
    public async Task<Topic> AddTopicAsync(string name)
    {
        var topic = new Topic { Name = name };
        await _topicRepo.AddAsync(topic);
        return topic;
    }

    // Kérdés törlése azonosító alapján.
    public async Task<bool> DeleteQuestionAsync(int id)
    {
        var question = await _questionRepo.GetByIdAsync(id);
        if (question == null) return false;

        await _questionRepo.DeleteAsync(question);
        return true;
    }

    // Témakör törlése azonosító alapján (és a hozzá tartozó kérdések törlése is).
    public async Task<bool> DeleteTopicAsync(int id)
    {
        var topic = await _topicRepo.GetByIdAsync(id);
        if (topic == null) return false;

        var questions = await _questionRepo.GetByTopicAsync(id);
        foreach (var q in questions)
        {
            await _questionRepo.DeleteAsync(q);
        }

        await _topicRepo.DeleteAsync(topic);
        return true;
    }
}
