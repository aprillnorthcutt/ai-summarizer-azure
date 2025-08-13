// File: src/Summarizer.Api/Program.cs
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using Azure.Core;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Json;

// ---------- Helpers ----------
static string? TryExtractTextFromBinaryData(BinaryData bd, int maxChars = 16000)
{
    try
    {
        var json = bd.ToString();
        using var doc = JsonDocument.Parse(json);

        static string? pullFromElement(JsonElement el, int max)
        {
            // content
            if (el.TryGetProperty("content", out var c))
            {
                var s = c.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s.Length > max ? s[..max] : s;
            }
            // paragraphs[].content
            if (el.TryGetProperty("paragraphs", out var paras) &&
                paras.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();
                foreach (var p in paras.EnumerateArray())
                {
                    if (p.TryGetProperty("content", out var pc))
                    {
                        var t = pc.GetString();
                        if (!string.IsNullOrWhiteSpace(t))
                        {
                            sb.AppendLine(t);
                            if (sb.Length > max) break;
                        }
                    }
                }
                if (sb.Length > 0) return sb.ToString();
            }
            // pages[].content and/or pages[].lines[].content
            if (el.TryGetProperty("pages", out var pages) &&
                pages.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();
                foreach (var p in pages.EnumerateArray())
                {
                    if (p.TryGetProperty("content", out var pc))
                    {
                        var t = pc.GetString();
                        if (!string.IsNullOrWhiteSpace(t))
                        {
                            sb.AppendLine(t);
                            if (sb.Length > max) break;
                        }
                    }
                    if (p.TryGetProperty("lines", out var lines) &&
                        lines.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var ln in lines.EnumerateArray())
                        {
                            if (ln.TryGetProperty("content", out var lc))
                            {
                                var t = lc.GetString();
                                if (!string.IsNullOrWhiteSpace(t))
                                {
                                    sb.AppendLine(t);
                                    if (sb.Length > max) break;
                                }
                            }
                        }
                    }
                    if (sb.Length > max) break;
                }
                if (sb.Length > 0) return sb.ToString();
            }
            return null;
        }

        // Try root
        var root = doc.RootElement;
        var s = pullFromElement(root, maxChars);
        if (!string.IsNullOrWhiteSpace(s)) return s;

        // Try nested result/analyzeResult
        foreach (var key in new[] { "result", "analyzeResult" })
        {
            if (root.TryGetProperty(key, out var inner))
            {
                s = pullFromElement(inner, maxChars);
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
        }

        return null;
    }
    catch
    {
        return null;
    }
}

static string? TryExtractText(object? resultObj, int maxChars = 16000)
{
    if (resultObj is null) return null;

    var contentProp = resultObj.GetType().GetProperty("Content");
    var contentVal = contentProp?.GetValue(resultObj)?.ToString();
    if (!string.IsNullOrWhiteSpace(contentVal))
        return contentVal.Length > maxChars ? contentVal[..maxChars] : contentVal;

    return null; // keeping this simple now — BinaryData will be handled separately
}

// ---------- Startup ----------
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

string langEndpoint;
string langKey;
string diEndpoint;
string diKey;
// Load .env only in Development
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


if (environment == "Development")
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

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


    //logging
    Console.WriteLine($"Environment: {environment}");
    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT");
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY");
    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT");
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY");

}
else { 

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ---------- Endpoint ----------
app.MapPost("/summarize",
    async (
        [FromForm(Name = "document")] IFormFile document,
        DocumentIntelligenceClient diClient,
        TextAnalyticsClient taClient) =>
    {
        if (document is null || document.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded for 'document'." });

        await using var ms = new MemoryStream();
        await document.CopyToAsync(ms);
        var bytes = ms.ToArray();

        string? diError = null;
        string? taError = null;
        string? diDiagnostic = null;
        string? extractedText = null;

        // --- Document Intelligence ---
        try
        {
            var diOp = await diClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                modelId: "prebuilt-read",
                content: BinaryData.FromObjectAsJson(new
                {
                    base64Source = Convert.ToBase64String(bytes),
                    contentType = string.IsNullOrWhiteSpace(document.ContentType)
                                    ? "application/pdf"
                                    : document.ContentType
                })
            );

            var resultObj = diOp.Value;

            if (resultObj is BinaryData bdValue)
            {
                extractedText = TryExtractTextFromBinaryData(bdValue, 16000);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    try
                    {
                        using var j = JsonDocument.Parse(bdValue.ToString());
                        var keys = new List<string>();
                        foreach (var p in j.RootElement.EnumerateObject()) keys.Add(p.Name);
                        foreach (var container in new[] { "result", "analyzeResult" })
                        {
                            if (j.RootElement.TryGetProperty(container, out var inner) &&
                                inner.ValueKind == JsonValueKind.Object)
                            {
                                keys.Add(container + ":{ " +
                                    string.Join(",", inner.EnumerateObject().Select(o => o.Name)) + " }");
                            }
                        }
                        diDiagnostic = "BinaryData JSON keys: [" + string.Join(", ", keys) + "]";
                    }
                    catch { }
                }
            }
            else
            {
                extractedText = TryExtractText(resultObj, 16000);
            }
        }
        catch (Exception ex)
        {
            diError = ex.Message;
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Results.Ok(new
            {
                fileName = document.FileName,
                length = document.Length,
                detectedLanguage = (string?)null,
                detectedLanguageIso = (string?)null,
                summary = (string?)null,
                keywords = Array.Empty<string>(),
                textPreview = (string?)null,
                documentIntelligenceError = diError,
                diDiagnostic,
                textAnalyticsError = "No text extracted to analyze."
            });
        }

        var sample = extractedText.Length > 15000 ? extractedText[..15000] : extractedText;

        // --- Text Analytics ---
        string? detectedLanguageName = null;
        string? detectedLanguageIso = null;
        string[] keywords = Array.Empty<string>();
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

        // --- Simple extractive summary ---
        string? summary = null;
        try
        {
            var docs = new List<string> { sample };
            var actions = new TextAnalyticsActions
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>
                {
                    new ExtractiveSummarizeAction()
                }
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
            // fallback
            var sentences = System.Text.RegularExpressions.Regex
                .Split(sample.Trim(), @"(?<=[\.!\?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(3)
                .ToArray();
            summary = sentences.Length > 0 ? string.Join(" ", sentences) : sample[..Math.Min(400, sample.Length)];
        }

        return Results.Ok(new
        {
            fileName = document.FileName,
            length = document.Length,
            detectedLanguage = detectedLanguageName,
            detectedLanguageIso,
            summary,
            keywords,
            textPreview = sample.Length > 600 ? sample[..600] + "…" : sample,
            documentIntelligenceError = diError,
            diDiagnostic,
            textAnalyticsError = taError
        });
    })
    .WithMetadata(new ConsumesAttribute("multipart/form-data"))
    .DisableAntiforgery();

app.Run();
