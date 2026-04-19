using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobInterviewDataAccess.Repositories;

public class BookmarkRepository : Repository<Bookmark>, IBookmarkRepository
{
    public BookmarkRepository(AppDbContext context) : base(context) { }

    public async Task<List<Bookmark>> GetAllWithQuestionsAsync()
    {
        return await _dbSet
            .Include(b => b.Question)
                .ThenInclude(q => q.Topic)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
}
