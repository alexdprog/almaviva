# Almaviva Slot Checker (Blazor Server)

Blazor Server приложение для:
1. OAuth-логина через редирект на `https://visa.almaviva-russia.ru/`.
2. Проверки доступности слотов (обычно `false`).
3. Сохранения аудита логинов и проверок в PostgreSQL.

## Запуск

```bash
dotnet restore
dotnet run
```

## Настройка OAuth

В `appsettings.json` заполните секцию `OAuth`:
- `ClientId`
- `ClientSecret`
- при необходимости скорректируйте `AuthorizeEndpoint`, `TokenEndpoint`, `UserInfoEndpoint`.

`CallbackPath` по умолчанию: `/auth/callback`.
Этот URI должен быть зарегистрирован в OAuth-клиенте Almaviva.

## Поток логина

- Страница `/login` отправляет пользователя на `/auth/login`.
- `/auth/login` делает редирект на OAuth-провайдер Almaviva.
- После авторизации провайдер возвращает на `/auth/callback`.
- На callback приложение меняет `code` на access token, сохраняет аудит и возвращает пользователя обратно на `/login`.
