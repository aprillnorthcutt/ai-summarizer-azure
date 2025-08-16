import React, { useRef, useState } from "react";

const API_BASE = window.location.origin;

export default function App() {
  return (
    <div className="min-h-screen bg-gray-50 text-gray-900 dark:bg-gray-950 dark:text-gray-100">
      <Header />
      <main className="mx-auto max-w-5xl px-4 pb-24">
        <Hero />
        <Tabs>
          <Tab label="Summarize Text">
            <TextSummarizer />
          </Tab>
          <Tab label="Summarize Document (.pdf, .png, .docx)">
            <DocumentSummarizer />
          </Tab>
        </Tabs>
        <FAQ />
      </main>
      <Footer />
    </div>
  );
}

/* ---------------- UI bits ---------------- */

function Header() {
  return (
    <header className="sticky top-0 z-40 backdrop-blur supports-[backdrop-filter]:bg-white/70 dark:supports-[backdrop-filter]:bg-gray-900/70 border-b border-gray-200 dark:border-gray-800">
      <div className="mx-auto max-w-5xl px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Logo />
          <span className="text-lg font-semibold tracking-tight">AI Summarizer</span>
        </div>
        <a
          href={`/swagger`}
          target="_blank"
          rel="noreferrer"
          className="text-sm rounded-xl px-3 py-1.5 border border-gray-300 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800"
        >
          Open Swagger
        </a>
      </div>
    </header>
  );
}

function Footer() {
  return (
    <footer className="border-t border-gray-200 dark:border-gray-800 mt-16">
      <div className="mx-auto max-w-5xl px-4 py-6 text-sm flex flex-col md:flex-row items-center justify-between gap-3">
        <p className="opacity-80">© {new Date().getFullYear()} AI Summarizer.</p>
        <p className="opacity-80">Temporary API UI. Swagger remains available.</p>
      </div>
    </footer>
  );
}

function Logo() {
  return <div className="h-8 w-8 rounded-2xl bg-gradient-to-br from-indigo-500 to-cyan-500 shadow" />;
}

function Hero() {
  return (
    <section className="text-center py-10 md:py-14">
      <h1 className="text-3xl md:text-4xl font-bold tracking-tight">Summarize text and documents fast</h1>
      <p className="mt-3 md:mt-4 text-base md:text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
        Paste text or upload a document. Get a clean, readable summary in seconds.
      </p>
      <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Backed by the same API used in Swagger.</p>
    </section>
  );
}

function SectionCard({ title, subtitle, children }: { title: string; subtitle?: string; children: React.ReactNode }) {
  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm p-5 md:p-6">
      <div className="mb-4">
        <h2 className="text-xl font-semibold tracking-tight">{title}</h2>
        {subtitle && <p className="text-sm text-gray-600 dark:text-gray-300 mt-1">{subtitle}</p>}
      </div>
      {children}
    </div>
  );
}

function Button({ children, className = "", ...rest }: any) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-xl px-4 py-2 text-sm font-medium border border-transparent bg-gray-900 text-white hover:bg-black disabled:opacity-50 disabled:cursor-not-allowed ${className}`}
      {...rest}
    >
      {children}
    </button>
  );
}

function GhostButton({ children, className = "", ...rest }: any) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-xl px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800 ${className}`}
      {...rest}
    >
      {children}
    </button>
  );
}

function Textarea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <textarea
      {...props}
      className={`w-full rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-950 px-3 py-2 text-sm focus:outline-none focus:ring-4 focus:ring-indigo-100 dark:focus:ring-indigo-900/40 resize-y min-h-[160px] ${props.className || ""}`}
    />
  );
}

function Input(props: React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      {...props}
      className={`w-full rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-950 px-3 py-2 text-sm focus:outline-none focus:ring-4 focus:ring-indigo-100 dark:focus:ring-indigo-900/40 ${props.className || ""}`}
    />
  );
}

function ClipboardButton({ text, label = "Copy" }: { text: string; label?: string }) {
  const [copied, setCopied] = useState(false);
  return (
    <GhostButton
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(text);
          setCopied(true);
          setTimeout(() => setCopied(false), 1500);
        } catch {}
      }}
    >
      {copied ? "Copied" : label}
    </GhostButton>
  );
}

function DownloadJsonButton({ data, filename = "summary.json" }: { data: any; filename?: string }) {
  return (
    <GhostButton
      onClick={() => {
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        a.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      }}
    >
      Download JSON
    </GhostButton>
  );
}

/* ---------------- Tabs ---------------- */

function Tabs({ children }: { children: React.ReactNode }) {
  const [active, setActive] = useState(0);
  const items = React.Children.toArray(children) as any[];
  return (
    <div className="mt-2">
      <div className="flex gap-2 overflow-x-auto pb-2">
        {items.map((it, i) => (
          <button
            key={i}
            onClick={() => setActive(i)}
            className={`rounded-2xl px-3 py-1.5 text-sm border ${
              i === active ? "bg-gray-900 text-white border-gray-900" : "border-gray-300 dark:border-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800"
            }`}
          >
            {it.props.label}
          </button>
        ))}
      </div>
      <div className="mt-4">{items[active]}</div>
    </div>
  );
}

function Tab({ children }: { label: string; children: React.ReactNode }) {
  return <div>{children}</div>;
}

/* --------------- Text Summarizer --------------- */

function TextSummarizer() {
  const [text, setText] = useState("");
  const [title, setTitle] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
 const [sentenceCount, setSentenceCount] = useState(3);


  const charCount = text.length;
  const disabled = loading || !text.trim();


  async function handleSummarize() {
  setError(null);
  setLoading(true);
  setResult(null);
  try {
console.log(`/summarize/text?sentences=${sentenceCount}`);
    const res = await fetch(`/summarize/text?sentences=${sentenceCount}`, {
      method: "POST",
      headers: { "Content-Type": "text/plain" }, // ✅ MUST be text/plain
      body: text, // ✅ Raw text only, NOT JSON
    });
    if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
    const data = await res.json();
    setResult(data);
  } catch (e: any) {
    setError(e.message || "Request failed");
  } finally {
    setLoading(false);
  }
}


  return (
    <SectionCard title="Paste text to summarize" subtitle="Supports articles, emails, reports, and more.">
      <div className="grid gap-3">
        <label className="text-sm">Optional title</label>
        <Input placeholder="e.g., Quarterly update" value={title} onChange={(e) => setTitle(e.target.value)} />
<label className="text-sm">Summary length (sentences)</label>
<Input
  type="number"
  min={1}
  max={20}
  value={sentenceCount}
  onChange={(e) => setSentenceCount(Number(e.target.value))}
/>
        <div className="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
          <label>Text</label>
          <span>{charCount.toLocaleString()} characters</span>
        </div>
        <Textarea placeholder="Paste your text here..." value={text} onChange={(e) => setText(e.target.value)} />
        <div className="flex items-center gap-2">
          <Button onClick={handleSummarize} disabled={disabled}>{loading ? "Summarizing..." : "Summarize"}</Button>
          <GhostButton onClick={() => setText("")}>Clear</GhostButton>
        </div>
        {error && <ErrorBanner message={error} />}
        {result && <SummaryResult result={result} />}
      </div>
    </SectionCard>
  );
}

/* --------------- Document Summarizer --------------- */

function DocumentSummarizer() {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  const accept = ".pdf,.png,.docx";

  function onDrop(e: React.DragEvent) {
    e.preventDefault();
    setDragOver(false);
    const f = e.dataTransfer.files?.[0];
    if (f) setFile(f);
  }

  async function handleUpload() {
    if (!file) return;
    setError(null);
    setLoading(true);
    setResult(null);
    try {
      const form = new FormData();
      form.append("document", file);
      const res = await fetch(`/summarize/document`, { method: "POST", body: form });
      if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
      const data = await res.json();
      setResult(data);
    } catch (e: any) {
      setError(e.message || "Upload failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <SectionCard title="Upload a document" subtitle="Accepted: .pdf, .png, .docx (more formats coming).">
      <div className="grid gap-4">
        <div
          onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
          onDragLeave={() => setDragOver(false)}
          onDrop={onDrop}
          className={`rounded-2xl border-2 border-dashed p-6 text-center transition ${
            dragOver ? "border-indigo-400 bg-indigo-50 dark:bg-indigo-950/20" : "border-gray-300 dark:border-gray-700"
          }`}
        >
          <p className="text-sm">Drag and drop a file here</p>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">or</p>
          <div className="mt-3">
            <GhostButton onClick={() => inputRef.current?.click()}>Choose a file</GhostButton>
            <input
              ref={inputRef}
              type="file"
              accept={accept}
              className="hidden"
              onChange={(e) => setFile(e.target.files?.[0] || null)}
            />
          </div>
          {file && <p className="mt-3 text-sm">Selected: <span className="font-medium">{file.name}</span></p>}
        </div>
        <div className="flex items-center gap-2">
          <Button onClick={handleUpload} disabled={loading || !file}>{loading ? "Summarizing..." : "Summarize"}</Button>
          {file && <GhostButton onClick={() => setFile(null)}>Remove file</GhostButton>}
        </div>
        {error && <ErrorBanner message={error} />}
        {result && <SummaryResult result={result} />}
      </div>
    </SectionCard>
  );
}

/* --------------- Result rendering --------------- */

function SummaryResult({ result }: { result: any }) {
  if (!result) return null;

  // Extract data from nested "analysis" object if present
  const analysis = result.analysis ?? result;
  const summary = analysis.summaryText || analysis.summary || "No summary available";
  const bullets = extractBullets(analysis);
  const keywords = extractKeywords(analysis);

  return (
    <div className="mt-3 grid gap-3">
      <div className="flex items-center justify-between">
        <h3 className="text-base font-semibold">Summary</h3>
        <div className="flex items-center gap-2">
          <ClipboardButton text={summary} />
          <DownloadJsonButton data={result} />
        </div>
      </div>

      <pre className="whitespace-pre-wrap rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-4 text-sm">
        {summary}
      </pre>

      {bullets && bullets.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold mb-2">Key points</h4>
          <ul className="list-disc pl-5 space-y-1 text-sm">
            {bullets.map((b, i) => (
              <li key={i}>{b}</li>
            ))}
          </ul>
        </div>
      )}

      {keywords && keywords.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold mb-2">Keywords</h4>
          <div className="flex flex-wrap gap-2">
            {keywords.map((k, i) => (
              <span key={i} className="px-3 py-1 text-sm rounded-xl bg-indigo-100 dark:bg-indigo-900 text-indigo-900 dark:text-indigo-100">
                {k}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}


function extractBullets(result: any): string[] | null {
  if (!result) return null;
  if (Array.isArray(result?.bullets)) return result.bullets;
  if (typeof result === "object") {
    const maybe = result?.summary?.bullets || result?.points || null;
    if (Array.isArray(maybe)) return maybe;
  }
  return null;
}

function ErrorBanner({ message }: { message: string }) {
  return (
    <div className="rounded-xl border border-red-200 bg-red-50 text-red-800 p-3 text-sm dark:bg-red-950/20 dark:border-red-900 dark:text-red-200">
      {message}
    </div>
  );
}

function FAQ() {
  return (
    <section className="mt-12 grid gap-4">
      <SectionCard title="How it works" subtitle="Friendly UI on top of your existing API endpoints.">
        <ol className="list-decimal pl-6 space-y-2 text-sm">
          <li>Paste text or upload a file.</li>
          <li>
            We send it to <code className="px-1 rounded bg-gray-100 dark:bg-gray-800">/summarize/text</code> or{" "}
            <code className="px-1 rounded bg-gray-100 dark:bg-gray-800">/summarize/document</code>.
          </li>
          <li>We display the returned summary and optional key points.</li>
        </ol>
        <div className="mt-4 text-sm text-gray-600 dark:text-gray-300">
          <p>Accepted file types: .pdf, .png, .docx.</p>
          <p className="mt-1">Swagger is available at <code>/swagger</code>.</p>
        </div>
      </SectionCard>
    </section>
  );
}


function extractKeywords(result: any): string[] | null {
  if (!result) return null;
  if (Array.isArray(result?.keywords)) return result.keywords;
  if (typeof result === "object") {
    const maybe = result?.summary?.keywords || result?.tags || null;
    if (Array.isArray(maybe)) return maybe;
  }
  return null;
}
