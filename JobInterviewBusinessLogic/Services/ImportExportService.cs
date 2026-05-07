using System.Text.Json;
using JobInterviewCore.DTOs;
using JobInterviewCore.Entities;
using JobInterviewCore.Enums;
using JobInterviewCore.Interfaces;

namespace JobInterviewBusinessLogic.Services;

public class ImportExportService : IImportExportService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IRepository<Topic> _topicRepo;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ImportExportService(
        IQuestionRepository questionRepo,
        IRepository<Topic> topicRepo)
    {
        _questionRepo = questionRepo;
        _topicRepo = topicRepo;
    }

    // Összes kérdés exportálása JSON fájlba.
    public async Task ExportQuestionsAsync(string filePath)
    {
        var questions = await _questionRepo.GetAllWithDetailsAsync();

        var exportData = new ImportData
        {
            Questions = questions.Select(q => new QuestionExportDto
            {
                Text = q.Text,
                Answer = q.Answer,
                TopicName = q.Topic.Name,
                Difficulty = q.Difficulty.ToString()
            }).ToList()
        };

        var json = JsonSerializer.Serialize(exportData, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    // Kérdések importálása JSON fájlból validációval.
    public async Task<(int imported, int skipped, List<string> errors)> ImportQuestionsAsync(string filePath)
    {
        int imported = 0, skipped = 0;
        var errors = new List<string>();

        if (!File.Exists(filePath))
        {
            errors.Add($"A fájl nem található: {filePath}");
            return (0, 0, errors);
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            errors.Add($"Olvasási hiba: {ex.Message}");
            return (0, 0, errors);
        }

        ImportData? data;
        try
        {
            data = JsonSerializer.Deserialize<ImportData>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            errors.Add($"JSON formátum hiba: {ex.Message}");
            return (0, 0, errors);
        }

        if (data?.Questions == null || data.Questions.Count == 0)
        {
            errors.Add("A fájl nem tartalmaz kérdéseket.");
            return (0, 0, errors);
        }

        var existingTopics = await _topicRepo.GetAllAsync();
        var existingQuestions = await _questionRepo.GetAllAsync();

        foreach (var dto in data.Questions)
        {
            // Validáció: szöveg
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                errors.Add("Üres szövegű kérdés kihagyva.");
                skipped++;
                continue;
            }

            // Validáció: nehézség
            if (!Enum.TryParse<Difficulty>(dto.Difficulty, true, out var difficulty))
            {
                errors.Add($"Érvénytelen nehézség '{dto.Difficulty}': {dto.Text[..Math.Min(50, dto.Text.Length)]}...");
                skipped++;
                continue;
            }

            // Validáció: témakör
            if (string.IsNullOrWhiteSpace(dto.TopicName))
            {
                errors.Add($"Hiányzó témakör: {dto.Text[..Math.Min(50, dto.Text.Length)]}...");
                skipped++;
                continue;
            }

            // Témakör keresése / létrehozása
            var topic = existingTopics.FirstOrDefault(t =>
                t.Name.Equals(dto.TopicName, StringComparison.OrdinalIgnoreCase));

            if (topic == null)
            {
                topic = new Topic { Name = dto.TopicName };
                await _topicRepo.AddAsync(topic);
                existingTopics.Add(topic);
            }

            // Duplikáció ellenőrzés
            if (existingQuestions.Any(q =>
                q.Text.Equals(dto.Text, StringComparison.OrdinalIgnoreCase)))
            {
                skipped++;
                continue;
            }

            var question = new Question
            {
                Text = dto.Text,
                Answer = dto.Answer ?? string.Empty,
                TopicId = topic.Id,
                Difficulty = difficulty,
                CreatedAt = DateTime.UtcNow
            };

            await _questionRepo.AddAsync(question);
            existingQuestions.Add(question);
            imported++;
        }

        return (imported, skipped, errors);
    }
}
