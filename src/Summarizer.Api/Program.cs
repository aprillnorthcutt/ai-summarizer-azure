
using System;
using System.Text;

using System.Diagnostics;
using System.Text.RegularExpressions;

using Azure;
using Azure.AI.OpenAI;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;



static string NormalizeText(string s) =>
    Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();

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

static async Task<string> GetAbstractiveSummaryAsync(OpenAIClient client, string deploymentName, string input, int? sentenceCount = null)
{
    string userPrompt = sentenceCount.HasValue
        ? $"Summarize the following text in exactly {sentenceCount.Value} sentence(s):\n\n{input}"
        : $"Summarize the following text clearly and concisely:\n\n{input}";

    var options = new ChatCompletionsOptions
    {
        Messages =
        {
            new ChatMessage(ChatRole.System, "You summarize text in a concise and professional way."),
            new ChatMessage(ChatRole.User, userPrompt)
        },
        Temperature = 0.5f,
        MaxTokens = 512
    };

    var response = await client.GetChatCompletionsAsync(deploymentName, options);
    return response.Value.Choices[0].Message.Content.Trim();
}

static string[] CleanKeywords(IEnumerable<string> rawKeywords)
{
    var blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "sentence count", "key sentences", "new language",
        "original words", "term frequency", "text preview"
    };

    return rawKeywords
        .Select(k => k.TrimStart()) // Trim whitespace from start
        .Select(k => k.StartsWith("n") && k.Length > 2 && char.IsUpper(k[1])
            ? k.Substring(1) : k) // Remove leading 'n' if it's a weird prefix
        .Select(k => k.Trim()) // Final trim after transformation
        .Where(k =>
                k.Length >= 3 &&
                !blacklist.Contains(k) &&
                k.Any(char.IsLetter) &&
                !Regex.IsMatch(k, @"^[\W\d_]+$") // only symbols or digits
        )
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderByDescending(k => k.Length)
        .Take(15)
        .ToArray();
}


int ClampSentences(int? n) => Math.Clamp(n ?? 6, 3, 20); // adjust default and max

static async Task<object> AnalyzeTextAsync(string text, TextAnalyticsClient taClient, int sentenceCount)
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

        keywords = CleanKeywords(kp.Value);

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
            ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>
            {
                new ExtractiveSummarizeAction
                {
                    MaxSentenceCount = sentenceCount
                }
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
                    foreach (var s in docResult.Sentences)
                    {
                        Console.WriteLine($"[RankScore: {s.RankScore}] {s.Text}");
                    }

                    summary = string.Join(" ",
                        docResult.Sentences
                            .OrderByDescending(s => s.RankScore)
                            .Take(sentenceCount)
                            .OrderBy(s => s.Offset)
                            .Select(s => s.Text.Trim()));
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(summary)) break;
        }
    }
    catch
    {
        var sentences = Regex
            .Split(sample.Trim(), @"(?<=[\.!\?])\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Take(sentenceCount)
            .ToArray();
        summary = sentences.Length > 0
            ? string.Join(" ", sentences)
            : sample[..Math.Min(400, sample.Length)];
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddAntiforgery();

string langEndpoint, langKey, diEndpoint, diKey, openAiEndpoint, openAiKey, openAiDeployment;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment == "Development")
{
    string projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName
                         ?? throw new InvalidOperationException("Unable to locate project root");
    string envPath = Path.Combine(projectRoot, "localvar.env");
    if (File.Exists(envPath)) Env.Load(envPath);

    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT") ?? throw new InvalidOperationException("AI_LANGUAGE_ENDPOINT not set");
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY") ?? throw new InvalidOperationException("AI_LANGUAGE_KEY not set");
    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT") ?? throw new InvalidOperationException("AI_DOCINTEL_ENDPOINT not set");
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY") ?? throw new InvalidOperationException("AI_DOCINTEL_KEY not set");

    openAiEndpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("OPENAI_ENDPOINT not set");
    openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? throw new InvalidOperationException("OPENAI_KEY not set");
    openAiDeployment = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT") ?? throw new InvalidOperationException("OPENAI_DEPLOYMENT not set");

}
else
{
    langEndpoint = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT") ?? throw new InvalidOperationException("AI_LANGUAGE_ENDPOINT not set");
    langKey = Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY") ?? throw new InvalidOperationException("AI_LANGUAGE_KEY not set");
    diEndpoint = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT") ?? throw new InvalidOperationException("AI_DOCINTEL_ENDPOINT not set");
    diKey = Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY") ?? throw new InvalidOperationException("AI_DOCINTEL_KEY not set");

    openAiEndpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("OPENAI_ENDPOINT not set");
    openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY") ?? throw new InvalidOperationException("OPENAI_KEY not set");
    openAiDeployment = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT") ?? throw new InvalidOperationException("OPENAI_DEPLOYMENT not set");

}

builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(langEndpoint), new AzureKeyCredential(langKey)));
builder.Services.AddSingleton(new DocumentIntelligenceClient(new Uri(diEndpoint), new AzureKeyCredential(diKey)));
builder.Services.AddSingleton(new OpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey)));
builder.Services.AddSingleton(_ => openAiDeployment);

var app = builder.Build();

app.UseHttpsRedirection();
//app.UseSwagger();
//app.UseSwaggerUI();
app.UseAntiforgery();
app.UseDefaultFiles();  
app.UseStaticFiles();     
app.MapFallbackToFile("index.html");

// ---------- POST /summarize/document ----------
app.MapPost("/summarize/document",
    async (IFormFile document, DocumentIntelligenceClient diClient, TextAnalyticsClient taClient, int? sentences) =>
    {
        if (document is null || document.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded for 'document'." });

        string? diError = null;
        string? extractedText = null;
        var sw = Stopwatch.StartNew();

        try
        {
            await using var stream = document.OpenReadStream();
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

        extractedText = NormalizeText(extractedText);

        var n = ClampSentences(sentences);
        var analysis = await AnalyzeTextAsync(extractedText, taClient, n);
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
    .DisableAntiforgery();

// ---------- POST /summarize/text ----------
app.MapPost("/summarize/text",
        async (HttpRequest req, TextAnalyticsClient taClient, int? sentences) =>
        {
            using var reader = new StreamReader(req.Body);
            var text = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(text))
                return Results.BadRequest(new { error = "Body must contain non-empty text." });

            text = NormalizeText(text);
            var n = ClampSentences(sentences);
            var analysis = await AnalyzeTextAsync(text, taClient, n);

            return Results.Ok(analysis);
        })
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .DisableAntiforgery();


app.MapPost("/summarize/abstractive", async (
    HttpRequest req,
    OpenAIClient openAIClient,
    TextAnalyticsClient taClient,
    int? sentences) =>
{
    using var reader = new StreamReader(req.Body);
    var inputText = (await reader.ReadToEndAsync()) ?? string.Empty;

    if (string.IsNullOrWhiteSpace(inputText))
        return Results.BadRequest(new { error = "Body must contain non-empty text." });

    var n = ClampSentences(sentences);
    var sw = Stopwatch.StartNew();

    var sample = inputText.Length > 15000 ? inputText[..15000] : inputText;
    string? langName = null, langIso = null, taError = null;
    string[] keywords = Array.Empty<string>();
    try
    {
        var lang = await taClient.DetectLanguageAsync(sample);
        langName = lang.Value.Name;
        langIso = lang.Value.Iso6391Name;

        var kp = await taClient.ExtractKeyPhrasesAsync(sample);
        keywords = CleanKeywords(kp.Value);

    }
    catch (Exception ex)
    {
        taError = $"Language/KeyPhrases failed: {ex.Message}";
    }

    // --- Abstractive summary (OpenAI) ---
    var summary = await GetAbstractiveSummaryAsync(openAIClient, openAiDeployment, inputText, n);
    
    return Results.Ok(new
    {
        summary,
        keywords,
        detectedLanguage = langName,
        detectedLanguageIso = langIso,
        textPreview = sample.Length > 600 ? sample[..600] + "…" : sample,
        textAnalyticsError = taError,

    });
})
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.DisableAntiforgery();


app.Run();
