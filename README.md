# FirePorker 🔥🐷

A real-time planning poker app for agile teams. Built with ASP.NET Core 8 and SignalR.

## Features

- **Real-time voting** — See votes appear instantly via SignalR
- **Multiple games** — Host multiple concurrent planning sessions
- **Story management** — Add stories, track estimates
- **Host controls** — Clear votes, accept estimates, remove players
- **No accounts needed** — Cookie-based sessions, jump right in

## Quick Start

### Docker Compose (recommended)

Create a `docker-compose.yml`:

```yaml
services:
  fireporker:
    image: ghcr.io/mkaltner/fireporker:latest
    ports:
      - "8080:8080"
    restart: unless-stopped
```

Then run:

```bash
docker compose up -d
```

App will be available at `http://localhost:8080`

### Docker (standalone)

```bash
docker run -d -p 8080:8080 ghcr.io/mkaltner/fireporker:latest
```

### Build from source

```bash
git clone https://github.com/mkaltner/fireporker.git
cd fireporker
docker build -t fireporker .
docker run -p 8080:8080 fireporker
```

### Local Development

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
cd FirePorker
dotnet run
```

## Configuration

Environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for detailed errors |
| `ASPNETCORE_URLS` | `http://+:8080` | Listen URLs |

## How It Works

1. **Create a game** — Enter your name and game name
2. **Share the link** — Other players join via the game URL
3. **Add stories** — Host adds stories to estimate
4. **Vote** — Everyone picks a point value
5. **Reveal** — When all votes are in, cards flip
6. **Accept** — Host accepts the estimate, move to next story

## Tech Stack

- ASP.NET Core 8
- SignalR (real-time communication)
- Knockout.js (UI bindings)
- Bootstrap 3 (styling)
- In-memory caching (game state)

## Notes

- Game state is **in-memory only** — games are lost on restart
- Games expire after 24 hours of inactivity
- No authentication — anyone with the link can join

## License

MIT
