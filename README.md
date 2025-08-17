# BankAccounts API

Решение четвертого задания
---

## Аутентификация через Keycloak

При запуске `docker-compose` автоматически импортируется realm из папки `./keycloak/import`.
При первом запуске контенера `postgres` автоматически импортируется скрипт `init_accrue_interest.sql` из папки `./postgres/initdb` и создается процедура `accrue_interest(p_account_id UUID)`.

Для авторизации в Swagger нажмите **Authorize** (в правом верхнем углу Swagger UI) и введите следующие параметры:

- **Client ID:** `swagger-client`
- **Flow:** implicit (автоматически выбран)
- **Scope:** `openid profile`

Вход в Keycloak:

- **Username:** `user1`
- **Password:** `user1`
 
## Hangfire

Для подключения к Dashboard Hangfire используйте:`http://localhost/hangfire`

## RabbitMq

Доступен по `http://localhost:15672`

## Tests

Запуск тестов по пути `./BankAccountsTests` `dotnet test`
