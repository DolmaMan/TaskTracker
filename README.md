\# Task Tracker - Система управления задачами



Учебный проект для практикантов: веб-приложение для создания, учёта и контроля задач с базовой отчётностью.



\## 📋 Описание проекта



Task Tracker - это REST API для управления задачами, разработанное с использованием ASP.NET Core и Entity Framework Core. Проект демонстрирует применение слоистой (N-Layer) архитектуры и лучших практик разработки на C#.



\### Основные возможности



\- ✅ CRUD операции с задачами

\- 🔍 Фильтрация задач по статусу, исполнителю, дедлайну и тегам

\- 📊 Три типа отчётов: по статусам, просроченным задачам, среднему времени закрытия

\- 🏷️ Система тегов для категоризации задач

\- ⚡ Автоматическое определение просроченных задач

\- 📝 Swagger/OpenAPI документация



\## 🏗️ Архитектура



Проект использует слоистую архитектуру с чётким разделением ответственности:



```

TaskTracker/

├── src/

│   ├── TaskTracker.Domain/          # Доменные модели и бизнес-логика

│   │   ├── Models/                  # User, TaskItem, Tag

│   │   └── Enums/                   # TaskStatus, TaskPriority

│   ├── TaskTracker.Application/     # Сервисы приложения

│   │   ├── Services/                # TaskService, ReportService

│   │   └── DTOs/                    # Data Transfer Objects

│   ├── TaskTracker.Infrastructure/  # Работа с данными

│   │   └── Data/                    # DbContext, миграции

│   └── TaskTracker.API/             # Web API

│       └── Controllers/             # TasksController, ReportsController

└── tests/

&nbsp;   └── TaskTracker.Tests/           # Unit тесты

```



\### Слои приложения



1\. \*\*Domain Layer\*\* - доменные модели, enums, бизнес-правила

2\. \*\*Application Layer\*\* - сервисы, DTOs, валидация, маппинг

3\. \*\*Infrastructure Layer\*\* - EF Core DbContext, работа с SQLite

4\. \*\*API Layer\*\* - контроллеры ASP.NET Core Web API



\## 🚀 Технологический стек



\- \*\*Backend:\*\* ASP.NET Core 8.0 (C#)

\- \*\*ORM:\*\* Entity Framework Core 8.0

\- \*\*Database:\*\* SQLite

\- \*\*API Documentation:\*\* Swagger/OpenAPI

\- \*\*Testing:\*\* xUnit

\- \*\*Version Control:\*\* Git



\## 📦 Требования



\- .NET 8.0 SDK

\- Visual Studio 2022 / VS Code / Rider (опционально)

\- Git



\## 🔧 Установка и запуск



\### 1. Клонирование репозитория



```bash

git clone https://github.com/DolmaMan/TaskTracker.git

cd task-tracker

```



\### 2. Восстановление зависимостей



```bash

dotnet restore

```



\### 3. Применение миграций базы данных



```bash

cd src/TaskTracker.API

dotnet ef database update --project ../TaskTracker.Infrastructure

```



\### 4. Запуск приложения



```bash

dotnet run --project src/TaskTracker.API

```



Приложение будет доступно по адресу:

\- API: `http://localhost:5000`

\- Swagger UI: `http://localhost:5000/swagger`



\## 📚 API Endpoints



\### Tasks API



| Метод  | Endpoint           | Описание                    |

|--------|--------------------|-----------------------------|

| GET    | /api/tasks         | Получить список задач       |

| GET    | /api/tasks/{id}    | Получить задачу по ID       |

| POST   | /api/tasks         | Создать новую задачу        |

| PUT    | /api/tasks/{id}    | Обновить задачу             |

| DELETE | /api/tasks/{id}    | Удалить задачу              |



\### Reports API



| Метод | Endpoint                           | Описание                              |

|-------|-------------------------------------|---------------------------------------|

| GET   | /api/reports/status-summary         | Количество задач по статусам          |

| GET   | /api/reports/overdue-by-assignee    | Просроченные задачи по исполнителям   |

| GET   | /api/reports/avg-completion-time    | Среднее время закрытия задач          |



\### Примеры запросов



\#### Создание задачи



```bash

POST /api/tasks

Content-Type: application/json



{

&nbsp; "title": "Реализовать аутентификацию",

&nbsp; "description": "Добавить JWT аутентификацию в API",

&nbsp; "assigneeId": 1,

&nbsp; "dueDate": "2026-02-20T18:00:00",

&nbsp; "priority": 3,

&nbsp; "tagIds": \[1, 2]

}

```



\#### Фильтрация задач



```bash

GET /api/tasks?status=InProgress\&assigneeId=1\&dueBefore=2026-02-15

```



\## 🗄️ Схема базы данных



\### Таблица Users

\- `Id` (int, PK) - Уникальный идентификатор

\- `Name` (string) - Имя пользователя

\- `Email` (string, unique) - Email адрес



\### Таблица Tasks

\- `Id` (int, PK) - Уникальный идентификатор

\- `Title` (string) - Заголовок задачи

\- `Description` (string, nullable) - Описание

\- `AssigneeId` (int, FK) - ID исполнителя

\- `CreatedAt` (DateTime) - Дата создания

\- `DueDate` (DateTime) - Дедлайн

\- `CompletedAt` (DateTime, nullable) - Дата закрытия

\- `Status` (enum) - New, InProgress, Done

\- `Priority` (enum) - Low(1), Medium(2), High(3)



\### Таблица Tags

\- `Id` (int, PK) - Уникальный идентификатор

\- `Name` (string, unique) - Название тега



\### Таблица TaskTags (связь many-to-many)

\- `TaskId` (int, FK) - ID задачи

\- `TagId` (int, FK) - ID тега



\## 🧪 Тестирование



Запуск unit тестов:



```bash

dotnet test

```



Проект содержит тесты для:

\- TaskService (создание, обновление, фильтрация, удаление)

\- ReportService (отчёты по статусам, просроченным задачам, среднему времени)



\## 📋 Бизнес-правила



\### Статусы задач



1\. \*\*New\*\* - задача создана, работа не начата

2\. \*\*InProgress\*\* - задача в процессе выполнения

3\. \*\*Done\*\* - задача завершена (автоматически заполняется `CompletedAt`)

4\. \*\*Overdue\*\* - вычисляемый статус для просроченных задач



> ⚠️ Статус Overdue не хранится в БД, а вычисляется в runtime



\### Приоритеты



\- \*\*Low (1)\*\* - низкий приоритет

\- \*\*Medium (2)\*\* - средний приоритет

\- \*\*High (3)\*\* - высокий приоритет



\### Валидация



\- `Title` - обязательное поле, 3-200 символов

\- `AssigneeId` - должен существовать в таблице Users

\- `Status` - только допустимые значения (0-2)

\- `Priority` - только допустимые значения (1-3)



\## 🎯 Definition of Done



Задача считается завершённой, если:



1\. ✅ Код написан и соответствует требованиям

2\. ✅ Код прошёл code review

3\. ✅ Написаны unit тесты

4\. ✅ Все тесты выполняются успешно

5\. ✅ Обновлена документация

6\. ✅ Функция протестирована вручную

7\. ✅ Изменения слиты в main через pull request



\## 👥 Роли в команде



\- \*\*Backend Developer 1\*\* - Domain models, EF Core, миграции, бизнес-логика

\- \*\*Backend Developer 2\*\* - Controllers, DTOs, валидация, Swagger

\- \*\*Frontend Developer\*\* - UI, формы, интеграция с API

\- \*\*QA/DevOps\*\* - Тестирование, чек-листы, сборка, CI/CD



\## 📅 Планирование спринтов



\### Sprint 1 (Неделя 1) 

\- Core CRUD

\- Базовая структура проекта

\- CRUD операции через API

\- Миграции БД

\- README и DoD



\### Sprint 2 (Неделя 2) 

\- Фильтры и отчёты

\- Фильтрация задач

\- Статус Overdue

\- Базовые отчёты

\- Unit тесты



\### Sprint 3 (Неделя 3) 

\- UI и финализация

\- Веб-интерфейс

\- Визуализация отчётов

\- Полировка и тестирование

\- Демо-презентация



\## 🔒 Безопасность



\- Валидация всех входных данных

\- Параметризованные запросы (автоматически через EF Core)

\- Обработка исключений без раскрытия деталей реализации



> ℹ️ Авторизация и аутентификация на данном этапе опциональны



\## 📄 Лицензия



Учебный проект для практикантов. Использование в образовательных целях.



---



\*\*Версия:\*\* 1.0  

\*\*Дата:\*\* 04.02.2026  

\*\*Срок реализации:\*\* 3 недели (3 спринта по 1 неделе)

