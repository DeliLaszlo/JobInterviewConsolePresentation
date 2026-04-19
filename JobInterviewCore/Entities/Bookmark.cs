namespace JobInterviewCore.Entities;

public class Bookmark
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
}
