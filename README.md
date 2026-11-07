# TriadIdentity

Сервис идентификации на ASP.NET Core Identity + JWT: регистрация, вход, профиль, роли, журнал действий.

## Структура решения

| Слой | Проект | Назначение |
|---|---|---|
| DAL | `TriadIdentity.DAL.Entities` | Сущности: `User`, `Role`, `LogEntry` и др. |
| DAL | `TriadIdentity.DAL.Interfaces` | `IRepository`, `IUnitOfWork`, `IApplicationDbContext` |
| DAL | `TriadIdentity.DAL` | EF Core `ApplicationDbContext`, миграции, репозитории |
| BLL | `TriadIdentity.BLL.Models` | DTO, константы ошибок |
| BLL | `TriadIdentity.BLL.Interfaces` | Интерфейсы сервисов |
| BLL | `TriadIdentity.BLL.Identity` | `CustomUserManager`, `CustomRoleManager` |
| BLL | `TriadIdentity.BLL` | Реализации сервисов, AutoMapper-профиль |
| UI | `TriadIdentity.UI.Api` | Web API, JWT, Swagger |
| UI | `TriadIdentity.Infrastructure` | HTTP-клиент к API (`ApiClientService`) |

## Текущее состояние (2026-11-08)

- Сборка проходит, уязвимых NuGet-пакетов нет.
- .NET 10, пакеты `Microsoft.*` 10.0.14, Swashbuckle 10.3.1, AutoMapper 15.1.3.
- Работает без Aspire, с Aspire и в Docker.
- Удалены устаревшие пакеты: `SwaggerUi 1.1.0`, `Microsoft.AspNetCore.Identity 2.x`, `Microsoft.AspNetCore.Http.Extensions 2.x`.
- Рефакторинг `IdentityService` → `IAuthService` / `IUserService` / `IRoleService` / `IClaimService` начат, но не закончен.
- Известные баги: см. этап 2.

---

# План работ

## Этап 0. Зафиксировать состояние
- [ ] Просмотреть `git diff`, закоммитить текущие правки отдельным коммитом.
- [ ] Запушить в `origin/master`.

## Этап 1. Доделать рефакторинг сервисов
- [ ] Оставить один `ILogService` в `Interfaces/Common`, объединив методы (`GetUserLogsAsync`, `GetAllLogsAsync`). Удалить `Interfaces/Identity/ILogService.cs` и алиасы `using ILogService = ...` в `IdentityService.cs` и `BLL/Extensions/ServiceCollectionExtensions.cs`.
- [ ] Реализовать `AuthService`, `UserService`, `RoleService`, `ClaimService`, перенести в них код из `IdentityService`.
- [ ] Разбить `IdentityController` на `AuthController`, `UsersController`, `RolesController`, `ClaimsController`.
- [ ] Удалить `IdentityService` / `IIdentityService`.
- [ ] `ExternalLoginAsync` убрать из `IAuthService` до этапа 5.

## Этап 2. Критические баги
- [ ] **Login не работает.** `IdentityController.Login` вызывает `GetUserByIdAsync(dto.Email)`, а метод ждёт Guid. Использовать id, который возвращает `LoginUserAsync`.
- [ ] **Глобальная обработка ошибок.** Добавить `GlobalExceptionHandler : IExceptionHandler`: `ValidationException` → 400, `UnauthorizedAccessException` → 403, `KeyNotFoundException` → 404, остальное → 500 без деталей наружу. Подключить через `AddExceptionHandler<>()`, `AddProblemDetails()`, `app.UseExceptionHandler()` первым в конвейере. Убрать `try/catch` из контроллеров.
- [ ] В `Update` вместо `Unauthorized(...)` возвращать `Forbid()`.
- [ ] **Маршруты клиента.** В `ApiClientService` убрать `{userId}` из `profile`, `roles`, `logs`: API берёт id из JWT.
- [ ] **JWT в клиенте.** Добавить `ITokenStorage`, `AuthTokenHandler : DelegatingHandler` (заголовок `Authorization: Bearer`), `LoginAsync` возвращает `TokenResponse` и сохраняет токен. Регистрация: `AddHttpClient<ApiClientService>().AddHttpMessageHandler<AuthTokenHandler>()`.
- [ ] Проверка в Swagger: register → login → profile; неверный пароль → 400, без токена → 401, чужой id → 403.

## Этап 3. Безопасность
- [ ] JWT-ключ убрать из `appsettings.json` (`dotnet user-secrets` для разработки, переменные окружения для прода). Текущий ключ-заглушку считать скомпрометированным.
- [ ] `lockoutOnFailure: true` в `PasswordSignInAsync`.
- [ ] Rate limiting на `/login` и `/register` (`Microsoft.AspNetCore.RateLimiting`).
- [ ] Явная CORS-политика.
- [ ] Убрать предупреждения nullable (CS8618): `required` или `= null!` в DTO и сущностях.

## Этап 4. Бэкенд для продакшна
- [x] Переход на .NET 10 (см. раздел ниже).
- [ ] Refresh-токены: таблица `RefreshTokens`, `/refresh`, `/logout`, миграция.
- [ ] Пагинация для `/logs`.
- [ ] `/health`, Serilog.
- [x] Оркестрация для локального запуска: .NET Aspire (см. раздел ниже).
- [ ] Тесты: `TriadIdentity.Tests` (xUnit), юнит-тесты сервисов, интеграционные через `WebApplicationFactory`.
- [x] README: запуск, миграции, переменные окружения.

## Этап 5. Новые фичи
- [ ] Подтверждение email.
- [ ] Сброс и смена пароля.
- [ ] Админские эндпоинты ролей и claims (`[Authorize(Roles = "Admin")]`), seed роли Admin.
- [ ] 2FA (TOTP).
- [ ] Внешний вход (Google / Microsoft) — `ExternalLoginAsync`.
- [ ] Версионирование API (`/api/v1`).

---

# Запуск

Требуется .NET SDK 10. Один раз создать секреты:

```bash
powershell -ExecutionPolicy Bypass -File scripts/init-secrets.ps1
```

## Без Aspire (Windows, LocalDB)

```bash
dotnet run --project UI/Backend/TriadIdentity.UI.Api
```

В Development миграции применяются при старте. Swagger: `https://localhost:<порт>/swagger`.

## С Aspire (нужен Docker Desktop)

```bash
dotnet run --project UI/TriadIdentity.AppHost
```

SQL Server поднимается в контейнере, строка подключения и `Jwt__Key` передаются в API автоматически. Ссылка на дашборд выводится в консоль.

## Docker

Из корня репозитория:

```bash
docker build -f UI/Backend/TriadIdentity.UI.Api/Dockerfile -t triadidentity:local .
docker run --rm -p 8080:8080 -e Jwt__Key="<ключ не короче 32 символов>" -e ConnectionStrings__DefaultConnection="<строка подключения>" triadidentity:local
```

Без `Jwt__Key` контейнер не стартует. В контейнере LocalDB недоступна, нужен внешний SQL Server. Миграции в контейнере (Production) при старте не применяются.

## Переменные окружения

| Переменная | Назначение |
|---|---|
| `Jwt__Key` | Ключ подписи JWT, обязателен |
| `ConnectionStrings__DefaultConnection` | Строка подключения к SQL Server |
| `ASPNETCORE_ENVIRONMENT` | `Development` включает Swagger и миграции при старте |

---

# Использование как шаблона

## Новый проект

Один раз установить шаблон из корня этого репозитория:

```bash
dotnet new install .
```

Создать проект (имя без дефисов и пробелов, оно станет namespace):

```bash
dotnet new triadidentity -n MyProject
```

Затем в папке нового проекта:

```bash
powershell -ExecutionPolicy Bypass -File scripts/init-secrets.ps1
dotnet ef database update --project DAL/MyProject.DAL --startup-project UI/Backend/MyProject.UI.Api
dotnet run --project UI/Backend/MyProject.UI.Api
```

## Настройки JWT

| Параметр | Где хранится | Секрет |
|---|---|---|
| `Jwt:Key` | user-secrets, на сервере `Jwt__Key` | Да |
| `Jwt:Issuer`, `Jwt:Audience` | `appsettings.json` | Нет |
| `Jwt:ExpiryMinutes` | `appsettings.json` | Нет |

Ключ должен быть не короче 32 символов и свой для каждого проекта и окружения.
Без него приложение не запустится. Никогда не коммитьте ключ в репозиторий.

---

# Переход на .NET 10 (LTS)

**Зачем:** поддержка .NET 9 (STS) заканчивается около 10.11.2026. .NET 10 — LTS. SDK 10.0.403 уже установлен.

**Когда:** после этапа 2, когда логин и обработка ошибок работают и есть чем проверить, что ничего не сломалось.

### Шаги
1. Отдельная ветка: `git switch -c upgrade/net10`.
2. Во всех `.csproj`: `<TargetFramework>net9.0</TargetFramework>` → `net10.0`.
   Удобнее вынести в `Directory.Build.props` в корне, чтобы версия была в одном месте:
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
     </PropertyGroup>
   </Project>
   ```
   и убрать эти свойства из отдельных `.csproj`.
3. Пакеты `Microsoft.*` 9.0.22 → 10.0.x (на 2026-10-29 последняя 10.0.14). Можно перейти на Central Package Management (`Directory.Packages.props`), чтобы версии были в одном файле.
4. Пакеты со сменой мажорной версии — читать release notes:
   - AutoMapper 15 → 16;
   - Swashbuckle 9 → 10. Альтернатива: встроенный `Microsoft.AspNetCore.OpenApi` (`AddOpenApi()` / `MapOpenApi()`) + Scalar или Swagger UI.
   - `Microsoft.Extensions.DependencyInjection.AutoActivation` 9.10 → 10.x.
5. `dotnet tool update --global dotnet-ef`, затем `dotnet ef migrations add Net10Upgrade` — проверить, что миграция пустая или содержит только ожидаемые изменения модели.
6. `dotnet build`, затем `dotnet list package --vulnerable --include-transitive`.
7. Прогнать тесты и ручной сценарий register → login → profile.
8. Обновить `Dockerfile`, если есть: базовые образы `mcr.microsoft.com/dotnet/aspnet:10.0` и `sdk:10.0`.
9. Merge в `master`.

---

# .NET Aspire: стоит ли переходить

**Коротко:** да, но как замену docker-compose для локальной разработки, а не как смену архитектуры. Слои DAL / BLL / UI остаются как есть.

### Что даёт
- Один запуск (`F5` на AppHost) поднимает API и SQL Server в контейнере вместо `localdb`, который работает только на Windows.
- Строка подключения пробрасывается в API автоматически, её не нужно хранить в `appsettings.json`.
- Дашборд с логами, трейсами и метриками (OpenTelemetry) без настройки Serilog и отдельных инструментов.
- Health checks, resilience для `HttpClient`, service discovery — через проект `ServiceDefaults`.
- Когда появится фронтенд или второй сервис, он добавляется одной строкой в AppHost.

### Чего стоит
- Два новых проекта: `TriadIdentity.AppHost` и `TriadIdentity.ServiceDefaults`.
- Нужен Docker Desktop (или Podman) на машине разработчика.
- Ещё одна технология в стеке; для одного API без зависимостей выигрыш небольшой. Выигрыш растёт с каждым новым сервисом.

### Как встроить
1. После перехода на .NET 10.
2. `dotnet new aspire-apphost -n TriadIdentity.AppHost` и `dotnet new aspire-servicedefaults -n TriadIdentity.ServiceDefaults`, добавить в решение.
3. В `TriadIdentity.UI.Api`: ссылка на `ServiceDefaults`, в `Program.cs` — `builder.AddServiceDefaults();` и `app.MapDefaultEndpoints();`.
4. В AppHost:
   ```csharp
   var builder = DistributedApplication.CreateBuilder(args);

   var sql = builder.AddSqlServer("sql")
                    .WithDataVolume()
                    .AddDatabase("TriadIdentity");

   builder.AddProject<Projects.TriadIdentity_UI_Api>("api")
          .WithReference(sql)
          .WaitFor(sql);

   builder.Build().Run();
   ```
5. В API строка подключения берётся по имени ресурса: `GetConnectionString("TriadIdentity")` (или `builder.AddSqlServerDbContext<ApplicationDbContext>("TriadIdentity")` из пакета `Aspire.Microsoft.EntityFrameworkCore.SqlServer`).
6. Миграции при старте в Development, или отдельный проект-мигратор (worker), который AppHost запускает перед API.
7. Пункты этапа 4 «Serilog» и «/health» частично закрываются `ServiceDefaults` — пересмотреть их после внедрения.
