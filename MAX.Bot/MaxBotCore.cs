using System.Net;
using MAX.Bot.Exceptions;
using MAX.Bot.Interfaces.Models.Attachment;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot;

/// <summary>
/// Внутренний HTTP-слой, разделяемый всеми модулями.
/// Не предназначен для прямого использования — используйте <see cref="MaxBotClient"/>.
/// </summary>
internal sealed class MaxBotCore : IDisposable
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

    internal readonly HttpClient HttpClient;
    internal readonly AttachmentRetryOptions AttachmentRetryOptions;
    internal readonly bool OwnsHttpClient;

    private bool _disposed;

    /// <summary>
    /// Конструктор с внешним <see cref="System.Net.Http.HttpClient"/> (DI).
    /// Не мутирует переданный клиент — BaseAddress и заголовки должны быть настроены снаружи.
    /// </summary>
    internal MaxBotCore(
        string token,
        HttpClient httpClient,
        AttachmentRetryOptions? attachmentRetryOptions)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        OwnsHttpClient = false;

        // Не перезаписываем BaseAddress и заголовки — они уже настроены через IHttpClientFactory.
        // Если BaseAddress не задан — устанавливаем дефолт.
        HttpClient.BaseAddress ??= new Uri(MaxBotClient.BaseUrl);

        if (!HttpClient.DefaultRequestHeaders.Contains("Authorization"))
            HttpClient.DefaultRequestHeaders.Add("Authorization", token);

        AttachmentRetryOptions = attachmentRetryOptions ?? new AttachmentRetryOptions();
    }

    /// <summary>
    /// Конструктор со встроенным <see cref="System.Net.Http.HttpClient"/>.
    /// </summary>
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

    internal async Task<T> SendRequestAsync<T>(
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

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Rate Limiting: автоматически ждём и повторяем при HTTP 429
            if (response.StatusCode == (HttpStatusCode)429 && rateLimitAttempt < MaxRateLimitRetries)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? DefaultRateLimitDelay;
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new MaxBotClientException(
                    $"HTTP {response.StatusCode}: {responseContent}: {request.Method} {request.RequestUri} {request.Content?.ReadAsStringAsync(cancellationToken).Result}",
                    response.StatusCode);
            }

            return JsonSerializer.Deserialize<T>(responseContent, DeserializeOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ");
        }
    }

    internal async Task<T> RetryWhileAttachmentNotReadyAsync<T>(
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
                return await action();
            }
            catch (MaxBotClientException ex) when (ex.IsAttachmentNotReady && attempt < maxAttempts - 1)
            {
                await Task.Delay(options.RetryDelay, cancellationToken);
            }
        }
    }

    internal static string BuildQueryString(Dictionary<string, string> queryParams)
    {
        if (queryParams.Count == 0)
            return string.Empty;

        return "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility_UrlEncode(kvp.Value)}"));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (OwnsHttpClient)
            HttpClient.Dispose();
    }
}
