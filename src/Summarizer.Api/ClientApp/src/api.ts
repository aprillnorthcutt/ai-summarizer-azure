let csrf = "";

export async function initAntiforgery() {
  const res = await fetch("/antiforgery/token", { credentials: "include" });
  if (!res.ok) throw new Error("CSRF bootstrap failed");
  const data = await res.json();
  csrf = data.requestToken ?? "";
}

export async function postSummarizeText(text: string) {
  return fetch("/summarize/text", {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      "X-CSRF-TOKEN": csrf,
    },
    body: JSON.stringify({ text }), // only { text }
  });
}

export async function postSummarizeDocument(file: File) {
  const fd = new FormData();
  fd.append("file", file); // name must match IFormFile param: "file"

  return fetch("/summarize/document", {
    method: "POST",
    credentials: "include",
    headers: {
      "X-CSRF-TOKEN": csrf,
      // DO NOT set Content-Type for FormData
    },
    body: fd,
  });
}
