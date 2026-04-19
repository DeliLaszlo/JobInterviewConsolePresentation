namespace JobInterviewCore.Entities;

public class Rating
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int Score { get; set; } // 1–5
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
}
