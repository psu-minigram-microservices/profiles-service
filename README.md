# Документ требований к API - Minigram Profiles Service

## Обзор

Данный документ определяет требования к API для проекта profiles-service - микросервисной платформы социальной сети, построенной на базе .NET 10.

## Архитектура

## Версионирование API

- **Стратегия**: Версионирование через URL
- **Базовый путь**: `/api/v{version}`
- **Текущая версия**: v1.0
- **Конвенция маршрутов**: Маршруты автоматически включают версию

## Аутентификация и Авторизация

### Схема аутентификации

- **Тип**: JWT Bearer Token
- **Расположение токена**: Заголовок `Authorization: Bearer {token}`
- **Клaims токена**:
  - `sub` (Subject): ID пользователя (Guid)
  - `email`: Email пользователя

### Защищенные эндпоинты

Все эндпоинты Profile и Relation требуют аутентификацию через JWT токен.

## API Auth Service

### Эндпоинты

#### POST /auth/login

Аутентифицирует пользователя и возвращает JWT токены.

**Тело запроса**:
```json
{
  "email": "user@example.com",
  "password": "string"
}
```

**Ответ** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-string"
}
```

**Ошибки**:
- `400 Bad Request`: Неверное тело запроса
- `401 Unauthorized`: Неверные учетные данные

---

#### POST /auth/register

Регистрирует новую учетную запись пользователя.

**Тело запроса**:
```json
{
  "email": "user@example.com",
  "password": "string"
}
```

**Ответ** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-string"
}
```

**Ошибки**:
- `400 Bad Request`: Неверное тело запроса или формат email
- `409 Conflict`: Пользователь с таким email уже существует

---

#### POST /auth/logout

Выход текущего пользователя (инвалидирует refresh token).

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{}
```

**Ошибки**:
- `401 Unauthorized`: Отсутствует или неверный токен

---

#### POST /auth/refresh

Обновляет JWT токены с использованием refresh token.

**Тело запроса**:
```json
{
  "refreshToken": "guid-string"
}
```

**Ответ** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-guid-string"
}
```

**Ошибки**:
- `400 Bad Request`: Неверное тело запроса
- `401 Unauthorized`: Неверный или истекший refresh token

---

## API Profile Service

### Эндпоинты

#### GET /profiles

Получает пагинированный список всех профилей.

**Параметры запроса**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| page | int | Нет | Номер страницы (0-индексированная) |
| perPage | int | Нет | Элементов на странице |

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{
  "count": 100,
  "data": [
    {
      "id": "guid",
      "name": "string",
      "photoUrl": "string|null"
    }
  ]
}
```

**Ошибки**:
- `401 Unauthorized`: Отсутствует или неверный токен

---

#### GET /profiles/me

Получает профиль текущего пользователя.

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{
  "id": "guid",
  "name": "string",
  "photoUrl": "string|null"
}
```

**Ошибки**:
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Профиль не найден для аутентифицированного пользователя

---

#### GET /profiles/{id}

Получает конкретный профиль по ID.

**Параметры маршрута**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| id | guid | Да | ID профиля |

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{
  "id": "guid",
  "name": "string",
  "photoUrl": "string|null"
}
```

**Ошибки**:
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Профиль не найден

---

#### POST /profiles

Создает новый профиль для аутентифицированного пользователя.

**Тело запроса**:
```json
{
  "name": "string",
  "photoUrl": "string|null"
}
```

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (201 Created):
```json
{
  "id": "guid",
  "name": "string",
  "photoUrl": "string|null"
}
```

**Заголовок Location**: `/profiles/me`

**Ошибки**:
- `400 Bad Request`: Неверное тело запроса
- `401 Unauthorized`: Отсутствует или неверный токен

---

#### PUT /profiles

Обновляет профиль текущего пользователя.

**Тело запроса**:
```json
{
  "name": "string",
  "photoUrl": "string|null"
}
```

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (201 Created):
```json
{
  "id": "guid",
  "name": "string",
  "photoUrl": "string|null"
}
```

**Заголовок Location**: `/profiles/me`

**Ошибки**:
- `400 Bad Request`: Неверное тело запроса
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Профиль не найден для аутентифицированного пользователя

---

### Эндпоинты Relation Service

#### GET /profiles/relation

Получает отношения текущего пользователя на основе статуса и типа.

**Параметры запроса**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| status | enum | Да | `none`, `friend`, `blocked` |
| type | enum | Да | `incoming`, `outgoing` |
| page | int | Нет | Номер страницы (0-индексированная) |
| perPage | int | Нет | Элементов на странице |

**Значения статуса**:
- `none`: Отношения не установлены
- `friend`: Дружеские отношения
- `blocked`: Отношения блокировки

**Значения типа**:
- `incoming`: Отношения, где текущий пользователь - получатель
- `outgoing`: Отношения, где текущий пользователь - отправитель

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{
  "count": 10,
  "data": [
    {
      "id": "guid",
      "name": "string",
      "photoUrl": "string|null"
    }
  ]
}
```

**Ошибки**:
- `400 Bad Request`: Отсутствуют обязательные параметры запроса
- `401 Unauthorized`: Отсутствует или неверный токен

---

#### GET /profiles/relation/{receiverId}

Получает конкретное отношение между пользователями.

**Параметры маршрута**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| receiverId | guid | Да | ID профиля получателя |

**Параметры запроса**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| type | enum | Да | `incoming` или `outgoing` |

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{
  "status": "none|friend|blocked",
  "profile": {
    "id": "guid",
    "name": "string",
    "photoUrl": "string|null"
  }
}
```

**Ошибки**:
- `400 Bad Request`: Отсутствуют обязательные параметры
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Отношение не найдено

---

#### POST /profiles/relation/send/{receiverId}

Отправляет запрос на дружбу другому пользователю.

**Параметры маршрута**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| receiverId | guid | Да | ID профиля получателя |

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{}
```

**Ошибки**:
- `400 Bad Request`: Неверный запрос
- `401 Unauthorized`: Отсутствует или неверный токен
- `409 Conflict`: Отношение уже существует

---

#### POST /profiles/relation/reply/{senderId}

Отвечает на запрос на дружбу от другого пользователя.

**Параметры маршрута**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| senderId | guid | Да | ID профиля отправителя |

**Параметры запроса**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| status | enum | Да | `accepted`, `rejected`, `blocked` |

**Значения статуса**:
- `accepted`: Принимает запрос на дружбу
- `rejected`: Отклоняет запрос на дружбу
- `blocked`: Блокирует отправителя

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{}
```

**Ошибки**:
- `400 Bad Request`: Отсутствуют обязательные параметры или неверный статус
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Отношение не найдено
- `409 Conflict`: Невозможно ответить на неподтвержденное отношение

---

#### DELETE /profiles/relation/{receiverId}

Удаляет отношение между пользователями.

**Параметры маршрута**:
| Параметр | Тип | Обязательный | Описание |
|----------|-----|------------|----------|
| receiverId | guid | Да | ID профиля получателя |

**Заголовки**:
- `Authorization: Bearer {accessToken}`

**Ответ** (200 OK):
```json
{}
```

**Ошибки**:
- `401 Unauthorized`: Отсутствует или неверный токен
- `404 Not Found`: Отношение не найдено

---

## Модели данных

### Базовая модель

Все сущности наследуются от `BaseModel`:

```csharp
public class BaseModel
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### User (Auth Service)

```csharp
public class User : BaseModel
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string Password { get; set; } = string.Empty;
}
```

### RefreshSession (Auth Service)

```csharp
public class RefreshSession : BaseModel
{
    public Guid UserId { get; set; }
    public Guid RefreshToken { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime ExpiresIn { get; set; }
    public User User { get; set; } = null!;
}
```

### Profile (Profile Service)

```csharp
public class Profile : BaseModel
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
```

### Relation (Profile Service)

```csharp
public class Relation : BaseModel
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public tStatus Status { get; set; }
    public virtual Profile Sender { get; set; } = null!;
    public virtual Profile Receiver { get; set; } = null!;
}
```

### tStatus (Profile Service)

```csharp
public enum tStatus
{
    None,
    Friend,
    Blocked
}
```

## Обработка ошибок

### Стандартный ответ об ошибке

```json
{
  "statusCode": 400,
  "message": "The request contains invalid data.",
  "exceptionType": "System.ArgumentNullException",
  "stackTrace": "..."
}
```

### HTTP статус коды

| Код | Описание |
|-----|----------|
| 200 | OK - Запрос успешен |
| 201 | Created - Ресурс успешно создан |
| 400 | Bad Request - Неверные данные запроса |
| 401 | Unauthorized - Требуется аутентификация |
| 403 | Forbidden - Недостаточно прав |
| 404 | Not Found - Ресурс не найден |
| 409 | Conflict - Конфликт ресурса |
| 500 | Internal Server Error - Непредвиденная ошибка |

### Типы исключений

- `EntityNotFoundException`: Ресурс не найден
- `UnauthorizedAccessException`: Требуется аутентификация
- `InvalidOperationException`: Неверное состояние операции
- `ArgumentException`: Неверный аргумент

## Пагинация

### Параметры запроса

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|--------------|----------|
| page | int | null | Номер страницы (0-индексированная) |
| perPage | int | null | Элементов на странице |

### Формат ответа

```json
{
  "count": 100,
  "data": []
}
```

## Соображения безопасности

1. **Хеширование паролей**: Пароли хешируются с использованием `PasswordHasher` из ASP.NET Core Identity
2. **Истечение JWT**: Токены истекают через настраиваемую продолжительность (настраивается через `Jwt.Expiration`)
3. **Ротация refresh token**: Каждый запрос обновления генерирует новый refresh token
4. **CORS**: Должен быть настроен в соответствии с окружением развертывания
5. **HTTPS**: Для продакшн-развертываний следует использовать HTTPS

## Конфигурация

### JWT опции (Auth Service)

```json
{
  "Jwt": {
    "Secret": "required-base64-secret-key",
    "Audience": "required-audience",
    "Issuer": "required-issuer",
    "Expiration": 60
  }
}
```

### Строка подключения к базе данных

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=minigram;Username=postgres;Password=password"
  }
}
```

## Схема базы данных

### База данных Auth

- **Users**: Учетные записи пользователей с email, хешем пароля и статусом верификации
- **RefreshSessions**: Активные refresh token'ы с метаданными

### База данных Profile

- **Profiles**: Профили пользователей, связанные с пользователями auth
- **Relations**: Отношения между пользователями со статусом
- **tStatus**: Справочная таблица для статусов отношений

## Зависимости

- **Фреймворк**: .NET 10
- **База данных**: PostgreSQL
- **ORM**: Entity Framework Core
- **Аутентификация**: JWT Bearer Tokens
- **Версионирование API**: Microsoft.AspNetCore.Mvc.Versioning

## Развертывание

### Docker Compose

Проект включает `docker-compose.yml` для контейнеризованного развертывания с PostgreSQL.

### Процесс сборки

1. Восстановление NuGet пакетов
2. Сборка решения
3. Запуск миграций базы данных
4. Запуск сервисов
