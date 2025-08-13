using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Azure AI clients (from environment variables)
builder.Services.AddSingleton(new TextAnalyticsClient(
    new Uri(Environment.GetEnvironmentVariable("AI_LANGUAGE_ENDPOINT")
            ?? throw new InvalidOperationException("AI_LANGUAGE_ENDPOINT not set")),
    new AzureKeyCredential(Environment.GetEnvironmentVariable("AI_LANGUAGE_KEY")
                           ?? throw new InvalidOperationException("AI_LANGUAGE_KEY not set"))
));

builder.Services.AddSingleton(new DocumentIntelligenceClient(
    new Uri(Environment.GetEnvironmentVariable("AI_DOCINTEL_ENDPOINT")
            ?? throw new InvalidOperationException("AI_DOCINTEL_ENDPOINT not set")),
    new AzureKeyCredential(Environment.GetEnvironmentVariable("AI_DOCINTEL_KEY")
                           ?? throw new InvalidOperationException("AI_DOCINTEL_KEY not set"))
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API endpoint for file upload
app.MapPost("/summarize",
        async ([FromForm(Name = "document")] IFormFile document) =>
        {
            if (document is null || document.Length == 0)
                return Results.BadRequest(new { error = "No file uploaded for 'document'." });

            using var stream = document.OpenReadStream();
            // TODO: process file stream with your Azure clients

            return Results.Ok(new
            {
                fileName = document.FileName, // confirm binding works
                length = document.Length
            });
        })
    .WithMetadata(new ConsumesAttribute("multipart/form-data"))
    .DisableAntiforgery();  // <-- this line fixes the 500 from antiforgery
//     and affects only this endpoint
app.Run();