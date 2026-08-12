using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class MembersModule : IMembersModule
{
    private readonly MaxBotCore _core;

    internal MembersModule(MaxBotCore core) => _core = core;

    public async Task<ChatMember> GetChatMembershipAsync(
        long chatId,
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<ChatMember>(
            HttpMethod.Get, $"/chats/{chatId}/members/me", null, cancellationToken);

    public async Task<BaseResponse> LeaveChatAsync(
        long chatId,
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/chats/{chatId}/members/me", null, cancellationToken);

    public async Task<GetChatMembersResponse> GetChatAdminsAsync(
        long chatId,
        CancellationToken cancellationToken = default)
        => await _core.SendRequestAsync<GetChatMembersResponse>(
            HttpMethod.Get, $"/chats/{chatId}/members/admins", null, cancellationToken);

    public async Task<BaseResponse> AddChatAdminsAsync(
        long chatId,
        AddChatAdminsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);
        request.Validate();

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Post, $"/chats/{chatId}/members/admins", request, cancellationToken);
    }

    public async Task<BaseResponse> RemoveChatAdminAsync(
        long chatId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("ID пользователя должен быть положительным числом.", nameof(userId));

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/chats/{chatId}/members/admins/{userId}", null, cancellationToken);
    }

    public async Task<GetChatMembersResponse> GetChatMembersAsync(
        GetChatMembersRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);

        var queryParams = new Dictionary<string, string>();

        if (request.UserIds != null)
            queryParams["user_ids"] = string.Join(",", request.UserIds);

        if (request.Marker.HasValue)
            queryParams["marker"] = request.Marker.Value.ToString();

        if (request.Count.HasValue)
            queryParams["count"] = request.Count.Value.ToString();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<GetChatMembersResponse>(
            HttpMethod.Get, $"/chats/{request.ChatId}/members{queryString}", null, cancellationToken);
    }

    public async Task<BaseResponse> DeleteChatMemberAsync(
        DeleteChatMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);

        var queryParams = new Dictionary<string, string>
        {
            ["user_id"] = request.UserId.ToString()
        };

        if (request.Block.HasValue)
            queryParams["block"] = request.Block.Value.ToString().ToLower();

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Delete, $"/chats/{request.ChatId}/members{queryString}", null, cancellationToken);
    }

    public async Task<BaseResponse> AddChatMemberAsync(
        AddChatMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(request);

        return await _core.SendRequestAsync<BaseResponse>(
            HttpMethod.Post, $"/chats/{request.ChatId}/members", request, cancellationToken);
    }
}
