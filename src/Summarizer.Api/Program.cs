// File: src/Summarizer.Api/Program.cs
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;

using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;          // ✅ for .DisableAntiforgery() extensions
using Swashbuckle.AspNetCore.Annotations;

// ---------- Helpers ----------
static string? TryExtractTextFromAnalyzeResult(AnalyzeResult ar, int maxChars = 16000)
{
    if (!string.IsNullOrWhiteSpace(ar.Content))
        return ar.Content.Length > maxChars ? ar.Content[..maxChars] : ar.Content;

    if (ar.Paragraphs is not null && ar.Paragraphs.Count > 0)
    {
        var sb = new StringBuilder();
        foreach (var p in ar.Paragraphs)
        {
            if (string.IsNullOrWhiteSpace(p.Content)) continue;
            var remaining = maxChars - sb.Length;
            if (remaining <= 0) break;

            if (p.Content.Length > remaining) { sb.Append(p.Content.AsSpan(0, remaining)); break; }
            sb.AppendLine(p.Content);
        }
        if (sb.Length > 0) return sb.ToString();
    }

    if (ar.Pages is not null && ar.Pages.Count > 0)
    {
        var sb = new StringBuilder();
        foreach (var page in ar.Pages)
        {
            if (page.Lines is null) continue;
            foreach (var line in page.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.Content)) continue;
                var remaining = maxChars - sb.Length;
                if (remaining <= 0) goto Done;

                if (line.Content.Length > remaining) { sb.Append(line.Content.AsSpan(0, remaining)); goto Done; }
                sb.AppendLine(line.Content);
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

// ✅ Add antiforgery so Ignore/Disable metadata is understood by middleware
builder.Services.AddAntiforgery();

string langEndpoint;
string langKey;
string diEndpoint;
string diKey;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    // Load local .env if present
    string projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName
                         ?? throw new InvalidOperationException("Unable to locate project root");
    string envPath = Path.Combine(projectRoot, "localvar.env");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        Console.WriteLine($"Loaded environment variables from: {envPath}");
    }

    Console.WriteLine($"Environment: {environment}");
    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT")
        ?? throw new InvalidOperationException("AI_LANGUAGE_ENDPOINT not set");
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY")
        ?? throw new InvalidOperationException("AI_LANGUAGE_KEY not set");
    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT")
        ?? throw new InvalidOperationException("AI_DOCINTEL_ENDPOINT not set");
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY")
        ?? throw new InvalidOperationException("AI_DOCINTEL_KEY not set");
}
else
{
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

// Swagger always on for Azure verification
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Add antiforgery middleware so endpoints with antiforgery metadata don't throw
// (Place after auth if you add it later)
app.UseAntiforgery();

// Root + health
app.MapGet("/", () => Results.Redirect("/swagger"))
   .DisableAntiforgery(); // not necessary, but consistent with API-only app

app.MapGet("/healthz", () => Results.Ok("OK"))
   .DisableAntiforgery();

// Controllers
app.MapControllers();

// ---------- POST /summarize/document (BinaryData overload; antiforgery disabled for Swagger form) ----------
app.MapPost("/summarize/document",
    async (IFormFile document,
           DocumentIntelligenceClient diClient,
           TextAnalyticsClient taClient) =>
    {
        if (document is null || document.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded for 'document'." });

        string? diError = null;
        string? extractedText = null;
        var sw = Stopwatch.StartNew();

        try
        {
            await using var stream = document.OpenReadStream(); // no named args (broadest compatibility)
            var response = await diClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromStream(stream)
            );

            AnalyzeResult analyze = response.Value;
            extractedText = TryExtractTextFromAnalyzeResult(analyze, 16000);
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
                diMs,
                detectedLanguage = (string?)null,
                detectedLanguageIso = (string?)null,
                summary = (string?)null,
                keywords = Array.Empty<string>(),
                textPreview = (string?)null,
                documentIntelligenceError = diError ?? "No text extracted.",
                textAnalyticsError = "Skipped because no text to analyze."
            });
        }

        var analysis = await AnalyzeTextAsync(extractedText, taClient);
        var totalMs = sw.ElapsedMilliseconds;

        return Results.Ok(new
        {
            fileName = document.FileName,
            length = document.Length,
            diMs,
            totalMs,
            analysis
        });
    })
    .WithMetadata(new ConsumesAttribute("multipart/form-data"))
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .DisableAntiforgery(); // ✅ disable antiforgery on this form endpoint

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
    .DisableAntiforgery(); // ✅ allow simple POSTs from Swagger without tokens

app.Run();