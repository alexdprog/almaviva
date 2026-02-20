# Almaviva Slot Checker (.NET 10 + Blazor Server)

Production-ready Blazor Server application for OAuth2 PKCE authorization and automatic/manual checking of available visa appointment slots.

## Features

- OAuth2 Authorization Code Flow + PKCE (server-side flow)
- Callback endpoint: `/auth/callback`
- Access/refresh token handling with proactive refresh (30 seconds before expiry)
- `invalid_grant` handling with automatic sign-out
- Manual check (`Check Now`)
- Automatic check with `BackgroundService` every 60 seconds
- Parallel check protection (`SemaphoreSlim`)
- Telegram notifications when slots are found
- Fluent UI Blazor dashboard with:
  - login button
  - auth indicator
  - start/stop automatic checking
  - live logs
  - found dates list
  - last check indicator
  - status (`Idle / Checking / Error`)

## Configuration

Set values in `appsettings.json`:

```json
"OAuth": {
  "Authority": "https://visaiam.almaviva-russia.ru/realms/oauth2-visaSystem-realm-pkce",
  "ClientId": "aa-visasys-public",
  "Scope": "openid profile offline_access"
},
"Telegram": {
  "BotToken": "<BOT_TOKEN>",
  "ChatId": "<CHAT_ID>"
}
```

## Run

```bash
dotnet restore
dotnet run
```

Open `https://localhost:xxxx`, click **Login**, complete OAuth, and start checks.
