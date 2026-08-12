using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для управления Webhook-подписками.
/// </summary>
public interface ISubscriptionsModule
{
    /// <summary>
    /// Получить все подписки через Webhook.
    /// </summary>
    Task<GetSubscriptionsResponse> GetSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Подписаться на обновления о новых событиях через Webhook.
    /// </summary>
    Task<BaseResponse> SubscribeAsync(
        SubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отписаться от обновлений о новых событиях через Webhook.
    /// </summary>
    Task<BaseResponse> UnsubscribeAsync(
        DeleteSubscriptionRequest request,
        CancellationToken cancellationToken = default);
}
