using Microsoft.AspNetCore.Mvc;
using Summarizer.Api.Services;

namespace Summarizer.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISummarizer _summarizer;
        private readonly ITextExtractor _extractor;

        public HomeController(ISummarizer summarizer, ITextExtractor extractor)
        {
            _summarizer = summarizer;
            _extractor = extractor;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                ViewBag.Error = "Please select a file.";
                return View();
            }

            using var ms = new MemoryStream();
            await document.CopyToAsync(ms);

            try
            {
                var text = await _extractor.ExtractTextAsync(ms, document.FileName);
                var summary = await _summarizer.SummarizeAsync(text);
                ViewBag.Summary = summary;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
            }

            return View();
        }
    }
}