
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

using System.Diagnostics;
using System.Text.RegularExpressions;

using Azure;
using Azure.AI.OpenAI;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;

using DotNetEnv;
using Microsoft.AspNetCore.Mvc;

using Summarizer.Api.Models;
using Swashbuckle.AspNetCore.Annotations;




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
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
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
    openAiDeployment = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("OPENAI_DEPLOYMENT_NAME not set");

}

builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(langEndpoint), new AzureKeyCredential(langKey)));
builder.Services.AddSingleton(new DocumentIntelligenceClient(new Uri(diEndpoint), new AzureKeyCredential(diKey)));

builder.Services.AddSingleton(new OpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey)));
builder.Services.AddSingleton(_ => openAiDeployment);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAntiforgery();
app.UseDefaultFiles();
app.UseStaticFiles();

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
    .WithMetadata(new ConsumesAttribute("text/plain"))
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .DisableAntiforgery();

//app.MapPost("/summarize/abstractive", async (
//    OpenAIClient openAIClient,
//    [FromBody] AbstractiveRequest request) =>
//{
//    string deploymentName = "abstractive-gpt"; // update this if your deployment name is different
//    string inputText = request.Text ?? string.Empty;

//    string userPrompt = request.SentenceCount.HasValue
//        ? $"Summarize the following text in exactly {request.SentenceCount.Value} sentence(s): {inputText}"
//        : $"Summarize the following text clearly and concisely: {inputText}";

//    var chatOptions = new ChatCompletionsOptions
//    {
//        Temperature = 0.5f,
//        MaxTokens = 500
//    };

//    chatOptions.Messages.Add(new ChatMessage(ChatRole.System, "You are a helpful assistant that summarizes text."));
//    chatOptions.Messages.Add(new ChatMessage(ChatRole.User, userPrompt));

//    Response<ChatCompletions> response = await openAIClient.GetChatCompletionsAsync(deploymentName, chatOptions);
//    string summary = response.Value.Choices[0].Message.Content;

//    return Results.Ok(new { summary });
//});

app.MapPost("/summarize/abstractive", async (
    OpenAIClient openAIClient,
    [FromBody] AbstractiveRequest request) =>
{
    string deploymentName = "abstractive-gpt";
    string inputText = request.Text ?? string.Empty;

    string userPrompt = request.SentenceCount.HasValue
        ? $"Summarize the following text in exactly {request.SentenceCount.Value} sentence(s): {inputText}"
        : $"Summarize the following text clearly and concisely: {inputText}";

    var chatOptions = new ChatCompletionsOptions
    {
        Temperature = 0.5f,
        MaxTokens = 500,
        Messages =
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant that summarizes text."),
            new ChatMessage(ChatRole.User, userPrompt)
        }
    };

    var response = await openAIClient.GetChatCompletionsAsync(deploymentName, chatOptions);
    string summary = response.Value.Choices[0].Message.Content;

    return Results.Ok(new { summary });
});



// ---------- Serve React frontend ----------
app.MapFallbackToFile("index.html").DisableAntiforgery();



app.Run();
