let csrf = ""; // keep if you later re-enable antiforgery

export async function initAntiforgery() {
  // No-op for now because your endpoints use .DisableAntiforgery()
  // If you later enable antiforgery on the API, implement the token fetch here.
  return;
}

export async function postSummarizeText(text: string, sentences = 6) {
  return fetch(`/summarize/text?sentences=${sentences}`, {
    method: "POST",
    // credentials not needed for same-origin without cookies; keep if you need them
    headers: {
      "Content-Type": "text/plain", // <-- server expects raw text
      // "X-CSRF-TOKEN": csrf, // only if you enable antiforgery
    },
    body: text, // <-- raw string (DO NOT JSON.stringify)
  });
}

export async function postSummarizeDocument(file: File, sentences = 6) {
  const fd = new FormData();
  fd.append("document", file); // <-- must match IFormFile parameter name

  return fetch(`/summarize/document?sentences=${sentences}`, {
    method: "POST",
    // DO NOT set Content-Type for FormData; browser sets boundary
    body: fd,
  });
}

export async function postSummarizeAbstractive(text: string, sentences = 6) {
  // Server expects JSON { text, sentenceCount }
  return fetch(`/summarize/abstractive?sentences=${sentences}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text, sentenceCount: sentences }),
  });
}