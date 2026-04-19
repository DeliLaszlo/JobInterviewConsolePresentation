namespace JobInterviewCore.Entities;

public class Attempt
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public bool Correct { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
}
