using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Response;
using Attachment = MAX.Bot.Interfaces.Models.Request.Message.Attachment.Attachment;

namespace MAX.Bot.Extensions;

/// <summary>
/// Методы расширения для <see cref="IMaxBotClient"/>, предоставляющие быстрый доступ к методам всех модулей.
/// </summary>
public static class MaxBotClientExtensions
{
    #region Messages

    /// <inheritdoc cref="IMessagesModule.SendMessageAsync"/>
    public static Task<SendMessageResponse> SendMessageAsync(
        this IMaxBotClient client,
        SendMessageRequest request,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
        => client.Messages.SendMessageAsync(request, retryOptions, cancellationToken);

    /// <inheritdoc cref="IMessagesModule.AnswerCallbackAsync"/>
    public static Task<BaseResponse> AnswerCallbackAsync(
        this IMaxBotClient client,
        AnswerCallbackRequest request,
        CancellationToken cancellationToken = default)
        => client.Messages.AnswerCallbackAsync(request, cancellationToken);

    /// <inheritdoc cref="IMessagesModule.GetMessagesAsync"/>
    public static Task<GetMessagesResponse> GetMessagesAsync(
        this IMaxBotClient client,
        GetMessagesRequest request,
        CancellationToken cancellationToken = default)
        => client.Messages.GetMessagesAsync(request, cancellationToken);

    /// <inheritdoc cref="IMessagesModule.GetMessageByIdAsync"/>
    public static Task<Message> GetMessageByIdAsync(
        this IMaxBotClient client,
        string messageId,
        CancellationToken cancellationToken = default)
        => client.Messages.GetMessageByIdAsync(messageId, cancellationToken);

    /// <inheritdoc cref="IMessagesModule.EditMessageByIdAsync"/>
    public static Task<BaseResponse> EditMessageByIdAsync(
        this IMaxBotClient client,
        string messageId,
        SendMessageRequest messageRequest,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
        => client.Messages.EditMessageByIdAsync(messageId, messageRequest, retryOptions, cancellationToken);

    /// <inheritdoc cref="IMessagesModule.DeleteMessageByIdAsync"/>
    public static Task<BaseResponse> DeleteMessageByIdAsync(
        this IMaxBotClient client,
        string messageId,
        CancellationToken cancellationToken = default)
        => client.Messages.DeleteMessageByIdAsync(messageId, cancellationToken);

    #endregion

    #region Chats

    /// <inheritdoc cref="IChatsModule.GetChatsAsync"/>
    public static Task<GetChatsResponse> GetChatsAsync(
        this IMaxBotClient client,
        GetChatsRequest request,
        CancellationToken cancellationToken = default)
        => client.Chats.GetChatsAsync(request, cancellationToken);

    /// <inheritdoc cref="IChatsModule.GetChatByIdAsync"/>
    public static Task<Chat> GetChatByIdAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Chats.GetChatByIdAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IChatsModule.UpdateChatAsync"/>
    public static Task<Chat> UpdateChatAsync(
        this IMaxBotClient client,
        long chatId,
        UpdateChatRequest request,
        CancellationToken cancellationToken = default)
        => client.Chats.UpdateChatAsync(chatId, request, cancellationToken);

    /// <inheritdoc cref="IChatsModule.DeleteChatAsync"/>
    public static Task<BaseResponse> DeleteChatAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Chats.DeleteChatAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IChatsModule.SendChatActionAsync"/>
    public static Task<BaseResponse> SendChatActionAsync(
        this IMaxBotClient client,
        long chatId,
        SendChatActionRequest request,
        CancellationToken cancellationToken = default)
        => client.Chats.SendChatActionAsync(chatId, request, cancellationToken);

    /// <inheritdoc cref="IChatsModule.GetChatPinnedMessageAsync"/>
    public static Task<GetChatPinnedMessageResponse> GetChatPinnedMessageAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Chats.GetChatPinnedMessageAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IChatsModule.PinChatMessageAsync"/>
    public static Task<BaseResponse> PinChatMessageAsync(
        this IMaxBotClient client,
        long chatId,
        PinChatMessageRequest request,
        CancellationToken cancellationToken = default)
        => client.Chats.PinChatMessageAsync(chatId, request, cancellationToken);

    /// <inheritdoc cref="IChatsModule.UnpinChatMessageAsync"/>
    public static Task<BaseResponse> UnpinChatMessageAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Chats.UnpinChatMessageAsync(chatId, cancellationToken);

    #endregion

    #region Members

    /// <inheritdoc cref="IMembersModule.GetChatMembershipAsync"/>
    public static Task<ChatMember> GetChatMembershipAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Members.GetChatMembershipAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IMembersModule.LeaveChatAsync"/>
    public static Task<BaseResponse> LeaveChatAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Members.LeaveChatAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IMembersModule.GetChatAdminsAsync"/>
    public static Task<GetChatMembersResponse> GetChatAdminsAsync(
        this IMaxBotClient client,
        long chatId,
        CancellationToken cancellationToken = default)
        => client.Members.GetChatAdminsAsync(chatId, cancellationToken);

    /// <inheritdoc cref="IMembersModule.AddChatAdminsAsync"/>
    public static Task<BaseResponse> AddChatAdminsAsync(
        this IMaxBotClient client,
        long chatId,
        AddChatAdminsRequest request,
        CancellationToken cancellationToken = default)
        => client.Members.AddChatAdminsAsync(chatId, request, cancellationToken);

    /// <inheritdoc cref="IMembersModule.RemoveChatAdminAsync"/>
    public static Task<BaseResponse> RemoveChatAdminAsync(
        this IMaxBotClient client,
        long chatId,
        long userId,
        CancellationToken cancellationToken = default)
        => client.Members.RemoveChatAdminAsync(chatId, userId, cancellationToken);

    /// <inheritdoc cref="IMembersModule.GetChatMembersAsync"/>
    public static Task<GetChatMembersResponse> GetChatMembersAsync(
        this IMaxBotClient client,
        GetChatMembersRequest request,
        CancellationToken cancellationToken = default)
        => client.Members.GetChatMembersAsync(request, cancellationToken);

    /// <inheritdoc cref="IMembersModule.DeleteChatMemberAsync"/>
    public static Task<BaseResponse> DeleteChatMemberAsync(
        this IMaxBotClient client,
        DeleteChatMemberRequest request,
        CancellationToken cancellationToken = default)
        => client.Members.DeleteChatMemberAsync(request, cancellationToken);

    /// <inheritdoc cref="IMembersModule.AddChatMemberAsync"/>
    public static Task<BaseResponse> AddChatMemberAsync(
        this IMaxBotClient client,
        AddChatMemberRequest request,
        CancellationToken cancellationToken = default)
        => client.Members.AddChatMemberAsync(request, cancellationToken);

    #endregion

    #region Attachments

    /// <inheritdoc cref="IAttachmentsModule.GetVideoAsync"/>
    public static Task<VideoInfoResponse> GetVideoAsync(
        this IMaxBotClient client,
        string videoToken,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default)
        => client.Attachments.GetVideoAsync(videoToken, retryOptions, cancellationToken);

    /// <inheritdoc cref="IAttachmentsModule.TryGetAttachmentDownloadUrlAsync"/>
    public static Task<string?> TryGetAttachmentDownloadUrlAsync(
        this IMaxBotClient client,
        Attachment attachment,
        VideoQuality videoQuality = VideoQuality.Mp4_480,
        CancellationToken cancellationToken = default)
        => client.Attachments.TryGetAttachmentDownloadUrlAsync(attachment, videoQuality, cancellationToken);

    /// <inheritdoc cref="IAttachmentsModule.DownloadAttachmentAsync"/>
    public static Task<AttachmentDownloadResult> DownloadAttachmentAsync(
        this IMaxBotClient client,
        Attachment attachment,
        DownloadAttachmentOptions? options = null,
        CancellationToken cancellationToken = default)
        => client.Attachments.DownloadAttachmentAsync(attachment, options, cancellationToken);

    /// <inheritdoc cref="IAttachmentsModule.UploadAsync"/>
    public static Task<string> UploadAsync(
        this IMaxBotClient client,
        UploadRequest request,
        CancellationToken cancellationToken = default)
        => client.Attachments.UploadAsync(request, cancellationToken);

    /// <inheritdoc cref="IAttachmentsModule.UploadsAsync"/>
    [Obsolete("Используйте UploadAsync вместо UploadsAsync.")]
    public static Task<string> UploadsAsync(
        this IMaxBotClient client,
        UploadRequest request,
        CancellationToken cancellationToken = default)
        => client.Attachments.UploadsAsync(request, cancellationToken);

    #endregion

    #region Updates

    /// <inheritdoc cref="IUpdatesModule.GetUpdatesAsync"/>
    public static Task<GetUpdatesResponse> GetUpdatesAsync(
        this IMaxBotClient client,
        GetUpdatesRequest request,
        CancellationToken cancellationToken = default)
        => client.Updates.GetUpdatesAsync(request, cancellationToken);

    /// <inheritdoc cref="IUpdatesModule.PollUpdatesWithCallback"/>
    public static Task PollUpdatesWithCallback(
        this IMaxBotClient client,
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        CancellationToken cancellationToken = default)
        => client.Updates.PollUpdatesWithCallback(callback, limit, timeout, marker, types, cancellationToken);

    #endregion

    #region Subscriptions

    /// <inheritdoc cref="ISubscriptionsModule.GetSubscriptionsAsync"/>
    public static Task<GetSubscriptionsResponse> GetSubscriptionsAsync(
        this IMaxBotClient client,
        CancellationToken cancellationToken = default)
        => client.Subscriptions.GetSubscriptionsAsync(cancellationToken);

    /// <inheritdoc cref="ISubscriptionsModule.SubscribeAsync"/>
    public static Task<BaseResponse> SubscribeAsync(
        this IMaxBotClient client,
        SubscriptionRequest request,
        CancellationToken cancellationToken = default)
        => client.Subscriptions.SubscribeAsync(request, cancellationToken);

    /// <inheritdoc cref="ISubscriptionsModule.UnsubscribeAsync"/>
    public static Task<BaseResponse> UnsubscribeAsync(
        this IMaxBotClient client,
        DeleteSubscriptionRequest request,
        CancellationToken cancellationToken = default)
        => client.Subscriptions.UnsubscribeAsync(request, cancellationToken);

    #endregion
}
