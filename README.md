# Notify

A REST API built with .NET 9 that manages notes with AI-generated tags and on-demand summaries, using the Groq API (Llama 3.3 70B). Includes a minimal frontend served directly from the API.

Built as a technical challenge after an interview at Sparksoft. The focus was on process, justified decisions, and conscious use of AI — not on complexity.

---

## Tech Stack

- **.NET 9** — Minimal API
- **EF Core + SQLite** — persistence, code-first with migrations
- **Groq API** (Llama 3.3 70B, OpenAI-compatible) — tag generation and summarization
- **HTML/CSS/JS** — frontend served from `wwwroot/`

---

## How to Run

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- A [Groq API key](https://console.groq.com) (free, no credit card required)
- `dotnet-ef` tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/eduardoareias/Notify.git
   cd Notify
   ```

2. Set your Groq API key using user-secrets (never stored in the repo):
   ```bash
   dotnet user-secrets set "Groq:ApiKey" "your-key-here"
   ```

3. Apply the database migration:
   ```bash
   dotnet ef database update
   ```

4. Run the API:
   ```bash
   dotnet run
   ```

5. Open the UI at `http://localhost:5109/index.html` or the Swagger docs at `http://localhost:5109/swagger`.

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/notes` | Create a note — tags generated automatically by the LLM |
| `GET` | `/notes` | List all notes |
| `GET` | `/notes/{id}` | Get a note by id |
| `PUT` | `/notes/{id}` | Update a note (title, content, tags) |
| `DELETE` | `/notes/{id}` | Delete a note |
| `POST` | `/notes/{id}/summary` | Generate and save a summary for a note |

---

## Key Decisions

**Minimal API over Controllers** — for an API of this size, Minimal API is the right call. Endpoints are defined directly in `Program.cs`, which keeps the codebase small and readable.

**SQLite + EF Core** — SQLite is lightweight and self-contained. EF Core handles the ORM layer so there's no raw SQL to maintain. Tags are stored as a CSV string (e.g. `"work,productivity"`) and converted back to `List<string>` via a value converter.

**Groq over other providers** — Groq doesn't require a credit card for free tier, and its API is OpenAI-compatible, which makes it easy to swap providers later.

**`ILlmService` abstraction** — the LLM implementation is behind an interface. If the provider changes, only `GroqLlmService` needs updating — the endpoints stay untouched.

**Tags on creation, summary on demand** — tags always add value to a note, so they're generated automatically. Summaries don't always make sense (short notes, lists), so they're generated only when explicitly requested.

**`dotnet user-secrets` for the API key** — the key is stored outside the project directory (`~/.microsoft/usersecrets/`), never in any committed file.

---

## What I'd Do Differently With More Time

- **Input validation** — endpoints currently accept empty strings. A 400 response with a clear message would improve the API contract.
- **Authentication** — the API is fully open. A simple API key in the request header would be a minimum.
- **LLM error handling** — if Groq fails (rate limit, timeout), the endpoint returns a generic 500. The error should be caught and returned with a meaningful response.
- **Pagination** — `GET /notes` returns all notes. With volume, pagination would be necessary.
- **Tests** — no tests were written. Integration tests on the main endpoints would be the priority.

---

## AI Usage

I used Claude (chat) in the early planning phase and Claude Code (VS Code) throughout implementation. The approach was always step-by-step: Claude explained, I validated, and we moved forward.

**Where it helped:**
The LLM integration was territory I hadn't covered before — I had never called an LLM directly from code. Claude walked me through the HttpClient setup, the OpenAI message format, and response deserialization. I came out of it understanding how it works, which was the goal.

The frontend was also built with Claude's help: I defined the layout and behaviour (what appears on each card, how the modal works), and Claude generated the code from those requirements.

**Where I drove the decisions:**
The architectural choices — Minimal API, SQLite, the DTO structure, the behaviour of tags vs. summary — were made by me, often after questioning Claude's first suggestion and understanding the trade-offs. The AI was a collaborator, not a replacement for thinking.

For a more detailed account of the process, see [NOTES.md](NOTES.md).
