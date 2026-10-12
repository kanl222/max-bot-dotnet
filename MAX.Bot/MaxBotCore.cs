using System.Net;
using MAX.Bot.Exceptions;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models.Attachment;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot;

/// <summary>
/// Реализация сетевого HTTP-транспорта по умолчанию для API мессенджера MAX.
/// Реализует <see cref="IMaxApiTransport"/>. Не предназначен для прямого использования снаружи библиотеки.
/// </summary>
internal sealed class MaxBotCore : IMaxApiTransport
{
    internal static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowOutOfOrderMetadataProperties = true,
    };

    internal static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Количество попыток при HTTP 429 Too Many Requests.</summary>
    private const int MaxRateLimitRetries = 3;

    /// <summary>Задержка по умолчанию при HTTP 429, если нет Retry-After.</summary>
    private static readonly TimeSpan DefaultRateLimitDelay = TimeSpan.FromSeconds(1);

    /// <summary>Максимально допустимая задержка из заголовка Retry-After для защиты от DoS.</summary>
    private static readonly TimeSpan MaxRetryAfterDelay = TimeSpan.FromSeconds(60);

    internal readonly HttpClient HttpClient;
    internal readonly bool OwnsHttpClient;

    /// <inheritdoc/>
    public Uri BaseAddress => HttpClient.BaseAddress ?? new Uri(MaxBotClient.BaseUrl);

    /// <inheritdoc/>
    public AttachmentRetryOptions AttachmentRetryOptions { get; }

    private bool _disposed;

    /// <summary>
    /// Конструктор с внешним <see cref="System.Net.Http.HttpClient"/> (DI).
    /// </summary>
    /// <param name="token">Токен бота для аутентификации в API MAX.</param>
    /// <param name="httpClient">Экземпляр HTTP-клиента из IHttpClientFactory.</param>
    /// <param name="attachmentRetryOptions">Настройки повторов для вложений.</param>
    internal MaxBotCore(
        string token,
        HttpClient httpClient,
        AttachmentRetryOptions? attachmentRetryOptions)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        OwnsHttpClient = false;

        HttpClient.BaseAddress ??= new Uri(MaxBotClient.BaseUrl);

        if (!HttpClient.DefaultRequestHeaders.Contains("Authorization"))
            HttpClient.DefaultRequestHeaders.Add("Authorization", token);

        AttachmentRetryOptions = attachmentRetryOptions ?? new AttachmentRetryOptions();
    }

    /// <summary>
    /// Конструктор со встроенным <see cref="System.Net.Http.HttpClient"/>.
    /// </summary>
    /// <param name="token">Токен бота для аутентификации в API MAX.</param>
    /// <param name="timeoutSeconds">Таймаут выполнения сетевых запросов в секундах.</param>
    /// <param name="attachmentRetryOptions">Настройки повторов для вложений.</param>
    internal MaxBotCore(
        string token,
        int timeoutSeconds,
        AttachmentRetryOptions? attachmentRetryOptions)
    {
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(MaxBotClient.BaseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        OwnsHttpClient = true;

        HttpClient.DefaultRequestHeaders.Add("Authorization", token);

        AttachmentRetryOptions = attachmentRetryOptions ?? new AttachmentRetryOptions();
    }

    /// <inheritdoc/>
    public async Task<T> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        for (var rateLimitAttempt = 0; ; rateLimitAttempt++)
        {
            var requestUri = new Uri(HttpClient.BaseAddress!, endpoint);
            using var request = new HttpRequestMessage(method, requestUri);

            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod_Patch))
            {
                var json = JsonSerializer.Serialize(data, SerializeOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            // Rate Limiting: автоматически ждём и повторяем при HTTP 429
            if (response.StatusCode == (HttpStatusCode)429 && rateLimitAttempt < MaxRateLimitRetries)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? DefaultRateLimitDelay;
                if (retryAfter > MaxRetryAfterDelay)
                    throw new MaxBotClientException($"Сервер запросил недопустимо длительную задержку Retry-After: {retryAfter.TotalSeconds}с", response.StatusCode);

                await Task.Delay(retryAfter, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MaxBotClientException(
                    $"HTTP {response.StatusCode}: {responseContent}: {request.Method} {request.RequestUri}",
                    response.StatusCode);
            }

            return JsonSerializer.Deserialize<T>(responseContent, DeserializeOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ");
        }
    }

    /// <inheritdoc/>
    public async Task<T> RetryWhileAttachmentNotReadyAsync<T>(
        Func<Task<T>> action,
        AttachmentRetryOptions? retryOptions,
        CancellationToken cancellationToken)
    {
        var options = retryOptions ?? AttachmentRetryOptions;
        var maxAttempts = Math.Max(1, options.MaxAttempts);

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (MaxBotClientException ex) when (ex.IsAttachmentNotReady && attempt < maxAttempts - 1)
            {
                await Task.Delay(options.RetryDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> PostMultipartAsync(
        Uri uploadUri,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(uploadUri);
        ArgumentNullException_ThrowIfNull(content);

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUri)
        {
            Content = content
        };

        return await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Формирует строку URL-параметров запроса из словаря ключ-значение.
    /// </summary>
    /// <param name="queryParams">Словарь query-параметров.</param>
    /// <returns>Строка query-параметров, начинающаяся с '?', либо пустая строка.</returns>
    internal static string BuildQueryString(Dictionary<string, string> queryParams)
    {
        if (queryParams.Count == 0)
            return string.Empty;

        return "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility_UrlEncode(kvp.Value)}"));
    }

    /// <summary>
    /// Освобождает ресурсы сетевого клиента.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (OwnsHttpClient)
            HttpClient.Dispose();
    }
}
