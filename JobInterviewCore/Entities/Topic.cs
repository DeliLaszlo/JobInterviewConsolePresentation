namespace JobInterviewCore.Entities;

public class Topic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
