using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для управления участниками и администраторами чатов.
/// </summary>
public interface IMembersModule
{
    /// <summary>
    /// Получить информацию о членстве текущего бота в групповом чате.
    /// </summary>
    Task<ChatMember> GetChatMembershipAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить текущего бота из группового чата.
    /// </summary>
    Task<BaseResponse> LeaveChatAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список администраторов группового чата.
    /// </summary>
    Task<GetChatMembersResponse> GetChatAdminsAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Назначить администраторов группового чата.
    /// </summary>
    Task<BaseResponse> AddChatAdminsAsync(
        long chatId,
        AddChatAdminsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить права администратора в групповом чате.
    /// </summary>
    Task<BaseResponse> RemoveChatAdminAsync(
        long chatId,
        long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить участников чата.
    /// </summary>
    Task<GetChatMembersResponse> GetChatMembersAsync(
        GetChatMembersRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить участника из чата.
    /// </summary>
    Task<BaseResponse> DeleteChatMemberAsync(
        DeleteChatMemberRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить участника в чат.
    /// </summary>
    Task<BaseResponse> AddChatMemberAsync(
        AddChatMemberRequest request,
        CancellationToken cancellationToken = default);
}
