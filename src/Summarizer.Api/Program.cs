using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;          // [FromForm]
using Microsoft.OpenApi.Models;          // OpenAPI schema
using Summarizer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- ENV VARS ONLY (no user-secrets, no CLI) ----
string Need(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"{name} not set");

// Read required values
var langEndpoint = Need("AI_LANGUAGE_ENDPOINT");
var langKey = Need("AI_LANGUAGE_KEY");
var diEndpoint = Need("AI_DOCINTEL_ENDPOINT");
var diKey = Need("AI_DOCINTEL_KEY");

// Azure clients
builder.Services.AddSingleton(new TextAnalyticsClient(new Uri(langEndpoint), new AzureKeyCredential(langKey)));
builder.Services.AddSingleton(new DocumentIntelligenceClient(new Uri(diEndpoint), new AzureKeyCredential(diKey)));

// App services
builder.Services.AddScoped<ITextExtractor, DocumentIntelligenceTextExtractor>();
builder.Services.AddScoped<ISummarizer, LanguageSummarizer>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Quick diagnostics (true/false only)
app.MapGet("/whoami", () => new {
    AI_DOCINTEL_ENDPOINT = Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT") != null,
    AI_LANGUAGE_ENDPOINT = Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT") != null
});

// ---- File upload summarize (works in Swagger) ----
app.MapPost("/summarize/form",
    async ([FromForm] IFormFile document,
           ITextExtractor extractor,
           ISummarizer summarizer,
           CancellationToken ct) =>
    {
        if (document is null || document.Length == 0)
            return Results.BadRequest(new { error = "No file uploaded or file is empty." });

        // DI-supported types (DOCX works in your setup)
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf", ".docx", ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".bmp" };
        var ext = Path.GetExtension(document.FileName);
        if (!allowed.Contains(ext))
            return Results.Problem("Upload PDF, DOCX, JPG, PNG, TIFF, or BMP.",
                statusCode: StatusCodes.Status415UnsupportedMediaType);

        await using var stream = document.OpenReadStream();
        var text = await extractor.ExtractTextAsync(stream, document.FileName, ct);
        if (string.IsNullOrWhiteSpace(text))
            return Results.BadRequest(new { error = "No text could be extracted from the file." });

        var result = await summarizer.SummarizeAsync(text, ct);
        return Results.Ok(result);
    })
    .DisableAntiforgery() // avoids CSRF requirement in Swagger
    .Accepts<IFormFile>("multipart/form-data")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status415UnsupportedMediaType)
    .WithName("SummarizeFile")
    .WithOpenApi(op =>
    {
        // Make Swagger render a real file picker and send the file bytes
        op.Parameters.Clear();
        op.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["document"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "PDF, DOCX, JPG, PNG, TIFF, BMP"
                            }
                        },
                        Required = new HashSet<string> { "document" }
                    }
                }
            }
        };
        return op;
    });

app.Run();
