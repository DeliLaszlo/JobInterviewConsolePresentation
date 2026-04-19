using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _bookmarkRepo;
    private readonly IQuestionRepository _questionRepo;

    public BookmarkService(
        IBookmarkRepository bookmarkRepo,
        IQuestionRepository questionRepo)
    {
        _bookmarkRepo = bookmarkRepo;
        _questionRepo = questionRepo;
    }

    /// <summary>
    /// Könyvjelző hozzáadása (duplikáció ellenőrzéssel).
    /// </summary>
    public async Task<Bookmark> AddBookmarkAsync(int questionId)
    {
        var existing = await _bookmarkRepo.FindAsync(b => b.QuestionId == questionId);
        if (existing.Count > 0)
            return existing.First();

        var bookmark = new Bookmark
        {
            QuestionId = questionId,
            CreatedAt = DateTime.UtcNow
        };

        await _bookmarkRepo.AddAsync(bookmark);
        return bookmark;
    }

    /// <summary>
    /// Könyvjelző eltávolítása kérdés alapján.
    /// </summary>
    public async Task<bool> RemoveBookmarkAsync(int questionId)
    {
        var bookmarks = await _bookmarkRepo.FindAsync(b => b.QuestionId == questionId);
        if (bookmarks.Count == 0) return false;

        await _bookmarkRepo.DeleteAsync(bookmarks.First());
        return true;
    }

    /// <summary>
    /// Összes könyvjelző lekérdezése a kérdés és témakör adataival.
    /// </summary>
    public async Task<List<Bookmark>> GetAllBookmarksAsync()
    {
        return await _bookmarkRepo.GetAllWithQuestionsAsync();
    }

    /// <summary>
    /// Ellenőrzi, hogy egy kérdés könyvjelzőzve van-e.
    /// </summary>
    public async Task<bool> IsBookmarkedAsync(int questionId)
    {
        var result = await _bookmarkRepo.FindAsync(b => b.QuestionId == questionId);
        return result.Count > 0;
    }
}
