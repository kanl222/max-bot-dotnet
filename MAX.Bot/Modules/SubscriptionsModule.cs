using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class SubscriptionsModule : ISubscriptionsModule
{
    private readonly MaxBotCore _core;

    internal SubscriptionsModule(MaxBotCore core) => _core = core;

    public async Task<GetSubscriptionsResponse> GetSubscriptionsAsync(
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<GetSubscriptionsResponse>(
            HttpMethod.Get, "/subscriptions", null, cancellationToken);

    public async Task<BaseResponse> SubscribeAsync(
        SubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Post, "/subscriptions", request, cancellationToken);
    }

    public async Task<BaseResponse> UnsubscribeAsync(
        DeleteSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete,
            $"/subscriptions?url={HttpUtility_UrlEncode(request.Url)}",
            null,
            cancellationToken);
    }
}
