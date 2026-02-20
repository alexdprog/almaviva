# Almaviva Slot Checker (Blazor Server)

Blazor Server приложение для:
1. Логина на `https://visa.almaviva-russia.ru/appointment`.
2. Проверки доступности слотов (обычно `false`).
3. Сохранения аудита логинов и проверок в PostgreSQL.

## Запуск

```bash
dotnet restore
dotnet ef database update
dotnet run
```

## Важно

Т.к. endpoint'ы на целевом сайте могут меняться и быть защищены (403/anti-bot),
в `Services/AlmavivaClient.cs` оставлены базовые URL:
- `api/account/login`
- `api/appointment/check`

При необходимости уточните реальные endpoint'ы через browser devtools и обновите сервис.
