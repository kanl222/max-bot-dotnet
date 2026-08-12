# MAX.Bot — .NET клиент для API мессенджера MAX

[![NuGet](https://img.shields.io/nuget/v/SaaSoft.MAX.Bot)](https://www.nuget.org/packages/SaaSoft.MAX.Bot)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SaaSoft.MAX.Bot)](https://www.nuget.org/packages/SaaSoft.MAX.Bot)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Библиотека для создания ботов в мессенджере [MAX](https://max.ru/) на платформе .NET. Предоставляет полный набор методов для работы с сообщениями, чатами, участниками, вложениями и событиями в реальном времени.

## Возможности

- 📨 **Сообщения** — отправка, редактирование, удаление, получение
- 💬 **Чаты** — управление групповыми чатами, закреплённые сообщения, действия бота
- 👥 **Участники** — управление пользователями и администраторами
- 📎 **Вложения** — загрузка файлов, изображений, видео, аудио; скачивание вложений
- 🔔 **События** — long polling с типизированными событиями
- 🔗 **Webhook** — управление подписками
- ⚙️ **DI** — готовая интеграция с `Microsoft.Extensions.DependencyInjection`
- 🔁 **Rate limiting** — автоматические повторы при HTTP 429

## Установка

```bash
dotnet add package SaaSoft.MAX.Bot
```

Или через NuGet Package Manager:

```
Install-Package SaaSoft.MAX.Bot
```

### Поддерживаемые платформы

`net8.0` · `net9.0` · `net10.0` · `net462`

### Сертификаты Минцифры

API MAX работает на `https://platform-api2.max.ru` и использует TLS-сертификаты Национального удостоверяющего центра Минцифры. Без их установки запросы будут завершаться ошибкой SSL/TLS.

Установите сертификаты на машине, где запускается бот. Инструкция и файлы — на [госуслугах](https://www.gosuslugi.ru/crt).

## Быстрый старт

Получите токен бота в [Личном кабинете разработчика MAX](https://dev.max.ru).

```csharp
using MAX.Bot;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request.Message;

var bot = new MaxBotClient("YOUR_BOT_TOKEN");

// Информация о боте
var me = await bot.GetMeAsync();
Console.WriteLine($"Бот: {me.FirstName} (ID: {me.Id})");

// Подписка на входящие сообщения
bot.OnMessageCreated += async (update, client) =>
{
    var text = update.Message?.Body?.Text;
    var chatId = update.Message?.Recipient.ChatId ?? 0;

    await client.SendMessageAsync(new SendMessageRequest
    {
        ChatId = chatId,
        Text = $"Вы написали: {text}",
    });
};

// Запуск polling в фоне
bot.StartPolling(limit: 100, timeout: 90);

Console.ReadLine();
bot.StopPolling();
```

## Способы создания клиента

### Простой конструктор

```csharp
// Базовый — таймаут 30 секунд по умолчанию
var bot = new MaxBotClient("your_token");

// С кастомным таймаутом и настройками retry для вложений
var bot = new MaxBotClient("your_token",
    timeoutSeconds: 60,
    attachmentRetryOptions: new AttachmentRetryOptions
    {
        MaxAttempts = 5,
        RetryDelay = TimeSpan.FromSeconds(3),
    });
```

### Dependency Injection (ASP.NET Core)

```csharp
// Program.cs
using MAX.Bot.Extensions;

builder.Services.AddMaxBotClient(
    token: builder.Configuration["MaxBot:Token"]!,
    timeoutSeconds: 60);
```

```csharp
// В сервисе
public class BotService(IMaxBotClient bot)
{
    public async Task SendGreeting(long chatId)
    {
        await bot.SendMessageAsync(new SendMessageRequest
        {
            ChatId = chatId,
            Text = "Добро пожаловать! 👋",
        });
    }
}
```

## Работа с событиями

Все события принимают `async`-обработчики. Если зарегистрировано несколько обработчиков — они вызываются параллельно.

```csharp
// Любое входящее обновление
bot.OnUpdate += async (update, client) => { ... };

// Новое сообщение
bot.OnMessageCreated += async (update, client) => { ... };

// Редактирование сообщения
bot.OnMessageEdited += async (update, client) => { ... };

// Удаление сообщения
bot.OnMessageRemoved += async (update, client) => { ... };

// Нажата кнопка inline-клавиатуры
bot.OnMessageCallback += async (update, client) => { ... };

// Бот добавлен в чат / запущен / остановлен / удалён из чата
bot.OnBotAddedToChat += async (update, client) => { ... };
bot.OnBotStarted     += async (update, client) => { ... };
bot.OnBotStopped     += async (update, client) => { ... };
bot.OnBotRemovedFromChat += async (update, client) => { ... };

// Изменения в чате
bot.OnChatTitleChanged    += async (update, client) => { ... };
bot.OnUserAddedToChat     += async (update, client) => { ... };
bot.OnUserRemovedFromChat += async (update, client) => { ... };

// Диалог
bot.OnDialogCleared   += async (update, client) => { ... };
bot.OnDialogMuted     += async (update, client) => { ... };
bot.OnDialogUnmuted   += async (update, client) => { ... };
bot.OnDialogRemoved   += async (update, client) => { ... };

// Ошибки в процессе polling
bot.OnPollingError += async (ex) => { Console.Error.WriteLine(ex); };
```

## Отправка вложений

### Inline-клавиатура

```csharp
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;

await bot.SendMessageAsync(new SendMessageRequest
{
    ChatId = chatId,
    Text = "Выберите действие:",
    Attachments =
    [
        new InlineKeyboardAttachment
        {
            Payload = new InlineKeyboardPayload
            {
                Buttons =
                [
                    [ new LinkButton     { Text = "Перейти",    Url  = "https://max.ru" } ],
                    [ new CallbackButton { Text = "Подтвердить", Payload = "confirm" } ],
                    [ new RequestGeoButton { Text = "Отправить геолокацию", Quick = true } ],
                ]
            }
        }
    ]
});
```

### Загрузка файла и отправка

```csharp
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;

// 1. Загрузить файл и получить токен
await using var stream = File.OpenRead("document.pdf");
var token = await bot.UploadAsync(new UploadRequest
{
    Type = UploadType.File,
    Content = stream,
    FileName = "document.pdf",
    ContentType = "application/pdf",
});

// 2. Отправить сообщение с вложением
await bot.SendMessageAsync(new SendMessageRequest
{
    ChatId = chatId,
    Text = "Документ",
    Attachments =
    [
        new FileAttachment { Payload = new FilePayload { Token = token } }
    ]
});
```

### Скачивание вложений из сообщения

```csharp
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Response;

var message = await bot.GetMessageByIdAsync("message_id");

foreach (var attachment in message.Body?.Attachments ?? [])
{
    try
    {
        // Сохранить в файл
        var result = await bot.DownloadAttachmentAsync(attachment, new DownloadAttachmentOptions
        {
            FilePath = Path.Combine("downloads", message.Body!.Mid!),
            VideoQuality = VideoQuality.Mp4_720,
        });
        Console.WriteLine($"Сохранено: {result.SavedFilePath}");

        // Или получить байты в памяти
        // var result = await bot.DownloadAttachmentAsync(attachment, new DownloadAttachmentOptions());
        // byte[] content = result.Content!;
    }
    catch (NotSupportedException)
    {
        // Sticker, inline-keyboard и др. — скачивание не поддерживается
    }
}
```

## Управление чатами

```csharp
// Список чатов
var chats = await bot.GetChatsAsync(new GetChatsRequest { Count = 10 });

// Информация о чате
var chat = await bot.GetChatByIdAsync(chatId);

// Обновление названия
await bot.UpdateChatAsync(chatId, new UpdateChatRequest { Title = "Новое название" });

// Закрепить сообщение
await bot.PinChatMessageAsync(chatId, new PinChatMessageRequest
{
    MessageId = messageId,
    Notify = true,
});

// Открепить сообщение
await bot.UnpinChatMessageAsync(chatId);

// Действие бота (например, "печатает...")
await bot.SendChatActionAsync(chatId, new SendChatActionRequest
{
    Action = SenderActions.TypingOn,
});
```

## Управление участниками

```csharp
// Список участников
var members = await bot.GetChatMembersAsync(new GetChatMembersRequest { ChatId = chatId });

// Добавить пользователя
await bot.AddChatMemberAsync(new AddChatMemberRequest
{
    ChatId = chatId,
    UserIds = [userId],
});

// Удалить пользователя
await bot.DeleteChatMemberAsync(new DeleteChatMemberRequest
{
    ChatId = chatId,
    UserId = userId,
});

// Назначить администратора
await bot.AddChatAdminsAsync(chatId, new AddChatAdminsRequest
{
    Admins =
    [
        new ChatAdmin
        {
            UserId = userId,
            Permissions = [ChatAdminPermission.Write, ChatAdminPermission.PinMessage],
            Alias = "Модератор",
        }
    ]
});
```

## Webhook

```csharp
// Создать подписку
await bot.SubscribeAsync(new SubscriptionRequest
{
    Url = "https://your-domain.com/webhook",
    UpdateTypes = [UpdateTypes.MessageCreated, UpdateTypes.MessageCallback],
    Secret = "your-secret",
});

// Список подписок
var subscriptions = await bot.GetSubscriptionsAsync();

// Удалить подписку
await bot.UnsubscribeAsync(new DeleteSubscriptionRequest
{
    Url = "https://your-domain.com/webhook",
});
```

## API-методы

### Легенда

| Статус | Значение |
|--------|----------|
| ✅ | Реализовано |
| 🚧 | Частично |
| ❌ | Не реализовано |

### 🤖 Bot

| Метод | Описание | Статус |
|-------|----------|--------|
| `GET /me` | Информация о боте | ✅ |

### ✉️ Messages

| Метод | Описание | Статус |
|-------|----------|--------|
| `GET /messages` | Получить сообщения | ✅ |
| `POST /messages` | Отправить сообщение | ✅ |
| `PUT /messages` | Редактировать сообщение | ✅ |
| `DELETE /messages` | Удалить сообщение | ✅ |
| `GET /messages/{id}` | Получить сообщение по ID | ✅ |
| `GET /videos/{token}` | Информация о видео | ✅ |
| `POST /answers` | Ответить на callback | ✅ |

### 💬 Chats

| Метод | Описание | Статус |
|-------|----------|--------|
| `GET /chats` | Список чатов | ✅ |
| `GET /chats/{id}` | Информация о чате | ✅ |
| `PATCH /chats/{id}` | Обновить чат | ✅ |
| `DELETE /chats/{id}` | Удалить чат | ✅ |
| `POST /chats/{id}/actions` | Действие бота | ✅ |
| `GET /chats/{id}/pin` | Закреплённое сообщение | ✅ |
| `PUT /chats/{id}/pin` | Закрепить сообщение | ✅ |
| `DELETE /chats/{id}/pin` | Открепить сообщение | ✅ |
| `GET /chats/{id}/members/me` | Членство бота | ✅ |
| `DELETE /chats/{id}/members/me` | Покинуть чат | ✅ |
| `GET /chats/{id}/members/admins` | Список администраторов | ✅ |
| `POST /chats/{id}/members/admins` | Назначить администратора | ✅ |
| `DELETE /chats/{id}/members/admins/{userId}` | Снять права администратора | ✅ |
| `GET /chats/{id}/members` | Список участников | ✅ |
| `POST /chats/{id}/members` | Добавить участников | ✅ |
| `DELETE /chats/{id}/members` | Удалить участника | ✅ |

### 📡 Subscriptions

| Метод | Описание | Статус |
|-------|----------|--------|
| `GET /subscriptions` | Список подписок | ✅ |
| `POST /subscriptions` | Создать подписку | ✅ |
| `DELETE /subscriptions` | Удалить подписку | ✅ |
| `GET /updates` | Получить обновления | ✅ |

### 📁 Upload

| Метод | Описание | Статус |
|-------|----------|--------|
| `POST /uploads` | Загрузить файл | ✅ |

## Лицензия

MIT © 2026 kanl
