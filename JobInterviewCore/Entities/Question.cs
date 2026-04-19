using JobInterviewCore.Enums;

namespace JobInterviewCore.Entities;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
    public string Answer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Topic Topic { get; set; } = null!;
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
