// File: src/Summarizer.Api/Program.cs
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

// ---------- Helpers ----------
static string? TryExtractTextFromAnalyzeResult(AnalyzeResult ar, int maxChars = 16000)
{
    // Prefer the built-in flattened content first
    if (!string.IsNullOrWhiteSpace(ar.Content))
    {
        return ar.Content.Length > maxChars ? ar.Content[..maxChars] : ar.Content;
    }

    // Fall back to paragraphs
    if (ar.Paragraphs is not null && ar.Paragraphs.Count > 0)
    {
        var sb = new StringBuilder();
        foreach (var p in ar.Paragraphs)
        {
            if (!string.IsNullOrWhiteSpace(p.Content))
            {
                if (sb.Length + p.Content.Length > maxChars)
                {
                    sb.Append(p.Content.AsSpan(0, maxChars - sb.Length));
                    break;
                }
                sb.AppendLine(p.Content);
            }
        }
        if (sb.Length > 0) return sb.ToString();
    }

    // Fall back to page lines
    if (ar.Pages is not null && ar.Pages.Count > 0)
    {
        var sb = new StringBuilder();
        foreach (var page in ar.Pages)
        {
            if (page.Lines is null) continue;
            foreach (var line in page.Lines)
            {
                if (!string.IsNullOrWhiteSpace(line.Content))
                {
                    if (sb.Length + line.Content.Length > maxChars)
                    {
                        sb.Append(line.Content.AsSpan(0, maxChars - sb.Length));
                        goto Done;
                    }
                    sb.AppendLine(line.Content);
                }
            }
        }
    Done:
        if (sb.Length > 0) return sb.ToString();
    }

    return null;
}

static async Task<object> AnalyzeTextAsync(string text, TextAnalyticsClient taClient)
{
    var sample = text.Length > 15000 ? text[..15000] : text;

    string? detectedLanguageName = null;
    string? detectedLanguageIso = null;
    string[] keywords = Array.Empty<string>();
    string? summary = null;
    string? taError = null;

    // Language + Key Phrases
    try
    {
        var lang = await taClient.DetectLanguageAsync(sample);
        detectedLanguageName = lang.Value.Name;
        detectedLanguageIso = lang.Value.Iso6391Name;

        var kp = await taClient.ExtractKeyPhrasesAsync(sample);
        keywords = kp.Value
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(s => s.Length)
                    .Take(25)
                    .ToArray();
    }
    catch (Exception ex)
    {
        taError = $"Language/KeyPhrases failed: {ex.Message}";
    }

    // Extractive summarization (with simple fallback)
    try
    {
        var docs = new List<string> { sample };
        var actions = new TextAnalyticsActions
        {
            ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction> { new() }
        };

        var op = await taClient.StartAnalyzeActionsAsync(docs, actions);
        await op.WaitForCompletionAsync();

        await foreach (var page in op.Value)
        {
            foreach (var exResult in page.ExtractiveSummarizeResults)
            {
                if (exResult.HasError) continue;
                var docResult = exResult.DocumentsResults.FirstOrDefault();
                if (docResult != null && docResult.Sentences.Count > 0)
                {
                    summary = string.Join(" ",
                        docResult.Sentences
                            .OrderByDescending(s => s.RankScore)
                            .Take(3)
                            .Select(s => s.Text.Trim()));
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(summary)) break;
        }
    }
    catch
    {
        var sentences = System.Text.RegularExpressions.Regex
            .Split(sample.Trim(), @"(?<=[\.!\?])\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Take(3)
            .ToArray();
        summary = sentences.Length > 0 ? string.Join(" ", sentences) : sample[..Math.Min(400, sample.Length)];
    }

    return new
    {
        detectedLanguage = detectedLanguageName,
        detectedLanguageIso,
        summary,
        keywords,
        textPreview = sample.Length > 600 ? sample[..600] + "…" : sample,
        textAnalyticsError = taError
    };
}

// ---------- Startup ----------
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
builder.Services.AddControllers();

string langEndpoint;
string langKey;
string diEndpoint;
string diKey;

// Load .env only in Development
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    // Only load env file in Development
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    {
        // Get project root (where Summarizer.Api.csproj lives)
        string projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName
                             ?? throw new InvalidOperationException("Unable to locate project root");

        // Build full path to localvar.env
        string envPath = Path.Combine(projectRoot, "localvar.env");

        // Load the file
        Env.Load(envPath);
        Console.WriteLine($"Loaded environment variables from: {envPath}");
        Console.WriteLine(Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT"));
    }

    // logging
    Console.WriteLine($"Environment: {environment}");
    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT")!;
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY")!;
    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT")!;
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY")!;
}
else
{
    // Azure AI clients from environment variables
    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT")
        ?? throw new InvalidOperationException("AI_LANGUAGE_ENDPOINT not set");
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY")
        ?? throw new InvalidOperationException("AI_LANGUAGE_KEY not set");

    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT")
        ?? throw new InvalidOperationException("AI_DOCINTEL_ENDPOINT not set");
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY")
        ?? throw new InvalidOperationException("AI_DOCINTEL_KEY not set");
}

builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(langEndpoint), new AzureKeyCredential(langKey)));
builder.Services.AddSingleton(new DocumentIntelligenceClient(new Uri(diEndpoint), new AzureKeyCredential(diKey)));

var app = builder.Build();

app.UseHttpsRedirection();

// ✅ Swagger always on (easier verification in Azure)
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Root + health endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Ok("OK"));

// Controllers
app.MapControllers();

// ---------- POST /summarize/document (file upload; fast: pages + timeout) ----------
app.MapPost("/summarize/document",
    async (HttpRequest req,
           IFormFile document,
           DocumentIntelligenceClient diClient,
           TextAnalyticsClient taClient) =>
    {
        if (document is null || document.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded for 'document'." });

        // Query params
        var pagesParam = req.Query["pages"].FirstOrDefault();
        var pages = string.IsNullOrWhiteSpace(pagesParam) ? "1-5" : pagesParam;

        int timeoutSeconds = 45;
        if (int.TryParse(req.Query["timeoutSeconds"].FirstOrDefault(), out var t) && t > 0 && t <= 180)
            timeoutSeconds = t;

        await using var ms = new MemoryStream();
        await document.CopyToAsync(ms);
        var bytes = ms.ToArray();

        string? diError = null;
        string? diDiagnostic = null;
        string? extractedText = null;

        var sw = Stopwatch.StartNew();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            // Build request body with page filter (string like "1-5" or "1,3,5")
            var requestBody = new
            {
                base64Source = Convert.ToBase64String(bytes),
                contentType = string.IsNullOrWhiteSpace(document.ContentType)
                                ? "application/pdf"
                                : document.ContentType,
                pages = pages
            };

            // NOTE: Your SDK returns AnalyzeResult (strongly-typed), not BinaryData
            var diOp = await diClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromObjectAsJson(requestBody),
                cts.Token
            );

            var analyze = diOp.Value; // AnalyzeResult
            extractedText = TryExtractTextFromAnalyzeResult(analyze, 16000);
        }
        catch (TaskCanceledException)
        {
            diError = $"Timed out after {timeoutSeconds}s while reading pages '{pages}'. Try fewer pages (e.g., 1-2).";
        }
        catch (Exception ex)
        {
            diError = ex.Message;
        }

        var diMs = sw.ElapsedMilliseconds;

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Results.Ok(new
            {
                fileName = document.FileName,
                length = document.Length,
                pages,
                diMs,
                detectedLanguage = (string?)null,
                detectedLanguageIso = (string?)null,
                summary = (string?)null,
                keywords = Array.Empty<string>(),
                textPreview = (string?)null,
                documentIntelligenceError = diError ?? "No text extracted.",
                diDiagnostic,
                textAnalyticsError = "Skipped because no text to analyze."
            });
        }

        var analysis = await AnalyzeTextAsync(extractedText, taClient);
        var totalMs = sw.ElapsedMilliseconds;

        return Results.Ok(new
        {
            fileName = document.FileName,
            length = document.Length,
            pages,
            diMs,
            totalMs,
            analysis
        });
    })
    .WithMetadata(new ConsumesAttribute("multipart/form-data"))
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .DisableAntiforgery();

// ---------- POST /summarize/text (raw text body) ----------
app.MapPost("/summarize/text",
        async (HttpRequest req, TextAnalyticsClient taClient) =>
        {
            using var reader = new StreamReader(req.Body, leaveOpen: false);
            var text = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(text))
                return Results.BadRequest(new { error = "Body must contain non-empty text." });

            var analysis = await AnalyzeTextAsync(text, taClient);
            return Results.Ok(analysis);
        })
    .WithMetadata(new ConsumesAttribute("text/plain"))
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .DisableAntiforgery();

app.Run();
