using JobInterviewCore.DTOs;
using JobInterviewCore.Entities;
using JobInterviewCore.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace JobInterviewBusinessLogic.Services;

public class WeeklySummaryService : IWeeklySummaryService
{
    private readonly IStatisticsService _statisticsService;
    private readonly IRepository<Attempt> _attemptRepo;
    private readonly IConfiguration _configuration;

    public WeeklySummaryService(
        IStatisticsService statisticsService,
        IRepository<Attempt> attemptRepo,
        IConfiguration configuration)
    {
        _statisticsService = statisticsService;
        _attemptRepo = attemptRepo;
        _configuration = configuration;
    }

    /// <summary>
    /// Heti összefoglaló generálása az utolsó 7 nap adataiból.
    /// </summary>
    public async Task<WeeklySummaryDto> GenerateWeeklySummaryAsync()
    {
        var weekEnd = DateTime.UtcNow;
        var weekStart = weekEnd.AddDays(-7);

        // Heti próbálkozások lekérdezése
        var weeklyAttempts = await _attemptRepo.FindAsync(
            a => a.AttemptedAt >= weekStart && a.AttemptedAt <= weekEnd);

        // Témakör statisztikák
        var topicStats = await _statisticsService.GetTopicStatsAsync();
        var activeTopic = topicStats.Where(t => t.TotalAttempts > 0).ToList();

        var weakest = activeTopic
            .OrderBy(t => t.AccuracyPercentage)
            .FirstOrDefault();

        var best = activeTopic
            .OrderByDescending(t => t.AccuracyPercentage)
            .FirstOrDefault();

        return new WeeklySummaryDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            TotalAttempts = weeklyAttempts.Count,
            CorrectAnswers = weeklyAttempts.Count(a => a.Correct),
            UniqueQuestionsAttempted = weeklyAttempts
                .Select(a => a.QuestionId)
                .Distinct()
                .Count(),
            WeakestTopic = weakest?.TopicName ?? "N/A",
            BestTopic = best?.TopicName ?? "N/A",
            TopicBreakdown = topicStats
        };
    }

    /// <summary>
    /// Heti összefoglaló küldése e-mailben (MailKit + SMTP).
    /// Az SMTP beállítások az appsettings.json SmtpSettings szekciójából jönnek.
    /// </summary>
    public async Task<bool> SendWeeklyEmailAsync(string recipientEmail)
    {
        var smtp = _configuration.GetSection("SmtpSettings");
        var host = smtp["Host"];
        var port = int.TryParse(smtp["Port"], out var p) ? p : 587;
        var username = smtp["Username"];
        var password = smtp["Password"];
        var fromEmail = smtp["FromEmail"];
        var fromName = smtp["FromName"] ?? "Állásinterjú Gyakorló";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        var summary = await GenerateWeeklySummaryAsync();

        // HTML email felépítése
        var html = $@"
        <html>
        <body style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 12px 12px 0 0;'>
                <h1 style='color: white; margin: 0;'>📊 Heti Összefoglaló</h1>
                <p style='color: rgba(255,255,255,0.8); margin: 5px 0 0;'>
                    {summary.WeekStart:yyyy.MM.dd} – {summary.WeekEnd:yyyy.MM.dd}
                </p>
            </div>
            <div style='background: #f8f9fa; padding: 25px; border-radius: 0 0 12px 12px;'>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6;'>
                            <strong>Összes próbálkozás</strong>
                        </td>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6; text-align: right; font-size: 1.2em;'>
                            {summary.TotalAttempts}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6;'>
                            <strong>Pontosság</strong>
                        </td>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6; text-align: right; font-size: 1.2em; color: {(summary.AccuracyPercentage >= 70 ? "#28a745" : summary.AccuracyPercentage >= 40 ? "#ffc107" : "#dc3545")};'>
                            {summary.AccuracyPercentage}%
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6;'>
                            <strong>Egyedi kérdések</strong>
                        </td>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6; text-align: right; font-size: 1.2em;'>
                            {summary.UniqueQuestionsAttempted}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6;'>
                            <strong>Leggyengébb téma</strong>
                        </td>
                        <td style='padding: 12px; border-bottom: 1px solid #dee2e6; text-align: right; color: #dc3545;'>
                            {summary.WeakestTopic}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 12px;'>
                            <strong>Legerősebb téma</strong>
                        </td>
                        <td style='padding: 12px; text-align: right; color: #28a745;'>
                            {summary.BestTopic}
                        </td>
                    </tr>
                </table>
                <p style='text-align: center; margin-top: 20px; color: #6c757d; font-size: 0.85em;'>
                    Küldve az Állásinterjú Gyakorló rendszer által
                </p>
            </div>
        </body>
        </html>";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = $"Heti Összefoglaló – {summary.WeekStart:yyyy.MM.dd} - {summary.WeekEnd:yyyy.MM.dd}";

        var bodyBuilder = new BodyBuilder { HtmlBody = html };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        return true;
    }
}
