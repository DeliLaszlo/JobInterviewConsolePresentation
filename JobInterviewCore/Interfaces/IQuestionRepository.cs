using JobInterviewCore.Entities;

namespace JobInterviewCore.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<List<Question>> GetAllWithDetailsAsync();
    Task<Question?> GetWithDetailsAsync(int id);
    Task<List<Question>> GetByTopicAsync(int topicId);
}
