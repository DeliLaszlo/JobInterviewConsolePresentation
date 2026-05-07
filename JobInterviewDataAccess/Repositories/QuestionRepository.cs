using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobInterviewDataAccess.Repositories;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(AppDbContext context) : base(context) { }

    public async Task<List<Question>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(q => q.Topic)
            .Include(q => q.Attempts)
            .Include(q => q.Ratings)
            .Include(q => q.Bookmarks)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<int> GetTotalQuestionCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<Question?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(q => q.Topic)
            .Include(q => q.Attempts)
            .Include(q => q.Ratings)
            .Include(q => q.Bookmarks)
            .AsSplitQuery()
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Question>> GetByTopicAsync(int topicId)
    {
        return await _dbSet
            .Include(q => q.Topic)
            .Include(q => q.Attempts)
            .Where(q => q.TopicId == topicId)
            .ToListAsync();
    }
}
