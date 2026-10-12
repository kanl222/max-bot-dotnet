using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class ChatsModule : IChatsModule
{
    private readonly IMaxApiTransport _core;

    internal ChatsModule(IMaxApiTransport core) => _core = core;

    public async Task<GetChatsResponse> GetChatsAsync(
        GetChatsRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>();

        if (request.Count.HasValue)
            queryParams["count"] = request.Count.Value.ToString();

        if (request.Marker.HasValue)
            queryParams["marker"] = request.Marker.Value.ToString();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<GetChatsResponse>(
            HttpMethod.Get, $"/chats{queryString}", null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat> GetChatByIdAsync(long chatId, CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<Chat>(
            HttpMethod.Get, $"/chats/{chatId}", null, cancellationToken).ConfigureAwait(false);

    public async Task<Chat> UpdateChatAsync(
        long chatId,
        UpdateChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<Chat>(
            HttpMethod_Patch, $"/chats/{chatId}", request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BaseResponse> DeleteChatAsync(long chatId, CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/chats/{chatId}", null, cancellationToken).ConfigureAwait(false);

    public async Task<BaseResponse> SendChatActionAsync(
        long chatId,
        SendChatActionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Post, $"/chats/{chatId}/actions", request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GetChatPinnedMessageResponse> GetChatPinnedMessageAsync(
        long chatId,
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<GetChatPinnedMessageResponse>(
            HttpMethod.Get, $"/chats/{chatId}/pin", null, cancellationToken).ConfigureAwait(false);

    public async Task<BaseResponse> PinChatMessageAsync(
        long chatId,
        PinChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Put, $"/chats/{chatId}/pin", request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BaseResponse> UnpinChatMessageAsync(
        long chatId,
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/chats/{chatId}/pin", null, cancellationToken).ConfigureAwait(false);
}
