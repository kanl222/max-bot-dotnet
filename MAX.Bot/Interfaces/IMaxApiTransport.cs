using MAX.Bot.Interfaces.Models.Attachment;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Абстракция сетевого транспорта для взаимодействия с API мессенджера MAX.
/// Позволяет изолировать бизнес-модули от деталей HTTP-протокола и обеспечивает модульное тестирование (mocking).
/// </summary>
public interface IMaxApiTransport : IDisposable
{
    /// <summary>
    /// Базовый URL API мессенджера MAX.
    /// </summary>
    Uri BaseAddress { get; }

    /// <summary>
    /// Настройки повторных попыток для операций с вложениями.
    /// </summary>
    AttachmentRetryOptions AttachmentRetryOptions { get; }

    /// <summary>
    /// Отправляет типизированный HTTP-запрос к API MAX с сериализацией тела запроса и десериализацией ответа.
    /// </summary>
    /// <typeparam name="T">Тип модели ожидаемого ответа.</typeparam>
    /// <param name="method">HTTP-метод запроса (GET, POST, PUT, DELETE, PATCH).</param>
    /// <param name="endpoint">Относительный путь эндпоинта API (например, "/me" или "/messages").</param>
    /// <param name="data">Необязательный объект данных для сериализации в JSON в теле запроса.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Десериализованный объект ответа типа <typeparamref name="T"/>.</returns>
    /// <exception cref="Exceptions.MaxBotClientException">Выбрасывается при получении неуспешного HTTP-статуса от API MAX.</exception>
    Task<T> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет операцию с повторными попытками при получении статуса «вложение ещё не готово».
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения операции.</typeparam>
    /// <param name="action">Делегат асинхронной операции.</param>
    /// <param name="retryOptions">Пользовательские параметры повторных попыток (или null для параметров по умолчанию).</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Результат выполнения операции типа <typeparamref name="T"/>.</returns>
    Task<T> RetryWhileAttachmentNotReadyAsync<T>(
        Func<Task<T>> action,
        AttachmentRetryOptions? retryOptions,
        CancellationToken cancellationToken);

    /// <summary>
    /// Выполняет отправку составного multipart/form-data запроса для загрузки файла на целевой URL.
    /// </summary>
    /// <param name="uploadUri">Абсолютный URL эндпоинта загрузки.</param>
    /// <param name="content">Содержимое формы с файлом в формате multipart/form-data.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>HTTP-ответ сервера загрузки файлов.</returns>
    /// <exception cref="Exceptions.MaxBotClientException">Выбрасывается при неуспешном коде статуса загрузки.</exception>
    Task<HttpResponseMessage> PostMultipartAsync(
        Uri uploadUri,
        HttpContent content,
        CancellationToken cancellationToken = default);
}
