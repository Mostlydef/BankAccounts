# BankAccounts API
Решение второго задания
---

## Аутентификация через Keycloak

При запуске docker-compose автоматически импортируется realm из папки ./keycloak/import.

Для авторизации в Swagger нажмите "Authorize" (в правом верхнем углу Swagger UI) и введите следующие параметры:

Client ID: swagger-client
Flow: implicit (автоматически выбран)
Scope: openid profile
Вход в Keycloak:

Username: user1
Password: user1
