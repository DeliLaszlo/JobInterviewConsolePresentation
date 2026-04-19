using JobInterviewCore.Entities;

namespace JobInterviewCore.Interfaces;

public interface IBookmarkRepository : IRepository<Bookmark>
{
    Task<List<Bookmark>> GetAllWithQuestionsAsync();
}
